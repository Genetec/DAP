// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Events.Video;
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

            engine.SetEventFilter(new[] { EventType.CameraMotionOn, EventType.CameraMotionOff });
            engine.EventReceived += (sender, e) =>
            {
                if (e.Event is CameraEvent cameraEvent)
                {
                    // Process CameraEvent
                }
            };

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                await LoadCameras();
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            async Task LoadCameras()
            {
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.Camera);
                await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            }
        }
    }
}
