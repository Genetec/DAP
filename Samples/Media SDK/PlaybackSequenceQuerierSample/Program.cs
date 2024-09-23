// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk.Queries;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media.Reader;

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state == ConnectionStateCode.Success)
    {
        // Load cameras into the entity cache
        await LoadCameras();

        // Retrieve cameras from the entity cache
        List<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>().ToList();

        Console.WriteLine($"{cameras.Count} cameras loaded");

        // Query and display video sequences for each camera
        foreach (Camera camera in cameras)
        {
            await QueryAndDisplayVideoSequence(camera);
        }
    }
    else
    {
        Console.WriteLine($"Login failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();

    async Task LoadCameras()
    {
        Console.WriteLine("Loading cameras...");

        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Camera);
        await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }

    async Task QueryAndDisplayVideoSequence(Camera camera)
    {
        Console.WriteLine($"\nQuerying {camera.Name} (ID: {camera.Guid}) for video sequences in the last 24 hours:");

        await using var reader = PlaybackSequenceQuerier.CreateVideoQuerier(engine, camera.Guid);

        // Query video sequences in the last 24 hours
        var timeRange = new Genetec.Sdk.Media.DateTimeRange(DateTime.Now.AddDays(-1), DateTime.Now);

        List<PlaybackSequence> sequences = await reader.QuerySequencesAsync(timeRange);

        if (sequences.Any())
        {
            Console.WriteLine($"Found {sequences.Count} video sequences:");
            foreach (PlaybackSequence sequence in sequences)
            {
                TimeSpan duration = sequence.Range.EndTime - sequence.Range.StartTime;
                Console.WriteLine($"  {sequence.Range.StartTime:yyyy-MM-dd HH:mm:ss} to {sequence.Range.EndTime:yyyy-MM-dd HH:mm:ss} (Duration: {duration:hh\\:mm\\:ss})");
            }
        }
        else
        {
            Console.WriteLine("No video sequences found in the last 24 hours.");
        }

        Console.WriteLine(new string('-', 50));
    }

}