// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media.Reader;
using DateTimeRange = Genetec.Sdk.Media.DateTimeRange;
namespace Genetec.Dap.CodeSamples;

class PlaybackSequenceQuerierSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load cameras into the entity cache
        await LoadEntities(engine, token, EntityType.Camera);

        // Get the list of cameras from the entity cache
        List<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>().ToList();

        Console.WriteLine($"\r{cameras.Count} cameras loaded".PadRight(Console.WindowWidth));

        // Define the time range for video sequence query (last 24 hours)
        var timeRange = new DateTimeRange(DateTime.Now.AddDays(-1), DateTime.Now);

        // Query each camera for video sequences
        foreach (Camera camera in cameras)
        {
            Console.WriteLine($"\nQuerying {camera.Name} (ID: {camera.Guid}) for video sequences in the last 24 hours:");
            await QueryAndDisplayVideoSequence(engine, camera, timeRange, token);
        }
    }

    // QueryAndDisplayVideoSequence uses the PlaybackSequenceQuerier to find recorded video segments
    async Task QueryAndDisplayVideoSequence(Engine engine, Camera camera, DateTimeRange timeRange, CancellationToken token = default)
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