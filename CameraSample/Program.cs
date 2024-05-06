// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk.Entities.Video;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Queries;

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
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.Camera);
                query.MaximumResultCount = 5;
                await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

                foreach (var camera in engine.GetEntities(EntityType.Camera).OfType<Camera>())
                {
                    Console.WriteLine($"Camera: {camera.Name}");
                    Console.WriteLine(new string('-', 20)); // Separator for clarity

                    foreach (var streamUsage in camera.StreamUsages)
                    {
                        var stream = (VideoStream)engine.GetEntity(streamUsage.Stream);

                        Console.WriteLine($"Stream Usage: {streamUsage.Usage}");
                        Console.WriteLine($"Video Stream: {stream.Name} | Codec: {stream.VideoCompressionAlgorithm}");
                        Console.WriteLine(new string('-', 20)); // Separator for stream details

                        Console.WriteLine("Video Configuration:");
                        foreach (VideoCompressionConfiguration config in stream.VideoCompressions)
                        {
                            PrintVideoCompressionConfiguration(config);
                            Console.WriteLine(new string('-', 20)); // Separator for each configuration
                        }
                       
                        PrintQualityBoostConfiguration("Event Recording Configuration", camera.IsBoostQualityOnEventRecordingEnabled, stream.BoostQualityOnEventRecording);
                        PrintQualityBoostConfiguration("Manual Recording Configuration", camera.IsBoostQualityOnManualRecordingEnabled, stream.BoostQualityOnManualRecording);

                        Console.WriteLine();
                    }
                }

                void PrintVideoCompressionConfiguration(VideoCompressionConfiguration config)
                {
                    Console.WriteLine($"- BitRate: {config.BitRate} kbps");
                    Console.WriteLine($"- FrameRate: {config.FrameRate} fps");
                    Console.WriteLine($"- ImageQuality: {config.ImageQuality}");
                    Console.WriteLine($"- IsImageCropped: {config.IsImageCropped}");
                    Console.WriteLine($"- KeyframeInterval: {config.KeyframeInterval} seconds");
                    Console.WriteLine($"- RecordingFrameInterval: {config.RecordingFrameInterval}");
                    Console.WriteLine($"- Resolution: {config.ResolutionX}x{config.ResolutionY}");
                    if (config.ResolutionXPal > 0 && config.ResolutionYPal > 0)
                    {
                        Console.WriteLine($"- PAL Resolution: {config.ResolutionXPal}x{config.ResolutionYPal}");
                    }

                    Console.WriteLine($"- Schedule: {engine.GetEntity(config.Schedule)?.Name}");
                }

                void PrintQualityBoostConfiguration(string title, bool isEnabled, VideoCompressionConfiguration config)
                {
                    if (isEnabled && config != null)
                    {
                        Console.WriteLine($"{title}");
                        Console.WriteLine(new string('-', 20)); // Separator for quality boost configuration
                        PrintVideoCompressionConfiguration(config);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
