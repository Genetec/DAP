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
using Genetec.Sdk.Helpers;
using Genetec.Sdk.Queries.Video;

namespace Genetec.Dap.CodeSamples;

public class ArchiverEventQuerySample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        await LoadEntities(engine, token, EntityType.Role);

        List<ArchiverRole> archivers = engine.GetEntities(EntityType.Role).OfType<ArchiverRole>().ToList();

        if (!archivers.Any())
        {
            Console.WriteLine("No archivers found in the system.");
            return;
        }

        List<ArchiverEvent> events = await QueryArchiverEvents(engine, archivers.Select(archiver => archiver.Guid));
        DisplayArchiverEvents(engine, events);
    }

    private async Task<List<ArchiverEvent>> QueryArchiverEvents(Engine engine, IEnumerable<Guid> archivers)
    {
        var query = (ArchiverEventQuery)engine.ReportManager.CreateReportQuery(ReportType.ArchiverEvent);
        query.Archivers.AddRange(archivers);
        query.TimeRange.SetTimeRange(DateTime.Now.AddDays(-7), DateTime.Now);
        query.MaximumResultCount = 50;

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(MapToArchiverEvent).ToList();

        ArchiverEvent MapToArchiverEvent(DataRow row) => new()
        {
            EventTime = row.Field<DateTime>(ArchiverEventQuery.EventTimeColumnName),
            ArchiveSourceGuid = row.Field<Guid>(ArchiverEventQuery.ArchiveSourceGuidColumnName),
            EventType = (EventType)row.Field<UInt32>(ArchiverEventQuery.EventTypeColumnName)
        };
    }

    private void DisplayArchiverEvents(Engine engine, List<ArchiverEvent> events)
    {
        Console.WriteLine($"Found {events.Count} archiver events:");

        foreach (var eventRecord in events)
        {
            var archiver = engine.GetEntity(eventRecord.ArchiveSourceGuid);

            Console.WriteLine($"{eventRecord.EventTime:yyyy-MM-dd HH:mm:ss} - {archiver?.Name ?? "Unknown Archiver"}");
            Console.WriteLine($"  Event Type: {eventRecord.EventType}");
            Console.WriteLine($"  Archiver ID: {eventRecord.ArchiveSourceGuid}");
            Console.WriteLine();
        }
    }
}