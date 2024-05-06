// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.AccessControl
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Credentials;
    using Sdk.Diagnostics.Logging;
    using Sdk.Diagnostics.Logging.Core;
    using Sdk.Entities;
    using Sdk.Queries;
    using Properties;

    public class EventExtractor : IDisposable
    {
        private readonly IEmployeeNumberReader m_employeeNumberReader;
        private readonly IEngine m_engine;
        private readonly IEventRecordRepository m_eventRecordStore;
        private readonly Logger m_logger;

        public EventExtractor(IEngine engine, IEventRecordRepository eventRecordStore, IEmployeeNumberReader employeeNumberReader)
        {
            m_logger = Logger.CreateInstanceLogger(this);
            m_engine = engine;
            m_eventRecordStore = eventRecordStore;
            m_employeeNumberReader = employeeNumberReader;
        }

        public void Dispose()
        {
            m_logger.Dispose();
        }

        public async Task ExtractData(DateTime startDate, DateTime endDate, IEnumerable<EventType> eventTypes)
        {
            var customFieldService = ((SystemConfiguration)m_engine.GetEntity(SystemConfiguration.SystemConfigurationGuid)).CustomFieldService;

            if (!customFieldService.TryGetCustomField("Employee Number", EntityType.Cardholder, out var employeeNumberCustomField))
            {
                m_logger.TraceWarning("Employee Number custom field does not exists.");
                return;
            }

            if (!customFieldService.TryGetCustomField("Location", EntityType.Door, out var locationCustomField))
            {
                m_logger.TraceWarning("Location custom field does not exists");
                return;
            }

            await CheckAccessManagerRolesAndUnit();

            var employees = await m_employeeNumberReader.GetAllEmployeeNumbers();

            var loadedEntities = new HashSet<Guid>();

            await RetrieveEvents(startDate, endDate, eventTypes, m_engine.GetEntities(EntityType.Role).OfType<AccessManagerRole>())
                .Buffer(2000)
                .Select(ProcessRawEvent)
                .Merge(1)
                .Buffer(10000)
                .Where(buffer => buffer.Any())
                .SelectMany(data => m_eventRecordStore.InsertRecords(data).ToObservable())
                .LastOrDefaultAsync()
                .ToTask();

            IObservable<EventRecord> ProcessRawEvent(IList<AccessControlRawEvent> events)
            {
                return Observable.FromAsync(async () =>
                {
                    var relatedEntities = events.SelectMany(rawEvent => rawEvent.GetRelatedEntities()).Distinct().Except(loadedEntities).ToList();
                    if (relatedEntities.Any())
                    {
                        m_logger.TraceInformation($"Loading {relatedEntities.Count} entities...");
                        await m_engine.LoadEntities(relatedEntities);
                        loadedEntities.AddRange(relatedEntities);
                    }

                    return events.SelectMany(Convert).ToObservable();
                }).Merge();

                IEnumerable<EventRecord> Convert(AccessControlRawEvent rawEvent)
                {
                    if (!TryGetCardholder(rawEvent, out var cardholder))
                        yield break;

                    var employeeId = customFieldService.GetValue(employeeNumberCustomField, cardholder.Guid) as string;

                    if (Settings.Default.FilterEmployeeId && !employees.Contains(employeeId?.Trim()))
                        yield break;

                    var door = rawEvent.AccessPointGroupGuid.HasValue ? m_engine.GetEntity(rawEvent.AccessPointGroupGuid.Value) as Door : null;
                    if (door is null)
                    {
                        yield break;
                    }

                    var accessPoint = rawEvent.AccessPointGuid.HasValue ? m_engine.GetEntity(rawEvent.AccessPointGuid.Value) as AccessPoint : null;
                    if (accessPoint is null)
                    {
                        yield break;
                    }

                    var data = new EventRecord
                    {
                        FullName = $"{cardholder.FirstName} {cardholder.LastName}",
                        EMPLID = employeeId,
                        EventType = rawEvent.EventType.ToString(),
                        EventDateLocalTime = rawEvent.Timestamp.ToLocalTime(),
                        EventDateGMT = rawEvent.Timestamp.ToUniversalTime(),
                        CreatedDate = DateTime.UtcNow,
                        DoorName = door.Name,
                        SiteName = customFieldService.GetValue(locationCustomField, door.Guid) as string,
                        EventID = $"{(byte)rawEvent.EventType}_{GetAccessPointSide(accessPoint.AccessPointSide)}", 
                        AccessManager = m_engine.GetEntity(rawEvent.AccessManagerGuid) is AccessManagerRole accessManager ? accessManager.Name : null
                    };

                    if (rawEvent.CredentialGuid.HasValue)
                        data.BadgeId = GetBadgeId(rawEvent.CredentialGuid);

                    if (rawEvent.DeviceGuid.HasValue)
                        data.ReaderName = m_engine.GetEntity(rawEvent.DeviceGuid.GetValueOrDefault()) is Reader reader ? reader.Name : null;

                    var rules = accessPoint.AccessRules.Select(m_engine.GetEntity).OfType<AccessRule>().ToList();
                    if (Settings.Default.ApplyAccessRuleFilter)
                    {
                        const string profileName = "Attendance";
                        if (rules.Any(rule => rule.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase)))
                        {
                            data.ProfileName = profileName;
                        }
                        else
                        {
                            yield break;
                        }
                    }
                    else
                    {
                        data.ProfileName = string.Join(",", rules.Select(rule => rule.Name));
                    }

                    yield return data;
                }

                long GetBadgeId(Guid? credentialGuid)
                {
                    if (m_engine.GetEntity(credentialGuid.GetValueOrDefault()) is Credential credential)
                    {
                        switch (credential.Format)
                        {
                            case WiegandCorporate1000CredentialFormat format:
                                return format.CardId;

                            case WiegandCsn32CredentialFormat format:
                                return format.CardId;

                            case WiegandStandardCredentialFormat format:
                                return format.CardId;

                            case WiegandH10302CredentialFormat format:
                                return format.CardId;

                            case WiegandH10304CredentialFormat format:
                                return format.CardId;

                            case WiegandH10306CredentialFormat format:
                                return format.CardId;

                            case Wiegand48BitCorporate1000CredentialFormat format:
                                return format.CardId;
                        }
                    }

                    return 0;
                }

                bool TryGetCardholder(AccessControlRawEvent rawEvent, out Cardholder cardholder)
                {
                    cardholder = rawEvent.CardholderGuid.HasValue ? m_engine.GetEntity(rawEvent.CardholderGuid.Value) as Cardholder : null;
                    if (cardholder == null && rawEvent.SourceGuid.HasValue)
                        cardholder = m_engine.GetEntity(rawEvent.SourceGuid.Value) as Cardholder;

                    return cardholder != null;
                }

                AccessPointSide GetAccessPointSide(AccessPointSide side)
                {
                    return side switch
                    {
                        AccessPointSide.Alpha => AccessPointSide.In,
                        AccessPointSide.Omega => AccessPointSide.Out,
                        _ => side
                    };
                }

            }
        }

        private IObservable<AccessControlRawEvent> RetrieveEvents(DateTime startDate, DateTime endDate, IEnumerable<EventType> eventTypes, IEnumerable<AccessManagerRole> accessManagers)
        {
            return Observable.Create<AccessControlRawEvent>(async observer =>
            {
                m_logger.TraceInformation($"Local insertion date range from: {startDate:F} to {endDate:F}.");

                var query = (AccessControlRawEventQuery)m_engine.ReportManager.CreateReportQuery(ReportType.AccessControlRawEvent);
                query.MaximumResultCount = Settings.Default.MaximumResultCount;

                if (startDate > DateTime.MinValue)
                {
                    query.InsertionStartTimeUtc = startDate.ToUniversalTime();
                }

                if (endDate > DateTime.MinValue)
                {
                    query.InsertionEndTimeUtc = endDate.ToUniversalTime();
                }

                query.EventTypeFilter.AddRange(eventTypes);

                m_logger.TraceInformation($"UTC insertion date range from: {query.InsertionStartTimeUtc.GetValueOrDefault():F} to {query.InsertionEndTimeUtc.GetValueOrDefault():F}.");

                foreach (var accessManager in accessManagers)
                {
                    query.StartingAfterIndexes.Add(new RawEventIndex { AccessManager = accessManager.Guid, Position = 0 });

                    int totalEvents = 0;
                    do
                    {
                        m_logger.TraceInformation($"retrieving next {query.MaximumResultCount} events from {accessManager.Name}");
                        var args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

                        if (!args.Success && args.Error != ReportError.TooManyResults)
                        {
                            m_logger.TraceWarning(new TraceEntry($"{args.Error}", string.Join(Environment.NewLine, args.SubQueryErrors.Where(details => !details.Successful).Select(details => $"{details.ReportError}: {details.ErrorDetails}"))));
                        }

                        var results = args.Data.AsEnumerable().Take(query.MaximumResultCount).Select(CreateEvent).ToList();

                        totalEvents += results.Count;
                        foreach (var result in results)
                        {
                            observer.OnNext(result);
                        }

                        if (results.Count < query.MaximumResultCount)
                        {
                            m_logger.TraceInformation($"Total events retrieved from {accessManager.Name}: {totalEvents}");
                            break;
                        }

                        query.StartingAfterIndexes.Clear();
                        query.StartingAfterIndexes.AddRange(results.GroupBy(@event => @event.AccessManagerGuid).Select(group => new RawEventIndex { AccessManager = group.Key, Position = group.Max(rawEvent => rawEvent.Position) }));

                    } while (true);

                    query.StartingAfterIndexes.Clear();
                }
            });

            AccessControlRawEvent CreateEvent(DataRow row)
            {
                return new AccessControlRawEvent
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

        private async Task CheckAccessManagerRolesAndUnit()
        {
            m_logger.TraceInformation("Verifying access manager roles and access control units state...");

            var query = (EntityConfigurationQuery)m_engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
            query.EntityTypeFilter.Add(EntityType.Role);
            query.EntityTypeFilter.Add(EntityType.Unit);
            await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            var roles = m_engine.GetEntities(EntityType.Role).OfType<AccessManagerRole>().Where(role => role.RunningState != State.Running).ToList();
            if (roles.Any())
            {
                m_logger.TraceWarning(new TraceEntry("Some access manager roles are offline.", string.Join(Environment.NewLine, roles.Select(role => $"{role.Name}: {role.RunningState}"))));

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            else
            {
                m_logger.TraceInformation("All access manager roles are operational.");
            }

            var units = m_engine.GetEntities(EntityType.Unit).OfType<Unit>().Where(unit => unit.RunningState != State.Running).ToList();
            if (units.Any())
            {
                m_logger.TraceWarning(new TraceEntry("Some access control units are offline.", string.Join(Environment.NewLine, units.Select(unit => $"{unit.Name}: {unit.RunningState}"))));

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            else
            {
                m_logger.TraceInformation("All access control units are operational.");
            }

            var federatedUnits = units.Where(unit => unit.Synchronised).ToList();
            if (federatedUnits.Any())
            {
                m_logger.TraceWarning(new TraceEntry("Some access manager roles are federated.", string.Join(Environment.NewLine, federatedUnits.Select(unit => unit.Name))));

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }
}