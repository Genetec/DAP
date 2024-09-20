// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.IO;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk.Media;

SdkResolver.Initialize();

RunSample();

void RunSample()
{
    string filePath = ReadFilePath();
    try
    {
        MediaFile mediaFile = new(filePath);

        Console.WriteLine($"\nMedia file path: {mediaFile}");

        Console.WriteLine("\nUnique Sources:");
        foreach (UniqueSource source in mediaFile.UniqueSources)
        {
            Console.WriteLine($"Collection: {source.Collection} ({source.CollectionName})");
            Console.WriteLine($"Encoder: {source.Encoder} ({source.EncoderName})");
            Console.WriteLine($"Origin: {source.Origin}");
            Console.WriteLine($"Media Type: {source.MediaType} ({GetMediaTypeName(source.MediaType)})");
            Console.WriteLine($"Usage: {source.Usage} ({GetUsageTypeName(source.Usage)})");
            Console.WriteLine($"Is Video: {source.IsVideo}, Is Audio: {source.IsAudio}");
            Console.WriteLine();
        }

        Console.WriteLine("Contained Files:");
        foreach (MediaFile.ContainedFile file in mediaFile.ContainedFiles)
        {
            Console.WriteLine($"Filename: {file.Filename}");
            Console.WriteLine($"Start Time: {file.StartTime}");
            Console.WriteLine($"End Time: {file.EndTime}");
            Console.WriteLine($"Time Zone: {file.TimeZone}");
            Console.WriteLine($"Is Watermarked: {file.IsWatermarked}");
            Console.WriteLine($"Associated Unique Source: {file.UniqueSource}");
            Console.WriteLine();
        }
    }
    catch (FormatException ex) // The file is not a valid media file
    {
        Console.WriteLine(ex.Message);
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

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