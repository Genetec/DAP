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

public class VideoFileQuerySample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load cameras into the entity cache
        await LoadEntities(engine, token, EntityType.Camera);

        // Retrieve cameras from the entity cache
        List<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>().ToList();
        Console.WriteLine($"{cameras.Count} cameras loaded");

        // Retrieve video file information for all cameras
        IEnumerable<VideoFileInfo> videoFileInfos = await GetVideoFileInfos(engine, cameras);

        // Display video file information
        foreach (VideoFileInfo videoFileInfo in videoFileInfos)
        {
            DisplayToConsole(engine, videoFileInfo);
        }
    }


    private async Task<IEnumerable<VideoFileInfo>> GetVideoFileInfos(Engine engine, IEnumerable<Camera> cameras)
    {
        Console.WriteLine("Retrieving video file information...");

        var query = (VideoFileQuery)engine.ReportManager.CreateReportQuery(ReportType.VideoFile);
        query.Cameras.AddRange(cameras.Select(camera => camera.Guid));

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

        return args.Data.AsEnumerable().Select(CreateFromDataRow).ToList();

        VideoFileInfo CreateFromDataRow(DataRow row) => new()
        {
            CameraGuid = row.Field<Guid>(VideoFileQuery.CameraGuidColumnName),
            ArchiveSourceGuid = row.Field<Guid>(VideoFileQuery.ArchiveSourceGuidColumnName),
            StartTime = row.Field<DateTime>(VideoFileQuery.StartTimeColumnName),
            EndTime = row.Field<DateTime>(VideoFileQuery.EndTimeColumnName),
            FilePath = row.Field<string>(VideoFileQuery.FilePathColumnName),
            FileSize = row.Field<decimal>(VideoFileQuery.FileSizeColumnName),
            MetadataPath = row.Field<string>(VideoFileQuery.MetadataPathColumnName),
            ProtectionStatus = (VideoProtectionState)row.Field<uint>(VideoFileQuery.ProtectionStatusColumnName),
            InfiniteProtection = row.Field<bool>(VideoFileQuery.InfiniteProtectionColumnName),
            Drive = row.Field<string>(VideoFileQuery.DriveColumnName),
            Error = row.Field<uint>(VideoFileQuery.ErrorColumnName),
            ProtectionEndDateTime = row.Field<DateTime>(VideoFileQuery.ProtectionEndDateTimeColumnName)
        };
    }

    private void DisplayToConsole(Engine engine, VideoFileInfo info)
    {
        Console.WriteLine("Video File Information:");
        Console.WriteLine($"  {"Camera:",-22} {engine.GetEntity(info.CameraGuid).Name}");
        Console.WriteLine($"  {"Archive Source:",-22} {engine.GetEntity(info.ArchiveSourceGuid).Name}");
        Console.WriteLine($"  {"Start Time:",-22} {info.StartTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"  {"End Time:",-22} {info.EndTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"  {"File Path:",-22} {info.FilePath}");
        Console.WriteLine($"  {"File Size:",-22} {info.FileSize:N0} bytes");
        Console.WriteLine($"  {"Metadata Path:",-22} {info.MetadataPath}");
        Console.WriteLine($"  {"Protection Status:",-22} {info.ProtectionStatus}");
        Console.WriteLine($"  {"Infinite Protection:",-22} {info.InfiniteProtection}");
        Console.WriteLine($"  {"Drive:",-22} {info.Drive}");
        Console.WriteLine($"  {"Error:",-22} {info.Error}");
        Console.WriteLine($"  {"Protection End Date:",-22} {info.ProtectionEndDateTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine(new string('-', 60));
    }
}