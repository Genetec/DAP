// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk;
    using Sdk.Entities;

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
                // Replace with your own camera GUID
                Guid cameraGuid = new Guid("YOUR_CAMERA_GUID_HERE");

                DisplayInTile(engine, cameraGuid);
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void DisplayInTile(Engine engine, Guid cameraGuid)
        {
            IEnumerable<int> monitors = engine.GetEntities(EntityType.Application)
                .OfType<Application>()
                .Where(application => application.ApplicationType == ApplicationType.SecurityDesk && application.IsOnline)
                .SelectMany(application => application.Monitors)
                .Select(engine.GetEntity).OfType<Monitor>().Select(monitor => monitor.MonitorId);

            foreach (var monitor in monitors)
            {
                string xmlContent = $@"
<TileContentGroup>
    <Contents>
        <VideoContent Camera=""{cameraGuid}"" VideoMode=""Live"" />
    </Contents>
</TileContentGroup>";

                engine.ActionManager.DisplayInTile(monitor, xmlContent);
            }
        }
    }
}