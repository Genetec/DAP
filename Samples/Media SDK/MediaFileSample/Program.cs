// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk.Media;

SdkResolver.Initialize();

RunSample();

Console.Write("Press any key to exit...");
Console.ReadKey(true);

void RunSample()
{
    string filePath = ReadFilePath();
    try
    {
        // Create a MediaFile instance to inspect the file's metadata
        // This will analyze the file structure without loading the actual media content
        MediaFile mediaFile = new(filePath);
        Console.WriteLine($"\nMedia file path: {mediaFile}");

        // Display information about unique sources in the media file
        // A unique source represents a distinct stream of data (video, audio, metadata)
        Console.WriteLine("\nUnique Sources:");
        foreach (UniqueSource source in mediaFile.UniqueSources)
        {
            // Collection represents the device or system that generated the media
            Console.WriteLine($"Collection: {source.Collection} ({source.CollectionName})");

            // Encoder identifies the specific camera or encoding device
            Console.WriteLine($"Encoder: {source.Encoder} ({source.EncoderName})");

            // Origin provides information about where the stream came from
            Console.WriteLine($"Origin: {source.Origin}");

            // MediaType indicates the type of data (video, audio, metadata, etc.)
            Console.WriteLine($"Media Type: {source.MediaType} ({GetMediaTypeName(source.MediaType)})");

            // Usage indicates how the stream is intended to be used
            Console.WriteLine($"Usage: {source.Usage} ({GetUsageTypeName(source.Usage)})");

            // Quick flags for identifying video and audio streams
            Console.WriteLine($"Is Video: {source.IsVideo}, Is Audio: {source.IsAudio}");
            Console.WriteLine();
        }

        // Display information about individual files within the media container
        // A single media file can contain multiple segments or streams
        Console.WriteLine("Contained Files:");
        foreach (var file in mediaFile.ContainedFiles)
        {
            // Basic file information
            Console.WriteLine($"Filename: {file.Filename}");

            // Timing information in the file's original timezone
            Console.WriteLine($"Start Time: {file.StartTime}");
            Console.WriteLine($"End Time: {file.EndTime}");
            Console.WriteLine($"Time Zone: {file.TimeZone}");

            // Security features
            Console.WriteLine($"Is Watermarked: {file.IsWatermarked}");

            // Link back to the source that generated this file
            Console.WriteLine($"Associated Unique Source: {file.UniqueSource}");
            Console.WriteLine();
        }
    }
    catch (FormatException ex) // Caught when the file format is not recognized
    {
        Console.WriteLine(ex.Message);
    }
}

// Helper function to get a valid media file path from user input
static string ReadFilePath()
{
    while (true)
    {
        Console.Write("Please enter the path to a .g64, .g64x, or .mp4 file: ");
        string filePath = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(filePath))
        {
            Console.WriteLine("File path cannot be empty. Please try again.");
            continue;
        }
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"The file {filePath} does not exist. Please try again.");
            continue;
        }
        return filePath;
    }
}

// Converts Security Center media type GUIDs to human-readable names
// These types indicate what kind of data is stored in the stream
static string GetMediaTypeName(Guid mediaType) => mediaType switch
{
    _ when mediaType == StreamMediaType.Legacy => "Legacy",
    _ when mediaType == StreamMediaType.Video => "Video",
    _ when mediaType == StreamMediaType.AudioIn => "Audio In",
    _ when mediaType == StreamMediaType.AudioOut => "Audio Out",
    _ when mediaType == StreamMediaType.Metadata => "Metadata",
    _ when mediaType == StreamMediaType.Ptz => "PTZ",
    _ when mediaType == StreamMediaType.OverlayUpdate => "Overlay Update",
    _ when mediaType == StreamMediaType.OverlayStream => "Overlay Stream",
    _ when mediaType == StreamMediaType.CollectionEvents => "Collection Events",
    _ when mediaType == StreamMediaType.ArchiverEvents => "Archiver Events",
    _ => "Unknown"
};

// Converts Security Center usage type GUIDs to human-readable names
// These types indicate how the stream is intended to be used in the system
static string GetUsageTypeName(Guid usageType) => usageType switch
{
    _ when usageType == StreamUsageType.Live => "Live",
    _ when usageType == StreamUsageType.Archiving => "Archiving",
    _ when usageType == StreamUsageType.Export => "Export",
    _ when usageType == StreamUsageType.HighRes => "High Resolution",
    _ when usageType == StreamUsageType.LowRes => "Low Resolution",
    _ when usageType == StreamUsageType.Remote => "Remote",
    _ when usageType == StreamUsageType.EdgePlayback => "Edge Playback",
    _ => "Unknown"
};