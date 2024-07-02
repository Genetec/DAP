// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk;
    using Sdk.Entities;

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