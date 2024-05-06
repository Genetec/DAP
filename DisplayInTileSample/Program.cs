// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk;
    using Sdk.Entities;
    using Sdk.Queries;

    internal class Program
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
                await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }


        async Task DisplayCameraInSecurityDesk(Engine engine, Guid cameraGuid)
        {
            var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
            query.EntityTypeFilter.Add(EntityType.Application);

            await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            var applications = engine.GetEntities(EntityType.Application).OfType<Application>().Where(application => application.ApplicationType == ApplicationType.SecurityDesk);

            foreach (var application in applications)
            {
                engine.ActionManager.DisplayEntityInSecurityDesk(application.LoggedUserGuid, cameraGuid, true);
            }

        }

        async Task LoadApplicationInToEngineEntityCache(Engine engine)
        {
            var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
            query.EntityTypeFilter.Add(EntityType.Application);
            await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        }

        static void DisplayInTile(Engine engine, Guid cameraGuid)
        {
            var monitors = engine.GetEntities(EntityType.Application)
                .OfType<Application>()
                .Where(application => application.ApplicationType == ApplicationType.SecurityDesk && application.IsOnline)
                .SelectMany(application => application.Monitors)
                .Select(engine.GetEntity).OfType<Monitor>().Select(monitor => monitor.MonitorId);

            foreach (var monitor in monitors)
            {
                string xmlContent = $"<TileContentGroup>\r\n    <Contents>\r\n        <VideoContent Camera=\"{cameraGuid}\" VideoMode=\"Live\" />\r\n    </Contents>\r\n</TileContentGroup>";

                engine.ActionManager.DisplayInTile(monitor, xmlContent);
            }
        }
    }
}