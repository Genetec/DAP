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
    using Genetec.Sdk.Queries;
    using Genetec.Sdk;
    using Genetec.Sdk.Entities;
    using Genetec.Sdk.Media.Reader;

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
                int cameraCount = await LoadCameras();
                Console.WriteLine($"Loaded {cameraCount} cameras into the entity cache.");

                IEnumerable<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>();
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

            async Task<int> LoadCameras()
            {
                Console.WriteLine("Loading cameras...");

                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.Camera);
                query.Page = 1;
                query.PageSize = 1000;

                int totalCameras = 0;
                QueryCompletedEventArgs args;
                do
                {
                    args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
                    totalCameras += Math.Min(args.Data.Rows.Count, query.PageSize);
                    Console.WriteLine($"  Loaded page {query.Page}: {args.Data.Rows.Count} cameras. Total: {totalCameras}");
                    query.Page++;

                } while (args.Error == ReportError.TooManyResults || args.Data.Rows.Count >= query.PageSize);

                return totalCameras;
            }

            async Task QueryAndDisplayVideoSequence(Camera camera)
            {
                Console.WriteLine($"\nQuerying {camera.Name} (ID: {camera.Guid}) for video sequences in the last 24 hours:");
                
                await using var reader = PlaybackSequenceQuerier.CreateVideoQuerier(engine, camera.Guid);
                
                var timeRange = new Sdk.Media.DateTimeRange(DateTime.Now.AddDays(-1), DateTime.Now);
                
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
    }
}