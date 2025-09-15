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
using Genetec.Sdk.Queries.IntrusionDetection;

namespace Genetec.Dap.CodeSamples;

public class IntrusionDetectionQuerySample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        await RunIntrusionAreaQuery(engine, token);
        await RunIntrusionUnitQuery(engine, token);
    }

    private static IntrusionDetectionRecord MapToIntrusionDetectionRecord(DataRow row) =>
        new()
        {
            Timestamp = row.Field<DateTime>(IntrusionDetectionReportQuery.TimestampUtcColumnName),
            EventType = (EventType)row.Field<int>(IntrusionDetectionReportQuery.EventTypeColumnName),
            IntrusionUnitId = row.Field<Guid>(IntrusionDetectionReportQuery.IntrusionUnitIdColumnName),
            IntrusionAreaId = row.Field<Guid>(IntrusionDetectionReportQuery.IntrusionAreaIdColumnName),
            DeviceId = row.Field<Guid>(IntrusionDetectionReportQuery.DeviceIdColumnName),
            SourceGuid = row.Field<Guid>(IntrusionDetectionReportQuery.SourceGuidColumnName),
            OccurrencePeriod = row.Field<int>(IntrusionDetectionReportQuery.OccurrencePeriodColumnName),
            TimeZoneId = row.Field<string>(IntrusionDetectionReportQuery.TimeZoneIdColumnName),
            InitiatorId = row.Field<Guid>(IntrusionDetectionReportQuery.InitiatorIdColumnName)
        };

    private async Task RunIntrusionAreaQuery(Engine engine, CancellationToken token)
    {
        Console.WriteLine("1. Intrusion Area Activity Query - Zone violations and sensor events");
        Console.WriteLine("-".PadRight(60, '-'));

        var events = await QueryIntrusionAreaActivity(engine);
        DisplayIntrusionAreaActivity(engine, events);
    }

    private async Task<List<IntrusionDetectionRecord>> QueryIntrusionAreaActivity(Engine engine)
    {
        var query = (IntrusionAreaActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.IntrusionAreaActivity);
        query.TimeRange.SetTimeRange(DateTime.Now.AddDays(-7), DateTime.Now);
        query.MaximumResultCount = 50;

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(MapToIntrusionDetectionRecord).ToList();
    }

    private void DisplayIntrusionAreaActivity(Engine engine, List<IntrusionDetectionRecord> events)
    {
        Console.WriteLine($"Found {events.Count} intrusion area events:");
        
        var eventsByArea = events
            .Where(e => e.IntrusionAreaId != Guid.Empty)
            .GroupBy(e => e.IntrusionAreaId);
        
        foreach (var areaGroup in eventsByArea)
        {
            var area = engine.GetEntity(areaGroup.Key);
            Console.WriteLine($"\n{area?.Name ?? "Unknown Area"}: {areaGroup.Count()} events");
            
            foreach (var evt in areaGroup)
            {
                var device = engine.GetEntity(evt.DeviceId);
                var initiator = engine.GetEntity(evt.InitiatorId);
                
                Console.WriteLine($"  {evt.Timestamp:yyyy-MM-dd HH:mm:ss} - {evt.EventType}");
                Console.WriteLine($"    Device: {device?.Name ?? "Unknown"}");
                if (initiator != null)
                    Console.WriteLine($"    Initiator: {initiator.Name}");
            }
        }
        Console.WriteLine();
    }

    private async Task RunIntrusionUnitQuery(Engine engine, CancellationToken token)
    {
        Console.WriteLine("2. Intrusion Unit Activity Query - Unit status and hardware events");
        Console.WriteLine("-".PadRight(60, '-'));

        var events = await QueryIntrusionUnitActivity(engine);
        DisplayIntrusionUnitActivity(engine, events);
    }

    private async Task<List<IntrusionDetectionRecord>> QueryIntrusionUnitActivity(Engine engine)
    {
        var query = (IntrusionUnitQuery)engine.ReportManager.CreateReportQuery(ReportType.IntrusionUnitActivity);
        query.TimeRange.SetTimeRange(DateTime.Now.AddDays(-7), DateTime.Now);
        query.MaximumResultCount = 50;

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(MapToIntrusionDetectionRecord).ToList();
    }

    private void DisplayIntrusionUnitActivity(Engine engine, List<IntrusionDetectionRecord> events)
    {
        Console.WriteLine($"Found {events.Count} intrusion unit events:");
        
        var eventsByUnit = events
            .Where(e => e.IntrusionUnitId != Guid.Empty)
            .GroupBy(e => e.IntrusionUnitId);
        
        foreach (var unitGroup in eventsByUnit)
        {
            Entity unit = engine.GetEntity(unitGroup.Key);
            Console.WriteLine($"\n{unit?.Name ?? "Unknown Unit"}: {unitGroup.Count()} events");
            
            var eventsByType = unitGroup.GroupBy(e => e.EventType);
            foreach (IGrouping<EventType, IntrusionDetectionRecord> typeGroup in eventsByType)
            {
                Console.WriteLine($"  {typeGroup.Key}: {typeGroup.Count()} events");
                
                foreach (IntrusionDetectionRecord evt in typeGroup)
                {
                    Console.WriteLine($"    {evt.Timestamp:MM-dd HH:mm:ss}");
                }
            }
        }
        Console.WriteLine();
    }
}