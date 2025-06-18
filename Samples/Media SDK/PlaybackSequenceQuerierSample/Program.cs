// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk.Queries;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media.Reader;
using System.Threading;
using DateTimeRange = Genetec.Sdk.Media.DateTimeRange;

SdkResolver.Initialize();

await RunSample();

Console.Write("Press any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    // Connection parameters for your Security Center server
    const string server = "localhost";  // Specify the IP address or hostname of your Security Center server.
    const string username = "admin";    // Enter the username for Security Center authentication.
    const string password = "";         // Provide the corresponding password for the specified username.

    // The Engine class is the main entry point for SDK operations.
    // It manages the connection to Security Center and provides access to the system's entities.
    using var engine = new Engine();

    // Set up event handlers to monitor connection status
    engine.LogonStatusChanged += (_, args) => Console.Write($"\rConnection status: {args.Status}".PadRight(Console.WindowWidth));
    engine.LogonFailed += (_, args) => Console.WriteLine($"\rError: {args.FormattedErrorMessage}".PadRight(Console.WindowWidth));
    engine.LoggedOn += (_, args) => Console.WriteLine($"\rConnected to {args.ServerName}".PadRight(Console.WindowWidth));

    // Attempt to connect to the Security Center server
    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
    {
        return;
    }

    // Load cameras into the entity cache
    await LoadCameras();

    // Get the list of cameras from the entity cache
    List<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>().ToList();

    Console.WriteLine($"\r{cameras.Count} cameras loaded".PadRight(Console.WindowWidth));

    // Set up cancellation support
    using var cancellationTokenSource = new CancellationTokenSource();

    Console.CancelKeyPress += (_, e) =>
    {
        Console.WriteLine("Cancelling...");
        e.Cancel = true;
        cancellationTokenSource.Cancel();
    };

    // Define the time range for video sequence query (last 24 hours)
    var timeRange = new DateTimeRange(DateTime.Now.AddDays(-1), DateTime.Now);

    // Query each camera for video sequences
    foreach (Camera camera in cameras)
    {
        Console.WriteLine($"\nQuerying {camera.Name} (ID: {camera.Guid}) for video sequences in the last 24 hours:");
        await QueryAndDisplayVideoSequence(camera, timeRange, cancellationTokenSource.Token);
    }

    // LoadCameras uses the EntityConfigurationQuery to retrieve camera information
    // This is more efficient than loading all entities when we only need cameras
    async Task LoadCameras()
    {
        Console.Write("Loading cameras...");

        // Create a query specifically for entity configuration
        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);

        // Filter the query to only return camera entities
        query.EntityTypeFilter.Add(EntityType.Camera);

        // Execute the query asynchronously
        await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }

    // QueryAndDisplayVideoSequence uses the PlaybackSequenceQuerier to find recorded video segments
    // This helps identify when video recordings are available for a camera
    async Task QueryAndDisplayVideoSequence(Camera camera, DateTimeRange timeRange, CancellationToken token = default)
    {
        // Create a video querier for the specific camera
        // The querier helps find video sequences (recordings) for a given time range
        await using var reader = PlaybackSequenceQuerier.CreateVideoQuerier(engine, camera.Guid);

        // Query for sequences within our time range
        List<PlaybackSequence> sequences = await reader.QuerySequencesAsync(timeRange, token);

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
