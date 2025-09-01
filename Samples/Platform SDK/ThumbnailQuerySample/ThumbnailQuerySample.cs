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

public class ThumbnailQuerySample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load cameras into the entity cache 
        await LoadEntities(engine, token, EntityType.Camera);

        // Retrieve cameras from the entity cache
        List<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>().ToList();
        Console.WriteLine($"{cameras.Count} cameras loaded");

        // Retrieve video thumbnails for all cameras
        IEnumerable<VideoThumbnail> thumbnails = await GetVideoThumbnails(engine, cameras);

        foreach (VideoThumbnail thumbnail in thumbnails)
        {
            DisplayToConsole(engine, thumbnail);
        }
    }

    private async Task<IEnumerable<VideoThumbnail>> GetVideoThumbnails(Engine engine, IEnumerable<Camera> cameras)
    {
        Console.WriteLine("Retrieving thumbnails...");

        var query = (VideoThumbnailQuery)engine.ReportManager.CreateReportQuery(ReportType.Thumbnail);

        const int thumbnailWidth = 200;

        foreach (Camera camera in cameras)
        {
            query.AddTimestamp(camera: camera.Guid, timestamp: DateTime.UtcNow.AddSeconds(-1), width: thumbnailWidth);
        }

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

        return args.Data.AsEnumerable().Select(CreateFromDataRow).ToList();

        VideoThumbnail CreateFromDataRow(DataRow row) => new()
        {
            Camera = row.Field<Guid>(VideoThumbnailQuery.CameraColumnName),
            Data = row.Field<byte[]>(VideoThumbnailQuery.ThumbnailColumnName),
            Timestamp = row.Field<DateTime>(VideoThumbnailQuery.TimestampColumnName),
            LatestFrame = row.Field<DateTime>(VideoThumbnailQuery.LatestFrameColumnName),
            Context = row.Field<Guid>(VideoThumbnailQuery.ContextColumnName)
        };
    }

    private void DisplayToConsole(Engine engine, VideoThumbnail videoThumbnail)
    {
        Console.WriteLine("Thumbnail Details:");
        Console.WriteLine($"  {"Camera:",-16} {engine.GetEntity(videoThumbnail.Camera).Name}");
        Console.WriteLine($"  {"Timestamp:",-16} {videoThumbnail.Timestamp.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"  {"Latest Frame:",-16} {videoThumbnail.LatestFrame.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"  {"Size:",-16} {videoThumbnail.Data?.Length ?? 0} bytes");
        Console.WriteLine($"  {"Context:",-16} {videoThumbnail.Context}");
        Console.WriteLine(new string('-', 50));
    }
}