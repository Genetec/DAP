// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media.Ptz;
using Genetec.Sdk.Queries;

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server: server, username: username, password: password);

    if (state == ConnectionStateCode.Success)
    {
        // Find the first PTZ camera that is running
        Camera camera = await FindPtzCamera();

        if (camera is not null)
        {
            // Load all networks entities into the entity cache
            await LoadNetworks();

            // Retrieve all the networks from the entity cache
            IEnumerable<Network> networks = engine.GetEntities(EntityType.Network).OfType<Network>();

            // The client subnet, for route resolving purpose
            // In this sample, we use the first network found, but you can use any network that is connected to the camera
            Network network = networks.First();

            using var manager = new PtzCoordinatesManager();
            manager.PtzStarted += (sender, e) => Console.WriteLine($"PTZ started. Coordinates: {e.Coordinates}\n");
            manager.PtzStopped += (sender, e) => Console.WriteLine($"PTZ stopped. Coordinates: {e.Coordinates}\n");
            manager.CoordinatesReceived += (sender, e) => Console.WriteLine($"Coordinates received. Coordinates: {e.Coordinates}\n");

            Console.WriteLine($"Initializing PTZ control for camera '{camera.Name}' using network '{network.Name}'.\n");
            manager.Initialize(sdkEngine: engine, cameraGuid: camera.Guid, clientSubnet: network.Guid);

            // Get the presets of the camera
            List<int> presets = Enumerable.Range(camera.PtzCapabilities.PresetBase, camera.PtzCapabilities.NumberOfPresets).ToList();

            if (presets.Any())
            {
                // Move the camera to each preset
                foreach (int preset in presets)
                {
                    // Get the name of the preset
                    string presetName = camera.GetPtzPresetName(preset);

                    // If the preset has no name, use the preset number
                    if (string.IsNullOrEmpty(presetName))
                    {
                        presetName = $"Preset {preset}";
                    }

                    Console.WriteLine($"Moving to {presetName}... (Preset number: {preset})");
                    manager.ControlPtz(PtzCommandType.GoToPreset, preset, 0);   // Move the camera to the preset

                    Console.WriteLine("Pausing for 2 seconds before next movement.\n");
                    await Task.Delay(2000);  // Wait for 2 seconds before moving to the next preset
                }

                Console.WriteLine($"Completed moving through {presets.Count} presets.\n");
            }
            else
            {
                Console.WriteLine($"No presets found for camera '{camera.Name}'.\n");
            }
        }
        else
        {
            Console.WriteLine("No suitable PTZ camera found. Ensure a PTZ camera is running and supports the GoToPreset command.\n");
        }
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine(value: "Press any key to exit...");
    Console.ReadKey();

    // Find the first PTZ camera that is running and supports the GoToPreset command
    async Task<Camera> FindPtzCamera()
    {
        Console.WriteLine("Loading cameras...\n");

        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);

        query.EntityTypeFilter.Add(EntityType.Camera); // Only fetch cameras
        query.Page = 1; // Start at page 1
        query.PageSize = 100; // Fetch 100 cameras at a time

        QueryCompletedEventArgs args;

        do
        {
            // Fetch the current page of cameras
            QueryCompletedEventArgs result = args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            // Find the first PTZ camera that is running and supports the GoToPreset command
            Camera camera = result.Data.AsEnumerable()
                .Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid))))
                .OfType<Camera>()
                .FirstOrDefault(IsPtzCamera);

            // If we found a camera, return it
            if (camera != null)
            {
                return camera;
            }

            // If we didn't find a camera, move to the next page
            query.Page++;

            // If there are more rows than the page size, we need to fetch them
        } while (args.Data.Rows.Count > query.PageSize);

        return null;

        // Predicate to check if a camera is a PTZ camera that is running and supports the GoToPreset command
        bool IsPtzCamera(Camera camera)
            => camera.IsPtz && camera.PtzCapabilities.IsSupportedCommand(PtzCommandType.GoToPreset) && camera.RunningState == State.Running;
    }

    // Load all networks entities into the entity cache
    async Task LoadNetworks()
    {
        Console.WriteLine("Loading networks...\n");

        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);

        query.EntityTypeFilter.Add(EntityType.Network); // Only fetch networks

        // Fetch the networks
        await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }
}