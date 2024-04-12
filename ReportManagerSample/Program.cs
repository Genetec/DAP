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

            var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);
            if (state == ConnectionStateCode.Success)
            {
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.AccessPoint);
                query.Query();
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
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

        async Task<IEnumerable<VideoFileInfo>> GetVideoFile(Engine engine, Guid camera)
        {
            var query = (VideoFileQuery)engine.ReportManager.CreateReportQuery(ReportType.VideoFile);
            query.Cameras.Add(camera);

            QueryCompletedEventArgs result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            return result.Data.AsEnumerable().Select(row => new VideoFileInfo
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
            });
        }

        async Task<List<Guid>> FindCardholdersByCustomFieldValue(Engine engine, string customFieldName, object customFieldValue, FieldRangeType condition)
        {
            var config = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
            var customField = config.CustomFieldService.GetCustomField(customFieldName, EntityType.Cardholder);

            var query = (CardholderConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
            query.DownloadAllRelatedData = true;
            query.CustomFields.Add(new CustomFieldFilter(customField, customFieldValue, condition));
            query.PageSize = 500;
            query.Page = 1;

            var cardholders = new List<Guid>();

            while (true)
            {
                QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                cardholders.AddRange(args.Data.AsEnumerable().Take(query.PageSize).Select(row => row.Field<Guid>(nameof(Guid))));

                if (args.Data.Rows.Count <= query.PageSize)
                    break;

                query.Page++;
            }

            return cardholders;
        }

        /// <summary>
        /// Find the creation and last modification timestamp of a give entity.
        /// </summary>
        static async Task<(DateTime CreationTime, Guid CreatedBy, DateTime LastModificationTime, Guid LastModifiedBy)> GetCreattionAndModificationDate(Engine engine, Entity entity)
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

        static async Task<IEnumerable<(DateTime date, Guid source, EventType eventType)>> GetDoorActivity(Engine engine, IEnumerable<Guid> doorGuids, DateTime startDate, DateTime endDate)
        {
            var query = (DoorActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.DoorActivity);

            if (doorGuids.Any())
            {
                foreach (Guid doorGuid in doorGuids)
                {
                    query.Doors.Add(doorGuid);
                }
            }
            else
            {
                // Retrieve events for all doors
                query.Doors.Add(SystemConfiguration.SystemConfigurationGuid);
            }


            query.TimeRange.SetTimeRange(startDate, endDate);
            query.MaximumResultCount = 100;
            query.SortOrder = OrderByType.Ascending;

            var results = new List<(DateTime date, Guid source, EventType eventType)>();

            while (true)
            {
                QueryCompletedEventArgs result = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

                var records = result.Data.AsEnumerable().Take(query.MaximumResultCount).Select(row =>
                {
                    var date = row.Field<DateTime>(AccessControlReportQuery.TimestampColumnName);
                    var source = row.Field<Guid>(AccessControlReportQuery.SourceGuidColumnName);
                    var eventType = row.Field<EventType>(AccessControlReportQuery.EventTypeColumnName);

                    return (date, source, eventType);
                }).ToList();

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

            return results;
        }

        static async Task<List<AccessControlRawEvent>> RetrieveOfflineEvents(Engine engine, DateTime startDate, DateTime endDate, IEnumerable<EventType> eventTypes)
        {
            var list = new List<AccessControlRawEvent>();

            var query = (AccessControlRawEventQuery)engine.ReportManager.CreateReportQuery(ReportType.AccessControlRawEvent);
            query.MaximumResultCount = 1000;
            query.InsertionStartTimeUtc = startDate.ToUniversalTime();
            query.InsertionEndTimeUtc = endDate.ToUniversalTime();

            foreach (var eventType in eventTypes)
            {
                query.EventTypeFilter.Add(eventType);
            }

            do
            {
                var args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

                var results = args.Data.AsEnumerable().Select(CreateEvent).ToList();
                if (results.Count == 0)
                    break;

                list.AddRange(results.Where(rawEvent => rawEvent.OccurrencePeriod == OfflinePeriodType.Offline || rawEvent.OccurrencePeriod == OfflinePeriodType.OfflineAlarmPeriod));

                if (results.Count < query.MaximumResultCount)
                {
                    break;
                }

                query.StartingAfterIndexes.Clear();
                foreach (var index in results.GroupBy(@event => @event.AccessManager).Select(group => new RawEventIndex { AccessManager = group.Key, Position = group.Max(rawEvent => rawEvent.EventPosition) }))
                {
                    query.StartingAfterIndexes.Add(index);
                }

            } while (true);

            return list;

            AccessControlRawEvent CreateEvent(DataRow row) => new AccessControlRawEvent
            {
                SourceGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.SourceGuid),
                AccessPointGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.AccessPointGuid),
                CredentialGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.CredentialGuid),
                CardholderGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.CardholderGuid),
                EventType = row.Field<EventType>(AccessControlRawEventQuery.TableColumns.EventType),
                InsertionTimestamp = DateTime.SpecifyKind(row.Field<DateTime>(AccessControlRawEventQuery.TableColumns.InsertionTimestamp), DateTimeKind.Utc),
                Timestamp = DateTime.SpecifyKind(row.Field<DateTime>(AccessControlRawEventQuery.TableColumns.Timestamp), DateTimeKind.Utc),
                EventPosition = row.Field<long>(AccessControlRawEventQuery.TableColumns.Position),
                EventTimeZone = row.Field<string>(AccessControlRawEventQuery.TableColumns.TimeZone),
                AccessManager = row.Field<Guid>(AccessControlRawEventQuery.TableColumns.AccessManagerGuid),
                OccurrencePeriod = row.Field<OfflinePeriodType>(AccessControlRawEventQuery.TableColumns.OccurrencePeriod)
            };
        }

        static async Task<List<AccessControlRawEvent>> RetrieveEvents(Engine engine, IEnumerable<EventType> eventTypes, IEnumerable<RawEventIndex> indexes)
        {
            var list = new List<AccessControlRawEvent>();

            var query = (AccessControlRawEventQuery)engine.ReportManager.CreateReportQuery(ReportType.AccessControlRawEvent);
            query.MaximumResultCount = 1000;

            foreach (var index in indexes)
            {
                query.StartingAfterIndexes.Add(index);
            }

            foreach (var eventType in eventTypes)
            {
                query.EventTypeFilter.Add(eventType);
            }

            do
            {
                var args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

                var results = args.Data.AsEnumerable().Select(CreateEvent).ToList();
                if (results.Count == 0)
                    break;

                list.AddRange(results);

                if (results.Count < query.MaximumResultCount)
                {
                    break;
                }

                query.StartingAfterIndexes.Clear();
                foreach (var index in results.GroupBy(@event => @event.AccessManager).Select(group => new RawEventIndex { AccessManager = group.Key, Position = group.Max(rawEvent => rawEvent.EventPosition) }))
                {
                    query.StartingAfterIndexes.Add(index);
                }

            } while (true);

            return list;

            AccessControlRawEvent CreateEvent(DataRow row) => new AccessControlRawEvent
            {
                SourceGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.SourceGuid),
                AccessPointGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.AccessPointGuid),
                CredentialGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.CredentialGuid),
                CardholderGuid = row.Field<Guid?>(AccessControlRawEventQuery.TableColumns.CardholderGuid),
                EventType = row.Field<EventType>(AccessControlRawEventQuery.TableColumns.EventType),
                InsertionTimestamp = DateTime.SpecifyKind(row.Field<DateTime>(AccessControlRawEventQuery.TableColumns.InsertionTimestamp), DateTimeKind.Utc),
                Timestamp = DateTime.SpecifyKind(row.Field<DateTime>(AccessControlRawEventQuery.TableColumns.Timestamp), DateTimeKind.Utc),
                EventPosition = row.Field<long>(AccessControlRawEventQuery.TableColumns.Position),
                EventTimeZone = row.Field<string>(AccessControlRawEventQuery.TableColumns.TimeZone),
                AccessManager = row.Field<Guid>(AccessControlRawEventQuery.TableColumns.AccessManagerGuid),
                OccurrencePeriod = row.Field<OfflinePeriodType>(AccessControlRawEventQuery.TableColumns.OccurrencePeriod)
            };
        }
    }

    class AccessControlRawEvent
    {
        public Guid? SourceGuid { get; set; }

        public Guid? AccessPointGuid { get; set; }

        public Guid? CredentialGuid { get; set; }

        public OfflinePeriodType OccurrencePeriod { get; set; }

        public Guid AccessManager { get; set; }

        public string EventTimeZone { get; set; }

        public long EventPosition { get; set; }

        public DateTime InsertionTimestamp { get; set; }

        public DateTime Timestamp { get; set; }

        public EventType EventType { get; set; }

        public Guid? CardholderGuid { get; set; }
    }
}
