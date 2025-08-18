// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media.Export;
using DateTimeRange = Genetec.Sdk.Media.DateTimeRange;

namespace Genetec.Dap.CodeSamples;

class VideoExportSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        const string cameraGuid = "YOUR_CAMERA_GUID_HERE"; // Replace with your camera's GUID

        if (engine.GetEntity(new Guid(cameraGuid)) is not Camera camera)
        {
            Console.WriteLine($"Camera {cameraGuid} not found");
            return;
        }

        var progress = new Progress<double>(percent => Console.Write($"\rExport progress: {percent,6:F2}%"));

        // Export the last 5 minutes of video (using UTC timestamps)
        DateTime endTime = DateTime.UtcNow;
        DateTime startTime = endTime.AddMinutes(-5);
        const string fileName = "Export";

        Console.WriteLine($"Exporting video from {startTime:yyyy-MM-dd HH:mm:ss} to {endTime:yyyy-MM-dd HH:mm:ss} UTC");

        string exportedFile = await Export(engine, camera, startTime, endTime, fileName, progress, token);

        Console.WriteLine($"\nVideo file exported: {exportedFile}");
    }

    static async Task<string> Export(Engine engine, Camera camera, DateTime startTime, DateTime endTime, string fileName, IProgress<double> progress = null, CancellationToken cancellationToken = default)
    {
        using var exporter = new MediaExporter();
        exporter.StatisticsReceived += OnStatisticsReceived;
        try
        {
            exporter.Initialize(engine, Environment.CurrentDirectory);
            exporter.SetExportFileFormat(MediaExportFileFormat.G64X); // G64X is Genetec's proprietary format that preserves metadata and supports encryption

            var config = new CameraExportConfig(camera.Guid, Enumerable.Repeat(new DateTimeRange(startTime, endTime), 1));

            using CancellationTokenRegistration registration = cancellationToken.Register(() => exporter.CancelExport(true));

            ExportEndedResult result = await exporter.ExportAsync(config, PlaybackMode.AllAtOnce, Path.GetFileNameWithoutExtension(fileName), false);

            return result.ExceptionDetails != null ? throw result.ExceptionDetails : result.ExportFileList.FirstOrDefault();
        }
        finally
        {
            exporter.StatisticsReceived -= OnStatisticsReceived;
        }

        void OnStatisticsReceived(object sender, ExportStatisticsEventArgs args) => progress?.Report(args.ExportPercentComplete);
    }
}