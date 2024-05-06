// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk.Queries.AccessControl;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Queries;
    using Sdk.Queries.Video;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);
            if (state == ConnectionStateCode.Success)
            {
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        async IAsyncEnumerable<VideoFileInfo> GetVideoFile(Engine engine, Guid camera)
        {
            var query = (VideoFileQuery)engine.ReportManager.CreateReportQuery(ReportType.VideoFile);
            query.Cameras.Add(camera);

            QueryCompletedEventArgs result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            foreach (var item in result.Data.AsEnumerable().Select(CreateVideoFileInfo))
            {
                yield return item;
            }

            VideoFileInfo CreateVideoFileInfo(DataRow row) => new VideoFileInfo
            {
                CameraGuid = row.Field<string>(VideoFileQuery.CameraGuidColumnName),
                Drive = row.Field<string>(VideoFileQuery.DriveColumnName),
                EndTime = row.Field<DateTime>(VideoFileQuery.EndTimeColumnName),
                Error = row.Field<string>(VideoFileQuery.ErrorColumnName),
                FilePath = row.Field<string>(VideoFileQuery.FilePathColumnName),
                FileSize = row.Field<long>(VideoFileQuery.FileSizeColumnName),
                InfiniteProtection = row.Field<bool>(VideoFileQuery.InfiniteProtectionColumnName),
                MetadataPath = row.Field<string>(VideoFileQuery.MetadataPathColumnName),
                ProtectionEndDateTime = row.Field<DateTime?>(VideoFileQuery.ProtectionEndDateTimeColumnName),
                ProtectionStatus = row.Field<string>(VideoFileQuery.ProtectionStatusColumnName),
                StartTime = row.Field<DateTime>(VideoFileQuery.StartTimeColumnName)
            };
        }

        async IAsyncEnumerable<Guid> FindCardholdersByCustomFieldValue(Engine engine, string customFieldName, object customFieldValue, FieldRangeType condition)
        {
            var config = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
            var customField = config.CustomFieldService.GetCustomField(customFieldName, EntityType.Cardholder);

            var query = (CardholderConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
            query.DownloadAllRelatedData = true;
            query.CustomFields.Add(new CustomFieldFilter(customField, customFieldValue, condition));
            query.PageSize = 500;
            query.Page = 1;

            while (true)
            {
                QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                foreach (var guid in args.Data.AsEnumerable().Take(query.PageSize).Select(row => row.Field<Guid>(nameof(Guid))))
                {
                    yield return guid;
                }

                if (args.Data.Rows.Count <= query.PageSize)
                    break;

                query.Page++;
            }
        }

        /// <summary>
        /// Find the creation and last modification timestamp of a give entity.
        /// </summary>
        static async Task<(DateTime CreationTime, Guid CreatedBy, DateTime LastModificationTime, Guid LastModifiedBy)> GetCreationAndModificationDate(Engine engine, Entity entity)
        {
            var first = await Query(OrderByType.Ascending);
            var last = await Query(OrderByType.Descending);

            return (CreationTime: first.ModificationTime, CreatedBy: first.ModifiedBy, LastModificationTime: last.ModificationTime, LastModifiedBy: last.ModifiedBy);

            async Task<(DateTime ModificationTime, Guid ModifiedBy)> Query(OrderByType sortOrder)
            {
                var query = (AuditTrailQuery)engine.ReportManager.CreateReportQuery(ReportType.AuditTrailsReport);
                query.QueryEntities.Add(entity.Guid);
                query.MaximumResultCount = 1;
                query.SortOrder = sortOrder;

                var result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

                return result.Data.AsEnumerable()
                    .Select(row => (ModificationTime: row.Field<DateTime>(AuditTrailQuery.ModificationTimestampColumnName), ModifiedBy: row.Field<Guid>(AuditTrailQuery.ModifiedByColumnName)))
                    .FirstOrDefault();
            }
        }

        static async IAsyncEnumerable<AccessControlActivity> GetDoorActivity(Engine engine, IEnumerable<Guid> doorGuids, DateTime startDate, DateTime endDate)
        {
            var query = (DoorActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.DoorActivity);

            if (doorGuids.Any())
            {
                query.Doors.AddRange(doorGuids);
            }
            else
            {
                // Retrieve events for all doors
                query.Doors.Add(SystemConfiguration.SystemConfigurationGuid);
            }

            query.TimeRange.SetTimeRange(startDate, endDate);
            query.MaximumResultCount = 100;
            query.SortOrder = OrderByType.Ascending;

      

            while (true)
            {
                QueryCompletedEventArgs result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

                AccessControlActivity Selector(DataRow row)
                {
                    AccessControlActivity activity = new AccessControlActivity();

                    activity.Timestamp = row.Field<DateTime>(AccessControlReportQuery.TimestampColumnName);
                    activity.Timestamp = row.Field<Guid>(AccessControlReportQuery.SourceGuidColumnName);
                    activity.Timestamp  = row.Field<EventType>(AccessControlReportQuery.EventTypeColumnName);

                }

                var records = result.Data.AsEnumerable().Take(query.MaximumResultCount).Select(Selector).ToList();

                if (!records.Any())
                {
                    break; // Exit the loop if no records are returned
                }

                results.AddRange(records);

                // Adjust the startDate for the next query to just after the last fetched record's timestamp
                query.TimeRange.SetTimeRange(records.Max(r => r.date), endDate);

                if (records.Count < query.MaximumResultCount)
                {
                    break; // Exit if fewer results than the maximum are returned, indicating all data has been fetched
                }
            }
        }

        private async IAsyncEnumerable<AccessControlEvent> GetAccessControlEvents(Engine engine, DateTime startDate, DateTime endDate, IEnumerable<EventType> eventTypes, IEnumerable<AccessManagerRole> accessManagers)
        {
            var query = (AccessControlRawEventQuery)engine.ReportManager.CreateReportQuery(ReportType.AccessControlRawEvent);
            query.MaximumResultCount = 2000;

            if (startDate > DateTime.MinValue)
            {
                query.InsertionStartTimeUtc = startDate.ToUniversalTime();
            }

            if (endDate > DateTime.MinValue)
            {
                query.InsertionEndTimeUtc = endDate.ToUniversalTime();
            }

            query.EventTypeFilter.AddRange(eventTypes);

            foreach (var accessManager in accessManagers)
            {
                query.StartingAfterIndexes.Add(new RawEventIndex { AccessManager = accessManager.Guid, Position = 0 });

                do
                {
                    var args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                    var results = args.Data.AsEnumerable().Take(query.MaximumResultCount).Select(CreateEvent).ToList();

                    foreach (var result in results)
                    {
                        yield return result;
                    }

                    if (results.Count < query.MaximumResultCount)
                    {
                        break;
                    }

                    query.StartingAfterIndexes.Clear();
                    query.StartingAfterIndexes.AddRange(results.GroupBy(@event => @event.AccessManagerGuid).Select(group => new RawEventIndex { AccessManager = group.Key, Position = group.Max(rawEvent => rawEvent.Position) }));

                } while (true);

                query.StartingAfterIndexes.Clear();
            }

            AccessControlEvent CreateEvent(DataRow row)
            {
                return new AccessControlEvent
                {
                    SourceGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.SourceGuid),
                    AccessPointGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.AccessPointGuid),
                    CredentialGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.CredentialGuid),
                    CardholderGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.CardholderGuid),
                    DeviceGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.DeviceGuid),
                    EventType = row.Field<EventType>(AccessControlRawEventQuery.TableColumns.EventType),
                    InsertionTimestamp = DateTime.SpecifyKind(row.Field<DateTime>(AccessControlRawEventQuery.TableColumns.InsertionTimestamp), DateTimeKind.Utc),
                    Timestamp = DateTime.SpecifyKind(row.Field<DateTime>(AccessControlRawEventQuery.TableColumns.Timestamp), DateTimeKind.Utc),
                    Position = row.Field<long>(AccessControlRawEventQuery.TableColumns.Position),
                    AccessPointGroupGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.AccessPointGroupGuid),
                    TimeZone = row.Field<string>(AccessControlRawEventQuery.TableColumns.TimeZone),
                    AccessManagerGuid = row.Field<Guid>(AccessControlRawEventQuery.TableColumns.AccessManagerGuid),
                    OccurrencePeriod = row.Field<OfflinePeriodType>(AccessControlRawEventQuery.TableColumns.OccurrencePeriod)
                };
            }
        }
    }

    public class VideoFileInfo
    {
        public string CameraGuid { get; set; }
        public string Drive { get; set; }
        public DateTime EndTime { get; set; }
        public string Error { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public bool InfiniteProtection { get; set; }
        public string MetadataPath { get; set; }
        public DateTime? ProtectionEndDateTime { get; set; }
        public string ProtectionStatus { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class AccessControlEvent
    {
        public Guid? SourceGuid { get; set; }

        public Guid? AccessPointGuid { get; set; }

        public Guid? CredentialGuid { get; set; }

        public OfflinePeriodType OccurrencePeriod { get; set; }

        public Guid AccessManagerGuid { get; set; }

        public string TimeZone { get; set; }

        public long Position { get; set; }

        public DateTime InsertionTimestamp { get; set; }

        public DateTime Timestamp { get; set; }

        public EventType EventType { get; set; }

        public Guid? CardholderGuid { get; set; }

        public Guid? DeviceGuid { get; set; }

        public Guid? AccessPointGroupGuid { get; set; }
    }
}
