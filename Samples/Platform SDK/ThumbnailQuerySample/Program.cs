// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Queries.Video;

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state == ConnectionStateCode.Success)
    {
        // Load cameras into the entity cache 
        await LoadCameras(engine);

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
    else
    {
        Console.WriteLine($"logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

Task LoadCameras(Engine engine)
{
    Console.WriteLine("Loading cameras...");

    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.Camera);

    return Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
}

async Task<IEnumerable<VideoThumbnail>> GetVideoThumbnails(Engine engine, IEnumerable<Camera> cameras)
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

void DisplayToConsole(Engine engine, VideoThumbnail videoThumbnail)
{
    Console.WriteLine("Thumbnail Details:");
    Console.WriteLine($"  {"Camera:",-16} {engine.GetEntity(videoThumbnail.Camera).Name}");
    Console.WriteLine($"  {"Timestamp:",-16} {videoThumbnail.Timestamp.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"  {"Latest Frame:",-16} {videoThumbnail.LatestFrame.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"  {"Size:",-16} {videoThumbnail.Data?.Length ?? 0} bytes");
    Console.WriteLine($"  {"Context:",-16} {videoThumbnail.Context}");
    Console.WriteLine(new string('-', 50));
}

class VideoThumbnail
{
    public Guid Camera { get; set; }
    public byte[] Data { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime LatestFrame { get; set; }
    public Guid Context { get; set; }
}
