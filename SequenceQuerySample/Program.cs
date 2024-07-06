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
        await LoadCameras(engine);

        // Retrieve cameras from the entity cache
        List<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>().ToList();
        Console.WriteLine($"{cameras.Count} cameras loaded");

        // Retrieve video sequences for all cameras in the last 24 hours
        IList<VideoSequence> sequences = await GetSequences(engine, cameras);

        Console.WriteLine($"Retrieved {sequences.Count} video sequences.");

        // Display the video sequences
        foreach (VideoSequence sequence in sequences)
        {
            DisplayToConsole(sequence);
        }
    }
    else
    {
        Console.WriteLine($"logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

Task LoadCameras(Engine engine)
{
    Console.WriteLine("Loading cameras...");

    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.Camera);

    return Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
}

async Task<IList<VideoSequence>> GetSequences(Engine engine, IEnumerable<Camera> cameras)
{
    Console.WriteLine("Retrieving video sequences in the last 24 hours...");

    var query = (SequenceQuery)engine.ReportManager.CreateReportQuery(ReportType.VideoSequence);
    query.TimeRange.SetTimeRange(DateTime.Now.AddDays(-1), DateTime.Now);
    query.Cameras.AddRange(cameras.Select(camera => camera.Guid));

    QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

    return args.Data.AsEnumerable().Select(CreateFromDataRow).ToList();

    VideoSequence CreateFromDataRow(DataRow row) => new()
    {
        CameraGuid = row.Field<Guid>(SequenceQuery.CameraGuidColumnName),
        ArchiveSourceGuid = row.Field<Guid>(SequenceQuery.ArchiveSourceGuidColumnName),
        StartTime = row.Field<DateTime>(SequenceQuery.StartTimeColumnName),
        EndTime = row.Field<DateTime>(SequenceQuery.EndTimeColumnName),
        Capabilities = row.Field<uint>(SequenceQuery.CapabilitiesColumnName),
        TimeZone = row.Field<string>(SequenceQuery.TimeZoneColumnName),
        EncoderGuid = row.Field<Guid>(SequenceQuery.EncoderGuidColumnName),
        UsageGuid = row.Field<Guid>(SequenceQuery.UsageGuidColumnName),
        OriginGuid = row.Field<Guid>(SequenceQuery.OriginGuidColumnName),
        MediaTypeGuid = row.Field<Guid>(SequenceQuery.MediaTypeGuidColumnName),
        OriginType = row.Field<int>(SequenceQuery.OriginTypeColumnName)
    };
}

 void DisplayToConsole(VideoSequence info)
{
    Console.WriteLine("Video Sequence Information:");
    Console.WriteLine($"{"Camera GUID:",-25} {info.CameraGuid}");
    Console.WriteLine($"{"Archive Source GUID:",-25} {info.ArchiveSourceGuid}");
    Console.WriteLine($"{"Start Time (UTC):",-25} {info.StartTime:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"{"End Time (UTC):",-25} {info.EndTime:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"{"Capabilities:",-25} {info.Capabilities}");
    Console.WriteLine($"{"Time Zone:",-25} {info.TimeZone}");
    Console.WriteLine($"{"Encoder GUID:",-25} {info.EncoderGuid}");
    Console.WriteLine($"{"Usage GUID:",-25} {info.UsageGuid}");
    Console.WriteLine($"{"Origin GUID:",-25} {info.OriginGuid}");
    Console.WriteLine($"{"Media Type GUID:",-25} {info.MediaTypeGuid}");
    Console.WriteLine($"{"Origin Type:",-25} {info.OriginType}");
    Console.WriteLine(new string('-', 50));
}

class VideoSequence
{
    public Guid CameraGuid { get; set; }
    public Guid ArchiveSourceGuid { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public uint Capabilities { get; set; }
    public string TimeZone { get; set; }
    public Guid EncoderGuid { get; set; }
    public Guid UsageGuid { get; set; }
    public Guid OriginGuid { get; set; }
    public Guid MediaTypeGuid { get; set; }
    public int OriginType { get; set; }
}