// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;

namespace Genetec.Dap.CodeSamples;

public class DisplayInTileSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Replace with your actual camera GUID
        Guid cameraGuid = Guid.Parse("your-camera-guid-here");

        await LoadEntities(engine, token, EntityType.Application);
        DisplayCameraInTiles(engine, cameraGuid);
    }


    // Display the camera feed in tiles on all available Security Desk monitors
    private static void DisplayCameraInTiles(Engine engine, Guid cameraGuid)
    {
        if (engine.GetEntity(cameraGuid) is not Camera)
        {
            Console.WriteLine($"Camera with GUID {cameraGuid} not found.");
            return;
        }

        Console.WriteLine("Searching for online Security Desk applications...");

        // Get all online Security Desk applications
        List<Application> securityDeskApps = engine.GetEntities(EntityType.Application)
            .OfType<Application>()
            .Where(app => app.ApplicationType == ApplicationType.SecurityDesk && app.IsOnline)
            .ToList();

        Console.WriteLine($"Found {securityDeskApps.Count} online Security Desk application(s).");

        Console.WriteLine("Retrieving monitors from Security Desk applications...");

        // Get all monitors from these applications
        List<int> monitors = securityDeskApps.SelectMany(app => app.Monitors)
            .Select(engine.GetEntity)
            .OfType<Sdk.Entities.Monitor>()
            .Select(monitor => monitor.LogicalId.GetValueOrDefault())
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