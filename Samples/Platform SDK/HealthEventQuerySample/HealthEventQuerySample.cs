// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Queries.HealthMonitoring;

namespace Genetec.Dap.CodeSamples;

public class HealthEventQuerySample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        List<HealthEventRecord> healthEvents = await QueryHealthEvents(engine);
        DisplayHealthEvents(engine, healthEvents);
    }

    private async Task<List<HealthEventRecord>> QueryHealthEvents(Engine engine)
    {
        var query = (HealthEventQuery)engine.ReportManager.CreateReportQuery(ReportType.HealthEvents);
        query.TimeRange.SetTimeRange(DateTime.Now.AddDays(-1), DateTime.Now);
        query.MaximumResultCount = 50;

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(MapToHealthEvent).ToList();

        HealthEventRecord MapToHealthEvent(DataRow row) => new()
        {
            HealthEventId = row.Field<int>(HealthEventQuery.HealthEventIdColumnName),
            EventSourceTypeId = row.Field<int>(HealthEventQuery.EventSourceTypeIdColumnName),
            SourceEntityGuid = row.Field<Guid?>(HealthEventQuery.SourceEntityGuidColumnName),
            EventDescription = row.Field<string>(HealthEventQuery.EventDescriptionColumnName),
            MachineName = row.Field<string>(HealthEventQuery.MachineNameColumnName),
            Timestamp = row.Field<DateTime>(HealthEventQuery.TimestampColumnName),
            SeverityId = row.Field<int>(HealthEventQuery.SeverityIdColumnName)
        };
    }

    private void DisplayHealthEvents(Engine engine, List<HealthEventRecord> healthEvents)
    {
        if (!healthEvents.Any())
        {
            Console.WriteLine("No health events found.");
            return;
        }

        Console.WriteLine("Health Events:");
        Console.WriteLine(new string('-', 100));
        Console.WriteLine($"{"Event ID",-10} {"Source Type",-15} {"Source GUID",-36} {"Description",-20} {"Machine",-15} {"Timestamp",-20} {"Severity",-10}");
        Console.WriteLine(new string('-', 100));

        foreach (var eventRecord in healthEvents)
        {
            Console.WriteLine($"{eventRecord.HealthEventId,-10} {eventRecord.EventSourceTypeId,-15} {GetEntityName(eventRecord.SourceEntityGuid),-36} {eventRecord.EventDescription,-20} {eventRecord.MachineName,-15} {eventRecord.Timestamp,-20} {eventRecord.SeverityId,-10}");
        }

        Console.WriteLine(new string('-', 100));
        Console.WriteLine($"Total events: {healthEvents.Count}");

        string GetEntityName(Guid? guid) => engine.GetEntity(guid.Value)?.Name ?? "Unknown Entity";
    }
}