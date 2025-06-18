// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Roles;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Queries.Video;

SdkResolver.Initialize();

await RunSample();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"logon failed: {state}");
        return;
    }

    await LoadCameras(engine);

    List<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>().ToList();

    Console.WriteLine($"{cameras.Count} cameras loaded");

    IList<VideoSequence> videoSequences = await GetVideoSequences(engine, cameras);

    Console.WriteLine($"Retrieved {videoSequences.Count} video sequences.");

    foreach (VideoSequence sequence in videoSequences)
    {
        DisplayToConsole(engine, sequence);
    }
}

Task LoadCameras(Engine engine)
{
    Console.WriteLine("Loading cameras...");

    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.Camera);

    return Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
}

async Task<IList<VideoSequence>> GetVideoSequences(Engine engine, IEnumerable<Camera> cameras)
{
    Console.WriteLine("Retrieving video sequences in the last 24 hours...");

    var query = (SequenceQuery)engine.ReportManager.CreateReportQuery(ReportType.VideoSequence);
    query.TimeRange.SetTimeRange(TimeSpan.FromDays(1));
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

void DisplayToConsole(Engine engine, VideoSequence info)
{
    Console.WriteLine("Video Sequence Information:");
    Console.WriteLine($"{"Camera:",-25} {GetEntityName(info.CameraGuid)}");
    Console.WriteLine($"{"Archive Source:",-25} {GetEntityName(info.ArchiveSourceGuid)}");
    Console.WriteLine($"{"Start Time:",-25} {info.StartTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"{"End Time:",-25} {info.EndTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"{"Capabilities:",-25} {info.Capabilities}");
    Console.WriteLine($"{"Time Zone:",-25} {info.TimeZone}");
    Console.WriteLine($"{"Encoder:",-25} {GetEntityName(info.EncoderGuid)}");
    Console.WriteLine($"{"Usage GUID:",-25} {GetStreamUsageName(info.UsageGuid)}");
    Console.WriteLine($"{"Origin GUID:",-25} {GetEntityName(info.OriginGuid)}");
    Console.WriteLine($"{"Media Type:",-25} {GetMediaTypeName(info.MediaTypeGuid)}");
    Console.WriteLine($"{"Origin Type:",-25} {(OriginType)info.OriginType}");
    Console.WriteLine(new string('-', 50));

    string GetEntityName(Guid entityId) => engine.GetEntity(entityId) switch
    {
        Agent agent => engine.GetEntity(agent.RoleId)?.Name,
        Entity entity => entity.Name,
        _ => null
    };

    string GetMediaTypeName(Guid mediaTypeGuid)
    {
        if (mediaTypeGuid == MediaTypes.Video) return "Video";
        if (mediaTypeGuid == MediaTypes.AudioIn) return "Audio In";
        if (mediaTypeGuid == MediaTypes.AudioOut) return "Audio Out";
        if (mediaTypeGuid == MediaTypes.Metadata) return "Metadata";
        if (mediaTypeGuid == MediaTypes.Ptz) return "Ptz";
        if (mediaTypeGuid == MediaTypes.AgentPtz) return "Agent Ptz";
        if (mediaTypeGuid == MediaTypes.OverlayUpdate) return "Overlay Update";
        if (mediaTypeGuid == MediaTypes.OverlayStream) return "Overlay Stream";
        if (mediaTypeGuid == MediaTypes.EncryptionKey) return "Encryption Key";
        if (mediaTypeGuid == MediaTypes.CollectionEvents) return "Collection Events";
        if (mediaTypeGuid == MediaTypes.ArchiverEvents) return "Archiver Events";
        if (mediaTypeGuid == MediaTypes.OnvifAnalyticsStream) return "Onvif Analytics Stream";
        if (mediaTypeGuid == MediaTypes.BoschVcaStream) return "Bosch VCA Stream";
        if (mediaTypeGuid == MediaTypes.FusionStream) return "Fusion Stream";
        if (mediaTypeGuid == MediaTypes.FusionStreamEvents) return "Fusion Stream Events";
        if (mediaTypeGuid == MediaTypes.OriginalVideo) return "Original Video";
        if (mediaTypeGuid == MediaTypes.Block) return "Block";
        return "Unknown";
    }

    static string GetStreamUsageName(Guid streamUsage)
    {
        if (streamUsage == StreamUsage.Live) return "Live";
        if (streamUsage == StreamUsage.Archiving) return "Archiving";
        if (streamUsage == StreamUsage.Export) return "Export";
        if (streamUsage == StreamUsage.HighRes) return "High Resolution";
        if (streamUsage == StreamUsage.LowRes) return "Low Resolution";
        if (streamUsage == StreamUsage.Remote) return "Remote";
        if (streamUsage == StreamUsage.EdgePlayback) return "Edge Playback";
        return "Unknown";
    }
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
