// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk.Entities;
    using Sdk.Media.Overlay;
    using Sdk;
    using Sdk.Queries;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                OverlayFactory.Initialize(engine);

                Camera camera = await FindSupportedCamera();

                if (camera is null)
                {
                    Console.WriteLine("No camera found");
                }
                else
                {
                    Console.WriteLine($"Drawing overlays on camera: {camera.Name}");

                    var timecodeOverlay = new TimecodeOverlay(camera.Guid);
                    timecodeOverlay.Start();

                    var bouncingBall = new BouncingBallOverlay(camera.Guid);
                    bouncingBall.Start();
                }
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            async Task<Camera> FindSupportedCamera()
            {
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.Camera);
                query.Page = 1;
                query.PageSize = 50;

                QueryCompletedEventArgs args;
                do
                {
                    var result = args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

                    var camera = result.Data.AsEnumerable().Take(query.PageSize)
                        .Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid))))
                        .OfType<Camera>()
                        .FirstOrDefault(camera => camera.IsOverlaySupported && camera.RunningState == State.Running);

                    if (camera != null)
                    {
                        return camera;
                    }

                    query.Page++;

                } while (args.Error == ReportError.TooManyResults || args.Data.Rows.Count > query.PageSize);

                return null;
            }
        }
    }
}
