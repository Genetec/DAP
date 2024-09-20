// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Sdk;
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

        // Replace with your actual camera GUID
        Guid cameraGuid = Guid.Parse("00000000-0000-0000-0000-000000000000");

        using var engine = new Engine();

        ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

        if (state == ConnectionStateCode.Success)
        {
            await LoadApplications();
            DisplayCameraInTiles();
        }
        else
        {
            Console.WriteLine($"Logon failed: {state}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        // Load all applications into the engine
        async Task LoadApplications()
        {
            Console.WriteLine("Loading applications...");
            var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
            query.EntityTypeFilter.Add(EntityType.Application);
            await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        }

        // Display the camera feed in tiles on all available Security Desk monitors
        void DisplayCameraInTiles()
        {
            Console.WriteLine("Searching for online Security Desk applications...");

            // Get all online Security Desk applications
            var securityDeskApps = engine.GetEntities(EntityType.Application)
                .OfType<Application>()
                .Where(app => app.ApplicationType == ApplicationType.SecurityDesk && app.IsOnline)
                .ToList();

            Console.WriteLine($"Found {securityDeskApps.Count} online Security Desk application(s).");

            Console.WriteLine("Retrieving monitors from Security Desk applications...");

            // Get all monitors from these applications
            var monitors = securityDeskApps.SelectMany(app => app.Monitors)
                .Select(engine.GetEntity)
                .OfType<Monitor>()
                .Select(monitor => monitor.MonitorId)
                .ToList();

            Console.WriteLine($"Found {monitors.Count} monitor(s).");

            // Display camera on each monitor
            foreach (var monitor in monitors)
            {

                string xmlContent = $@"
<TileContentGroup>
    <Contents>
        <VideoContent Camera=""{cameraGuid}"" VideoMode=""Live"" />
    </Contents>
</TileContentGroup>";

                Console.WriteLine($"Displaying camera on monitor ID: {monitor}");
                engine.ActionManager.DisplayInTile(monitor, xmlContent);
            }
        }
    }
}