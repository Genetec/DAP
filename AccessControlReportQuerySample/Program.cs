// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk;
    using Genetec.Sdk.Entities;
    using Genetec.Sdk.Queries;
    using Genetec.Sdk.Queries.AccessControl;

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
                DateTime from = DateTime.UtcNow.AddDays(-30); // last 30 days
                DateTime to = DateTime.UtcNow;
                int maxResultCount = 10; // limit the number of results to 10
                Guid allEntitiesGuid = SystemConfiguration.SystemConfigurationGuid; // Get result from all entities

                await QueryAndDisplayActivities("Unit", ReportType.AccessControlUnitActivity, from, to, maxResultCount, allEntitiesGuid);
                await QueryAndDisplayActivities("Credential", ReportType.CredentialActivity, from, to, maxResultCount, allEntitiesGuid);
                await QueryAndDisplayActivities("Cardholder", ReportType.CardholderActivity, from, to, maxResultCount, allEntitiesGuid);
                await QueryAndDisplayActivities("Area", ReportType.AreaActivity, from, to, maxResultCount, allEntitiesGuid);
                await QueryAndDisplayActivities("Door", ReportType.DoorActivity, from, to, maxResultCount, allEntitiesGuid);
                await QueryAndDisplayActivities("Zone", ReportType.ZoneActivity, from, to, maxResultCount, allEntitiesGuid);
                await QueryAndDisplayActivities("Elevator", ReportType.ElevatorActivity, from, to, maxResultCount, allEntitiesGuid);
                await QueryAndDisplayActivities("Visitor", ReportType.VisitorActivity, from, to, maxResultCount, allEntitiesGuid);
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            async Task QueryAndDisplayActivities(string activityType, ReportType reportType, DateTime from, DateTime to,int maximumResultCount, params Guid[] allEntitiesGuid)
            {
                List<AccessControlEvent> activities = await ExecuteAccessControlReportQuery(reportType, from, to, maximumResultCount, allEntitiesGuid);
                
                Console.WriteLine($"{activityType} activities ({activities.Count})");

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

            async Task<List<AccessControlEvent>> ExecuteAccessControlReportQuery(ReportType reportType, DateTime from, DateTime to, int maxResults, params Guid[] entities)
            {
                var query = (AccessControlReportQuery)engine.ReportManager.CreateReportQuery(reportType);
                query.TimeRange.SetTimeRange(from, to);
                query.MaximumResultCount = maxResults;

                if (query is VisitorActivityQuery visitorActivityQuery)
                {
                    visitorActivityQuery.Visitor = entities.First();
                }
                else
                {
                    query.QueryEntities.AddRange(entities);
                }

                QueryCompletedEventArgs results = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                return results.Data.AsEnumerable().Take(query.MaximumResultCount).Select(CreateAccessControlEvent).ToList();

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
}