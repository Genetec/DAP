// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sdk;
using Sdk.Entities;
using Sdk.Queries.Video;

public class CameraEventSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        Console.WriteLine("Loading cameras...");
        await LoadEntities(engine, token, EntityType.Camera);

        Console.WriteLine("Retrieving camera events...");
        // Fetch camera events for the last 24 hours
        DateTime from = DateTime.UtcNow.AddDays(-1);
        DateTime to = DateTime.UtcNow;
        IEnumerable<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>();

        ICollection<CameraEventDetail> events = await GetCameraEvents(engine, cameras.Select(camera => camera.Guid), from, to);
        PrintCameraEvents(engine, events);
    }

    async Task<ICollection<CameraEventDetail>> GetCameraEvents(Engine engine, IEnumerable<Guid> cameras, DateTime from, DateTime to)
    {
        var query = (CameraEventQuery)engine.ReportManager.CreateReportQuery(ReportType.CameraEvent);
        query.TimeRange.SetTimeRange(from, to);
        query.Cameras.AddRange(cameras);

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(CreateFromDataRow).ToList();

        CameraEventDetail CreateFromDataRow(DataRow row) => new()
        {
            CameraGuid = row.Field<Guid>(VideoEventQuery.CameraGuidColumnName),
            ArchiveSourceGuid = row.Field<Guid>(VideoEventQuery.ArchiveSourceGuidColumnName),
            EventTime = row.Field<DateTime>(VideoEventQuery.EventTimeColumnName),
            EventType = (EventType)row.Field<uint>(VideoEventQuery.EventTypeColumnName),
            Value = row.Field<uint>(VideoEventQuery.ValueColumnName),
            Notes = row.Field<string>(VideoEventQuery.NotesColumnName),
            XmlData = row.Field<string>(VideoEventQuery.XmlDataColumnName),
            Capabilities = row.Field<uint>(VideoEventQuery.CapabilitiesColumnName),
            TimeZone = row.Field<string>(VideoEventQuery.TimeZoneColumnName),
            Thumbnail = row.Field<byte[]>(VideoEventQuery.ThumbnailColumnName)
        };
    }

    void PrintCameraEvents(Engine engine, ICollection<CameraEventDetail> events)
    {
        if (!events.Any())
        {
            Console.WriteLine("No camera events found in the specified time range.");
            return;
        }

        Console.WriteLine($"\nFound {events.Count} camera events:");
        Console.WriteLine(new string('-', 80));

        foreach (CameraEventDetail detail in events.OrderBy(e => e.EventTime))
        {
            Console.WriteLine($"Camera: {GetEntityName(detail.CameraGuid)}");
            Console.WriteLine($"  Event Type: {detail.EventType} (Value: {detail.Value})");
            Console.WriteLine($"  Event Time: {detail.EventTime.ToLocalTime()}");
            Console.WriteLine($"  Archive Source: {GetEntityName(detail.ArchiveSourceGuid)}");
            Console.WriteLine($"  Time Zone: {detail.TimeZone}");

            if (!string.IsNullOrEmpty(detail.Notes))
            {
                Console.WriteLine($"  Notes: {detail.Notes}");
            }

            if (!string.IsNullOrEmpty(detail.XmlData))
            {
                Console.WriteLine($"  XML Data: {detail.XmlData}");
            }

            if (detail.Thumbnail?.Length > 0)
            {
                Console.WriteLine($"  Thumbnail: {detail.Thumbnail.Length} bytes");
            }

            Console.WriteLine();
        }

        string GetEntityName(Guid entityId) => engine.GetEntity(entityId)?.Name ?? "Unknown";
    }
}