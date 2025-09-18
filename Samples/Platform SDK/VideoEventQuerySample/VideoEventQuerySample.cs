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
using Genetec.Sdk.Queries.Video;

namespace Genetec.Dap.CodeSamples;

public class VideoEventQuerySample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        await LoadEntities(engine, token, EntityType.Camera);
        List<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>().ToList();
        
        if (!cameras.Any())
        {
            Console.WriteLine("No cameras found to query events for.");
            return;
        }

        await RunCameraEventQuery(engine, cameras, token);
        await RunMotionEventQuery(engine, cameras, token);
        await RunRecordingEventQuery(engine, cameras, token);
    }

    private static VideoEventRecord MapToVideoEvent(DataRow row) => new()
    {
        CameraGuid = row.Field<Guid>(VideoEventQuery.CameraGuidColumnName),
        ArchiveSourceGuid = row.Field<Guid>(VideoEventQuery.ArchiveSourceGuidColumnName),
        EventTime = row.Field<DateTime>(VideoEventQuery.EventTimeColumnName),
        EventType = (EventType)row.Field<uint>(VideoEventQuery.EventTypeColumnName),
        Value = row.Field<uint>(VideoEventQuery.ValueColumnName),
        Notes = row.Field<string>(VideoEventQuery.NotesColumnName),
        XMLData = row.Field<string>(VideoEventQuery.XmlDataColumnName),
        Capabilities = row.Field<uint>(VideoEventQuery.CapabilitiesColumnName),
        TimeZone = row.Field<string>(VideoEventQuery.TimeZoneColumnName)
    };

    private async Task RunCameraEventQuery(Engine engine, List<Camera> cameras, CancellationToken token)
    {
        Console.WriteLine("1. Camera Event Query");
        Console.WriteLine("-".PadRight(50, '-'));

        List<VideoEventRecord> events = await QueryCameraEvents(engine, cameras, token);
        DisplayCameraEvents(engine, events, cameras.Count);
    }

    private async Task<List<VideoEventRecord>> QueryCameraEvents(Engine engine, List<Camera> cameras, CancellationToken token)
    {
        var query = (CameraEventQuery)engine.ReportManager.CreateReportQuery(ReportType.CameraEvent);
        query.TimeRange.SetTimeRange(DateTime.Now.AddDays(-7), DateTime.Now);
        query.MaximumResultCount = 20;

        query.Cameras.Clear();
        foreach (var camera in cameras)
        {
            query.Cameras.Add(camera.Guid);
        }

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(MapToVideoEvent).ToList();
    }

    private void DisplayCameraEvents(Engine engine, List<VideoEventRecord> events, int cameraCount)
    {
        Console.WriteLine($"Found {events.Count} camera events from {cameraCount} cameras:");
        
        var eventsByCamera = events.GroupBy(e => e.CameraGuid);
        
        foreach (var cameraGroup in eventsByCamera)
        {
            Entity camera = engine.GetEntity(cameraGroup.Key);
            Console.WriteLine($"\n{camera?.Name ?? "Unknown Camera"}: {cameraGroup.Count()} events");
            
            foreach (var evt in cameraGroup)
            {
                Console.WriteLine($"  {evt.EventTime:MM-dd HH:mm} - {evt.EventType} (Value: {evt.Value})");
                if (!string.IsNullOrEmpty(evt.Notes))
                    Console.WriteLine($"    Notes: {evt.Notes}");
            }
        }
        Console.WriteLine();
    }

    private async Task RunMotionEventQuery(Engine engine, List<Camera> cameras, CancellationToken token)
    {
        Console.WriteLine("2. Motion Event Query");
        Console.WriteLine("-".PadRight(50, '-'));

        List<VideoEventRecord> events = await QueryMotionEvents(engine, cameras, token);
        DisplayMotionEvents(engine, events);
    }

    private async Task<List<VideoEventRecord>> QueryMotionEvents(Engine engine, List<Camera> cameras, CancellationToken token)
    {
        var query = (MotionEventQuery)engine.ReportManager.CreateReportQuery(ReportType.MotionEvent);
        query.TimeRange.SetTimeRange(DateTime.Now.AddDays(-3), DateTime.Now);
        query.MaximumResultCount = 15;

        query.Cameras.Clear();
        foreach (var camera in cameras)
        {
            query.Cameras.Add(camera.Guid);
        }

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(MapToVideoEvent).ToList();
    }

    private void DisplayMotionEvents(Engine engine, List<VideoEventRecord> events)
    {
        Console.WriteLine($"Found {events.Count} motion events:");
        
        foreach (VideoEventRecord videoEvent in events)
        {
            var camera = engine.GetEntity(videoEvent.CameraGuid);
            Console.WriteLine($"{videoEvent.EventTime:yyyy-MM-dd HH:mm:ss} - {camera?.Name ?? "Unknown"}");
            Console.WriteLine($"  Motion Level: {videoEvent.Value}, Type: {videoEvent.EventType}");
        }
        Console.WriteLine();
    }

    private async Task RunRecordingEventQuery(Engine engine, List<Camera> cameras, CancellationToken token)
    {
        Console.WriteLine("3. Recording Event Query");
        Console.WriteLine("-".PadRight(50, '-'));

        List<VideoEventRecord> events = await QueryRecordingEvents(engine, cameras, token);
        DisplayRecordingEvents(engine, events);
    }

    private async Task<List<VideoEventRecord>> QueryRecordingEvents(Engine engine, List<Camera> cameras, CancellationToken token)
    {
        var query = (RecordingEventQuery)engine.ReportManager.CreateReportQuery(ReportType.RecordingEvent);
        query.TimeRange.SetTimeRange(DateTime.Now.AddDays(-2), DateTime.Now);
        query.MaximumResultCount = 25;
        query.StartRecordingFilter = true;
        query.StopRecordingFilter = true;

        query.Cameras.Clear();
        foreach (var camera in cameras)
        {
            query.Cameras.Add(camera.Guid);
        }

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(MapToVideoEvent).ToList();
    }

    private void DisplayRecordingEvents(Engine engine, List<VideoEventRecord> events)
    {
        if (!events.Any())
        {
            Console.WriteLine("No recording events found.");
            return;
        }

        Console.WriteLine($"Found {events.Count} recording events:");
        Console.WriteLine(new string('-', 80));

        foreach (var videoEvent in events)
        {
            Entity camera = engine.GetEntity(videoEvent.CameraGuid);
            Console.WriteLine($"{videoEvent.EventTime:MM-dd HH:mm} - {camera?.Name ?? "Unknown Camera"} - {videoEvent.EventType} (Value: {videoEvent.Value})");
            if (!string.IsNullOrEmpty(videoEvent.Notes))
            {
                Console.WriteLine($"    Notes: {videoEvent.Notes}");
            }
        }

        Console.WriteLine(new string('-', 80));
    }
}