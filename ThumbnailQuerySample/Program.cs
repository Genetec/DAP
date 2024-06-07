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
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk.Queries;
    using Genetec.Sdk.Queries.Video;
    using Genetec.Sdk;

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
                List<Guid> cameras = await GetCameras();

                if (cameras.Any())
                {
                    IEnumerable<Thumbnail> thumbnails = await GetThumbnails(cameras);

                    foreach (var thumbnail in thumbnails)
                    {
                        Console.WriteLine("Thumbnail Details:");
                        Console.WriteLine($"  Camera:        {engine.GetEntity(thumbnail.Camera).Name}");
                        Console.WriteLine($"  Timestamp:     {thumbnail.Timestamp.ToLocalTime()}");
                        Console.WriteLine($"  Latest Frame:  {thumbnail.LatestFrame.ToLocalTime()}");
                        Console.WriteLine($"  Size:          {thumbnail.Data?.Length ?? 0} bytes");
                        Console.WriteLine($"  Context:       {thumbnail.Context}");
                        Console.WriteLine(new string('-', 40));
                    }
                }
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            async Task<List<Guid>> GetCameras()
            {
                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.Camera);
                query.Page = 1;
                query.PageSize = 50;

                var cameras = new List<Guid>();

                QueryCompletedEventArgs args;
                do
                {
                    args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

                    cameras.AddRange(args.Data.AsEnumerable().Take(query.PageSize).Select(row => row.Field<Guid>(nameof(Guid))));

                    query.Page++;

                } while (args.Error == ReportError.TooManyResults || args.Data.Rows.Count > query.PageSize);

                return cameras;
            }

            async Task<IEnumerable<Thumbnail>> GetThumbnails(List<Guid> cameras)
            {
                var query = (VideoThumbnailQuery)engine.ReportManager.CreateReportQuery(ReportType.Thumbnail);

                const int thumbnailWidth = 200;

                foreach (var camera in cameras)
                {
                    query.AddTimestamp(camera: camera, timestamp: DateTime.UtcNow.AddSeconds(-1), width: thumbnailWidth);
                }

                QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

                return args.Data.AsEnumerable().Select(row => new Thumbnail
                {
                    Camera = row.Field<Guid>(VideoThumbnailQuery.CameraColumnName),
                    Data = row.Field<byte[]>(VideoThumbnailQuery.ThumbnailColumnName),
                    Timestamp = row.Field<DateTime>(VideoThumbnailQuery.TimestampColumnName),
                    LatestFrame = row.Field<DateTime>(VideoThumbnailQuery.LatestFrameColumnName),
                    Context = row.Field<Guid>(VideoThumbnailQuery.ContextColumnName)
                });
            }
        }
    }
}