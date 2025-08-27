// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class AccessControlRawEventQuerySample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load roles into the entity cache
        await LoadEntities(engine, token, EntityType.Role);

        // Retrieve access manager roles from the entity cache
        List<AccessManagerRole> accessManagers = engine.GetEntities(EntityType.Role).OfType<AccessManagerRole>().ToList();

        // Retrieve access granted events from the last 24 hours
        DateTime startDate = DateTime.UtcNow.AddDays(-1);
        DateTime endDate = DateTime.UtcNow;
        IEnumerable<EventType> eventTypes = [EventType.AccessGranted];

        // Retrieve access control raw events
        IList<AccessControlRawEvent> rawEvents = await GetAccessControlRawEvents(engine, startDate, endDate, eventTypes, accessManagers);

        // Display access control raw events
        Console.WriteLine($"{rawEvents.Count} access control raw events retrieved.");
        foreach (AccessControlRawEvent rawEvent in rawEvents)
        {
            DisplayToConsole(rawEvent);
        }
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
}