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
using Genetec.Sdk.Queries;
using Genetec.Sdk.Queries.Video;

const string server = "localhost";
const string username = "admin";
const string password = "";

SdkResolver.Initialize();

using var engine = new Engine();

ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

if (state == ConnectionStateCode.Success)
{
    await LoadCameras();

    List<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>().ToList();
    Console.WriteLine($"Loaded {cameras.Count} cameras.");

    IEnumerable<Thumbnail> thumbnails = await GetThumbnails(cameras);

    foreach (Thumbnail thumbnail in thumbnails)
    {
        Console.WriteLine("Thumbnail Details:");
        Console.WriteLine($"  {"Camera:",-16} {engine.GetEntity(thumbnail.Camera).Name}");
        Console.WriteLine($"  {"Timestamp:",-16} {thumbnail.Timestamp.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"  {"Latest Frame:",-16} {thumbnail.LatestFrame.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"  {"Size:",-16} {thumbnail.Data?.Length ?? 0} bytes");
        Console.WriteLine($"  {"Context:",-16} {thumbnail.Context}");
        Console.WriteLine(new string('-', 50));
    }
}
else
{
    Console.WriteLine($"logon failed: {state}");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

Task LoadCameras()
{
    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.Camera);

    return Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
}

async Task<IEnumerable<Thumbnail>> GetThumbnails(IEnumerable<Camera> cameras)
{
    var query = (VideoThumbnailQuery)engine.ReportManager.CreateReportQuery(ReportType.Thumbnail);

    const int thumbnailWidth = 200;

    foreach (var camera in cameras)
    {
        query.AddTimestamp(camera: camera.Guid, timestamp: DateTime.UtcNow.AddSeconds(-1), width: thumbnailWidth);
    }

    QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

    return args.Data.AsEnumerable().Select(CreateFromDataRow).ToList();

    Thumbnail CreateFromDataRow(DataRow row) => new()
    {
        Camera = row.Field<Guid>(VideoThumbnailQuery.CameraColumnName),
        Data = row.Field<byte[]>(VideoThumbnailQuery.ThumbnailColumnName),
        Timestamp = row.Field<DateTime>(VideoThumbnailQuery.TimestampColumnName),
        LatestFrame = row.Field<DateTime>(VideoThumbnailQuery.LatestFrameColumnName),
        Context = row.Field<Guid>(VideoThumbnailQuery.ContextColumnName)
    };
}

public class Thumbnail
{
    public Guid Camera { get; set; }
    public byte[] Data { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime LatestFrame { get; set; }
    public Guid Context { get; set; }
}