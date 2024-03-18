// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Media.Ptz;
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
            var state = await engine.LogOnAsync(server: server, username: username, password: password);

            if (state == ConnectionStateCode.Success)
            {
                var camera = await FindPtzCamera();
                
                var manager = new PtzCoordinatesManager();
                manager.Initialize(sdkEngine: engine, cameraGuid: camera.Guid);
                manager.ControlPtz(PtzCommandType.GoToPreset,  camera.PtzCapabilities.PatternBase, 0);
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }
       

            Console.WriteLine(value: "Press any key to exit...");
            Console.ReadKey();

            async Task<Camera> FindPtzCamera()
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
                        .FirstOrDefault(c => c.IsPtz && c.RunningState == State.Running);
                    
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