// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media;

namespace Genetec.Dap.CodeSamples;

public class AudioRecorderSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        const string cameraGuid = "YOUR_CAMERA_GUID_HERE"; // TODO : Replace with your actual camera GUID

        if (engine.GetEntity(new Guid(cameraGuid)) is not Camera camera)
        {
            Console.WriteLine($"Camera {cameraGuid} not found");
            return;
        }

        AudioRecorder audioRecorder = new();

        try
        {
            Console.WriteLine("Initializing audio recorder...");
            audioRecorder.Initialize(engine, camera.Guid);

            Console.WriteLine("Starting audio recording...");
            audioRecorder.StartRecording();

            try
            {
                await Task.Delay(Timeout.Infinite, token); // Wait indefinitely until cancellation is requested
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Stopping audio recording...");
                audioRecorder.StopRecording();
                throw;
            }
        }
        finally
        {
            audioRecorder.Dispose();
        }
    }
}