// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;

const string server = "localhost";
const string username = "admin";
const string password = "";

SdkResolver.Initialize();
await RunSample();

async Task RunSample()
{
    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state == ConnectionStateCode.Success)
    {
        // Load roles into the entity cache
        await LoadRoles(engine);

        // Retrieve access manager roles from the entity cache
        List<AccessManagerRole> accessManagers = engine.GetEntities(EntityType.Role).OfType<AccessManagerRole>().ToList();

        // Retrieve access granted events from the last 24 hours
        DateTime startDate = DateTime.UtcNow.AddDays(-1);
        DateTime endDate = DateTime.UtcNow;
        IEnumerable<EventType> eventTypes = new[] { EventType.AccessGranted };

        // Retrieve access control raw events
        IList<AccessControlRawEvent> rawEvents = await GetAccessControlRawEvents(engine, startDate, endDate, eventTypes, accessManagers);

        // Display access control raw events
        Console.WriteLine($"{rawEvents.Count} access control raw events retrieved.");
        foreach (AccessControlRawEvent rawEvent in rawEvents)
        {
            DisplayToConsole(rawEvent);
        }
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

Task LoadRoles(Engine engine)
{
    Console.WriteLine("Loading roles...");
    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.Role);
    return Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
}

async Task<IList<AccessControlRawEvent>> GetAccessControlRawEvents(Engine engine, DateTime startDate, DateTime endDate, IEnumerable<EventType> eventTypes, IEnumerable<AccessManagerRole> accessManagers)
{
    var results = new List<AccessControlRawEvent>();

    var query = (AccessControlRawEventQuery)engine.ReportManager.CreateReportQuery(ReportType.AccessControlRawEvent);
    query.MaximumResultCount = 5000; // Maximum number of results to retrieve per query
    query.InsertionStartTimeUtc = startDate.ToUniversalTime();
    query.InsertionEndTimeUtc = endDate.ToUniversalTime();
    query.EventTypeFilter.AddRange(eventTypes);

    foreach (AccessManagerRole accessManager in accessManagers)
    {
        Console.WriteLine($"Retrieving access control raw events for access manager: {accessManager.Name}");

        long lastPosition = 0;
        do
        {
            // Retrieve the next batch of results starting after the last position
            query.StartingAfterIndexes.Clear();
            query.StartingAfterIndexes.Add(new RawEventIndex { AccessManager = accessManager.Guid, Position = lastPosition });

            QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            List<AccessControlRawEvent> batchResults = args.Data.AsEnumerable().Take(query.MaximumResultCount).Select(CreateAccessControlRawEvent).ToList();
            results.AddRange(batchResults);

            if (batchResults.Count < query.MaximumResultCount)
            {
                break;
            }

            // Update the last position to retrieve the next batch of results
            lastPosition = batchResults.Max(rawEvent => rawEvent.Position);

        } while (true);
    }

    return results;

    AccessControlRawEvent CreateAccessControlRawEvent(DataRow row) => new()
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

void DisplayToConsole(AccessControlRawEvent rawEvent)
{
    Console.WriteLine($"Event Type: {rawEvent.EventType}");
    Console.WriteLine($"Timestamp: {rawEvent.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
    Console.WriteLine($"Insertion: {rawEvent.InsertionTimestamp:yyyy-MM-dd HH:mm:ss} UTC");
    Console.WriteLine($"Time Zone: {rawEvent.TimeZone ?? "N/A"}");
    Console.WriteLine($"Position: {rawEvent.Position}");
    Console.WriteLine($"Occurrence Period: {rawEvent.OccurrencePeriod}");
    Console.WriteLine("Access Information:");
    Console.WriteLine($"  Access Point: {rawEvent.AccessPointGuid}");
    Console.WriteLine($"  Access Point Group: {rawEvent.AccessPointGroupGuid}");
    Console.WriteLine($"  Cardholder: {rawEvent.CardholderGuid}");
    Console.WriteLine($"  Credential: {rawEvent.CredentialGuid}");
    Console.WriteLine($"  Access Manager: {rawEvent.AccessManagerGuid}");
    Console.WriteLine($"  Device: {rawEvent.DeviceGuid}");
    Console.WriteLine(new string('=', 80));
    Console.WriteLine();
}

class AccessControlRawEvent
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