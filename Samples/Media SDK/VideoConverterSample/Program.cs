// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk.Media.Export;
using File = System.IO.File;

SdkResolver.Initialize();

string mediaFile = ReadFilePath();
await ProcessFile(mediaFile);

Console.WriteLine("Press any key to exit...");
Console.ReadKey(true);

string ReadFilePath()
{
    while (true)
    {
        Console.Write("Enter media file path: ");
        string input = Console.ReadLine();
        if (File.Exists(input))
        {
            string extension = Path.GetExtension(input).ToLower();
            if (extension is ".g64" or ".g64x")
            {
                return input;
            }
            Console.WriteLine("Invalid file type. Please enter a .g64, or .g64x file.");
        }
        else
        {
            Console.WriteLine("File not found. Please enter a valid file path.");
        }
    }
}

async Task ProcessFile(string filePath)
{
    var cancellationTokenSource = new CancellationTokenSource();

    Console.WriteLine("Press Ctrl+C to cancel the conversion.");

    Console.CancelKeyPress += (sender, args) =>
    {
        Console.WriteLine("\nCancellation requested.");
        args.Cancel = true; // Prevents the process from terminating
        cancellationTokenSource.Cancel();
    };

    Console.WriteLine("Converting to MP4...");
    try
    {
        string convertedFile = await ConvertToMp4(filePath, true, new Progress<(int Percent, string Message)>(ReportProgress), cancellationTokenSource.Token);
        Console.WriteLine($"\nConversion to MP4 completed: {convertedFile}");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("\nMP4 conversion cancelled.");
        return;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"MP4 conversion failed: {ex.Message}");
    }

    Console.WriteLine("\nConverting to ASF...");
    try
    {
        string convertedFile = await ConvertToAsf(filePath, true, new Progress<(int Percent, string Message)>(ReportProgress), cancellationTokenSource.Token);
        Console.WriteLine($"\nConversion to ASF completed: {convertedFile}");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("\nASF conversion cancelled.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ASF conversion failed: {ex.Message}");
    }

    void ReportProgress((int Percent, string Message) progress)
    {
        Console.Write($"\rConversion progress: {progress.Percent}%");
        if (!string.IsNullOrEmpty(progress.Message))
        {
            Console.Write($"\n{progress.Message}");
        }
    }
}

async Task<string> ConvertToMp4(string filePath, bool exportAudio = true, IProgress<(int Percent, string Message)> progress = default, CancellationToken token = default)
{
    using var converter = new G64ToMp4Converter();

    converter.Initialize(
        filePath: filePath,
        exportAudio: exportAudio,
        outputFilePath: Path.ChangeExtension(filePath, ".mp4"));

    return (await converter.ConvertAsync(progress, token)).FirstOrDefault();
}

async Task<string> ConvertToAsf(string filePath, bool exportAudio = true, IProgress<(int Percent, string Message)> progress = default, CancellationToken token = default)
{
    using var converter = new G64ToAsfConverter();

    converter.Initialize(
        filePath: filePath,
        displayDateTime: false,
        writeCameraName: false,
        exportAudio: exportAudio,
        outputFilePath: Path.ChangeExtension(filePath, ".asf"),
        profileId: G64ToAsfConverter.GetAsfProfiles().First().Profile);

    return (await converter.ConvertAsync(progress, token)).FirstOrDefault();
}