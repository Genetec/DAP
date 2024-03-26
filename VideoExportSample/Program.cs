// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Sdk.Entities;
    using Sdk.Media.Export;
    using Sdk;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                var entity = (Camera)engine.GetEntity(EntityType.Camera, 1);

                try
                {
                    var progress = new Progress<double>(percent => Console.WriteLine($"Export progress: {percent}"));
                    var exportedFile = await Export(engine, entity, DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow, "Export", progress);
                    Console.WriteLine($"Video file exported: {exportedFile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occured while exporting video: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task<string> Export(Engine engine, Camera camera, DateTime startTime, DateTime endTime, string fileName, IProgress<double> progress = null, CancellationToken cancellationToken = default)
        {
            var exporter = new MediaExporter();
            exporter.StatisticsReceived += OnStatisticsReceived;

            try
            {
                exporter.Initialize(engine, Environment.CurrentDirectory);
                exporter.SetExportFileFormat(MediaExportFileFormat.G64X);

                var config = new CameraExportConfig(camera.Guid, Enumerable.Repeat(new Genetec.Sdk.Media.DateTimeRange(startTime, endTime), 1));

                using (cancellationToken.Register(() => exporter.CancelExport(true)))
                {
                    ExportEndedResult result = await exporter.ExportAsync(config, PlaybackMode.AllAtOnce, Path.GetFileNameWithoutExtension(fileName), false);

                    return result.ExceptionDetails != null ? throw result.ExceptionDetails : result.ExportFileList.FirstOrDefault();
                }
            }
            finally
            {
                exporter.StatisticsReceived -= OnStatisticsReceived;
                exporter.Dispose();
            }

            void OnStatisticsReceived(object sender, ExportStatisticsEventArgs args) => progress?.Report(args.ExportPercentComplete);
        }
    }
}
