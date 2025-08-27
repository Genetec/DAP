// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Queries.Video;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

SdkResolver.Initialize();

await RunSample();

Console.WriteLine("Press any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    const string server = "localhost";  // Specify the IP address or hostname of your Security Center server.
    const string username = "admin";    // Set the username for Security Center authentication.
    const string password = "";         // Set the corresponding password for the specified username.

    using var engine = new Engine();
    engine.LoginManager.LogonStatusChanged += (sender, args) => Console.WriteLine($"Logon status: {args.Status}");
    engine.LoginManager.LogonFailed += (sender, e) => Console.WriteLine($"Logon Failed | Error Message: {e.FormattedErrorMessage} | Error Code: {e.FailureCode}");

    // Set up cancellation support
    using var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        Console.WriteLine("Cancelling...");
        e.Cancel = true;
        cancellationTokenSource.Cancel();
    };

    Console.WriteLine($"Logging to {server}... Press Ctrl+C to cancel");
    ConnectionStateCode state = await engine.LoginManager.LogOnAsync(server, username, password, cancellationTokenSource.Token);
    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"Logon failed: {state}");
        return;
    }

    Console.WriteLine("Loading cameras...");
    await LoadCameras(engine);

    Console.WriteLine("Retrieving camera events...");
    // Fetch camera events for the last 24 hours

    DateTime from = DateTime.UtcNow.AddDays(-1);
    DateTime to = DateTime.UtcNow;
    IEnumerable<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>();

    ICollection<CameraEventDetail> events = await GetCameraEvents(engine, cameras.Select(camera => camera.Guid), from, to);
    PrintCameraEvents(engine, events);
}

async Task LoadCameras(Engine engine)
{
    var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
    query.EntityTypeFilter.Add(EntityType.Camera);
    query.DownloadAllRelatedData = true;
    query.Page = 1;
    query.PageSize = 50;

    QueryCompletedEventArgs args;
    do
    {
        args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

        query.Page++;
    } while (args.Data.Rows.Count >= query.PageSize);
}

async Task<ICollection<CameraEventDetail>> GetCameraEvents(Engine engine, IEnumerable<Guid> cameras, DateTime from, DateTime to)
{
    var query = (CameraEventQuery)engine.ReportManager.CreateReportQuery(ReportType.CameraEvent);
    query.TimeRange.SetTimeRange(from, to);
    query.Cameras.AddRange(cameras);

    QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    return args.Data.AsEnumerable().Select(CreateFromDataRow).ToList();

    CameraEventDetail CreateFromDataRow(DataRow row) => new()
    {
        CameraGuid = row.Field<Guid>(VideoEventQuery.CameraGuidColumnName),
        ArchiveSourceGuid = row.Field<Guid>(VideoEventQuery.ArchiveSourceGuidColumnName),
        EventTime = row.Field<DateTime>(VideoEventQuery.EventTimeColumnName),
        EventType = (EventType)row.Field<uint>(VideoEventQuery.EventTypeColumnName),
        Value = row.Field<uint>(VideoEventQuery.ValueColumnName),
        Notes = row.Field<string>(VideoEventQuery.NotesColumnName),
        XmlData = row.Field<string>(VideoEventQuery.XmlDataColumnName),
        Capabilities = row.Field<uint>(VideoEventQuery.CapabilitiesColumnName),
        TimeZone = row.Field<string>(VideoEventQuery.TimeZoneColumnName),
        Thumbnail = row.Field<byte[]>(VideoEventQuery.ThumbnailColumnName)
    };
}

void PrintCameraEvents(Engine engine, ICollection<CameraEventDetail> events)
{
    if (!events.Any())
    {
        Console.WriteLine("No camera events found in the specified time range.");
        return;
    }

    Console.WriteLine($"\nFound {events.Count} camera events:");
    Console.WriteLine(new string('-', 80));

    foreach (CameraEventDetail detail in events.OrderBy(e => e.EventTime))
    {
        Console.WriteLine($"Camera: {GetEntityName(detail.CameraGuid)}");
        Console.WriteLine($"  Event Type: {detail.EventType} (Value: {detail.Value})");
        Console.WriteLine($"  Event Time: {detail.EventTime.ToLocalTime()}");
        Console.WriteLine($"  Archive Source: {GetEntityName(detail.ArchiveSourceGuid)}");
        Console.WriteLine($"  Time Zone: {detail.TimeZone}");

        if (!string.IsNullOrEmpty(detail.Notes))
        {
            Console.WriteLine($"  Notes: {detail.Notes}");
        }

        if (!string.IsNullOrEmpty(detail.XmlData))
        {
            Console.WriteLine($"  XML Data: {detail.XmlData}");
        }

        if (detail.Thumbnail?.Length > 0)
        {
            Console.WriteLine($"  Thumbnail: {detail.Thumbnail.Length} bytes");
        }

        Console.WriteLine();
    }

    string GetEntityName(Guid entityId) => engine.GetEntity(entityId)?.Name ?? "Unknown";
}

record CameraEventDetail
{
    public Guid CameraGuid { get; init; }
    public Guid ArchiveSourceGuid { get; init; }
    public DateTime EventTime { get; init; }
    public EventType EventType { get; init; }
    public uint Value { get; init; }
    public string Notes { get; init; }
    public string XmlData { get; init; }
    public uint Capabilities { get; init; }
    public string TimeZone { get; init; }
    public byte[] Thumbnail { get; set; }
}