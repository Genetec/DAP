// Copyright 2024 Genetec
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Queries;
    using Sdk.Queries.AccessControl;

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
                DateTime from = DateTime.UtcNow.AddDays(-30);
                DateTime to = DateTime.UtcNow;

                Guid allEntitiesGuid = SystemConfiguration.SystemConfigurationGuid;

                List<AccessControlEvent> unitActivities = await GetUnitActivities(engine, from, to, allEntitiesGuid);
                Console.WriteLine($"Unit activities ({unitActivities.Count})");
                DisplayActivities(unitActivities);

                List<AccessControlEvent> credentialActivities = await GetCredentialActivities(engine, from, to, allEntitiesGuid);
                Console.WriteLine($"Credential activities ({credentialActivities.Count})");
                DisplayActivities(credentialActivities);

                List<AccessControlEvent> cardholderActivities = await GetCardholderActivities(engine, from, to, allEntitiesGuid);
                Console.WriteLine($"Cardholder activities ({cardholderActivities.Count})");
                DisplayActivities(cardholderActivities);

                List<AccessControlEvent> areaActivities = await GetAreaActivities(engine, from, to, allEntitiesGuid);
                Console.WriteLine($"Area activities ({areaActivities.Count})");
                DisplayActivities(areaActivities);

                List<AccessControlEvent> doorActivities = await GetDoorActivities(engine, from, to, allEntitiesGuid);
                Console.WriteLine($"Door activities ({doorActivities.Count})");
                DisplayActivities(doorActivities);

                List<AccessControlEvent> zoneActivities = await GetZoneActivities(engine, from, to, allEntitiesGuid);
                Console.WriteLine($"Zone activities ({zoneActivities.Count})");
                DisplayActivities(zoneActivities);

                List<AccessControlEvent> elevatorActivities = await GetElevatorActivities(engine, from, to, allEntitiesGuid);
                Console.WriteLine($"Elevator activities ({elevatorActivities.Count})");
                DisplayActivities(elevatorActivities);

                List<AccessControlEvent> visitorActivities = await GetVisitorActivities(engine, allEntitiesGuid, from, to);
                Console.WriteLine($"Visitor activities ({visitorActivities.Count})");
                DisplayActivities(visitorActivities);
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            void DisplayActivities(IEnumerable<AccessControlEvent> activities)
            {
                foreach (AccessControlEvent activity in activities)
                {
                    Console.Write($"Timestamp: {activity.Timestamp}, EventType: {activity.EventType}");

                    WriteEntityName(activity.Unit);
                    WriteEntityName(activity.Device);
                    WriteEntityName(activity.AccessPointGroup);
                    WriteEntityName(activity.AccessPoint);
                    WriteEntityName(activity.Credential);
                    WriteEntityName(activity.Credential2);
                    WriteEntityName(activity.Cardholder);

                    Console.WriteLine();
                }

                Console.WriteLine();

                void WriteEntityName(Guid? guid)
                {
                    if (guid.HasValue)
                    {
                        Entity entity = engine.GetEntity(guid.Value);
                        if (entity != null) Console.Write($", {entity.EntityType}: {entity.Name}");
                    }
                }
            }
        }

        static Task<List<AccessControlEvent>> GetCredentialActivities(Engine engine, DateTime from, DateTime to, params Guid[] credentials)
        {
            var query = (CredentialActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.CredentialActivity);
            query.Credentials.AddRange(credentials);
            return ExecuteQuery(query, from, to);
        }

        static Task<List<AccessControlEvent>> GetCardholderActivities(Engine engine, DateTime from, DateTime to, params Guid[] cardholders)
        {
            var query = (CardholderActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.CardholderActivity);
            query.Cardholders.AddRange(cardholders);
            return ExecuteQuery(query, from, to);
        }

        static Task<List<AccessControlEvent>> GetAreaActivities(Engine engine, DateTime from, DateTime to, params Guid[] areas)
        {
            var query = (AreaActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.AreaActivity);
            query.Areas.AddRange(areas);
            return ExecuteQuery(query, from, to);
        }

        static Task<List<AccessControlEvent>> GetDoorActivities(Engine engine, DateTime from, DateTime to, params Guid[] doors)
        {
            var query = (DoorActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.DoorActivity);
            query.Doors.AddRange(doors);
            return ExecuteQuery(query, from, to);
        }

        static Task<List<AccessControlEvent>> GetZoneActivities(Engine engine, DateTime from, DateTime to, params Guid[] zones)
        {
            var query = (ZoneActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.ZoneActivity);
            query.Zones.AddRange(zones);
            return ExecuteQuery(query, from, to);
        }

        static Task<List<AccessControlEvent>> GetElevatorActivities(Engine engine, DateTime from, DateTime to, params Guid[] elevators)
        {
            var query = (ElevatorActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.ElevatorActivity);
            query.Elevators.AddRange(elevators);
            return ExecuteQuery(query, from, to);
        }

        static Task<List<AccessControlEvent>> GetVisitorActivities(Engine engine, Guid visitor, DateTime from, DateTime to)
        {
            var query = (VisitorActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.VisitorActivity);
            query.Visitor = visitor;
            return ExecuteQuery(query, from, to);
        }

        static Task<List<AccessControlEvent>> GetUnitActivities(Engine engine, DateTime from, DateTime to, params Guid[] units)
        {
            var query = (AccessControlUnitQuery)engine.ReportManager.CreateReportQuery(ReportType.AccessControlUnitActivity);
            query.Units.AddRange(units);
            return ExecuteQuery(query, from, to);
        }

        static async Task<List<AccessControlEvent>> ExecuteQuery(AccessControlReportQuery query, DateTime from, DateTime to)
        {
            query.TimeRange.SetTimeRange(from, to);
            query.MaximumResultCount = 10;

            QueryCompletedEventArgs results = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            return results.Data.AsEnumerable().Select(CreateAccessControlEvent).ToList();

            AccessControlEvent CreateAccessControlEvent(DataRow row) => new AccessControlEvent
            {
                Timestamp = DateTime.SpecifyKind(row.Field<DateTime>(AccessControlReportQuery.TimestampColumnName), DateTimeKind.Utc),
                Unit = row.Field<Guid?>(AccessControlReportQuery.UnitGuidColumnName),
                AccessPoint = row.Field<Guid?>(AccessControlReportQuery.APGuidColumnName),
                AccessPointGroup = row.Field<Guid?>(AccessControlReportQuery.AccessPointGroupGuidColumnName),
                Credential = row.Field<Guid?>(AccessControlReportQuery.CredentialGuidColumnName),
                Credential2 = row.Field<Guid?>(AccessControlReportQuery.Credential2GuidColumnName),
                Device = row.Field<Guid?>(AccessControlReportQuery.DeviceGuidColumnName),
                CustomEventMessage = row.Field<string>(AccessControlReportQuery.CustomEventMessageColumnName),
                EventType = row.Field<EventType>(AccessControlReportQuery.EventTypeColumnName),
                Source = row.Field<Guid?>(AccessControlReportQuery.SourceGuidColumnName),
                Cardholder = row.Field<Guid?>(AccessControlReportQuery.CardholderGuidColumnName),
                OccurrencePeriod = row.Field<OfflinePeriodType>(AccessControlReportQuery.OccurrencePeriodColumnName),
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById(row.Field<string>(AccessControlReportQuery.TimeZoneColumnName))
            };
        }
    }
}