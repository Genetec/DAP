// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media;

namespace Genetec.Dap.CodeSamples;

/// <summary>
/// // This sample demonstrates how to transmit audio to a Security Center camera
/// // using the AudioTransmitter class. It generates and transmits a simple sine wave tone.
/// </summary>
public class AudioTransmitterSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        const string cameraGuid = "YOUR_CAMERA_GUID_HERE"; // TODO : Replace with your actual camera GUID

        if (engine.GetEntity(new Guid(cameraGuid)) is not Camera camera)
        {
            Console.WriteLine($"Camera {cameraGuid} not found");
            return;
        }

        AudioTransmitter audioTransmitter = new();
        bool startAttempted = false;
        try
        {
            audioTransmitter.Initialize(engine, camera.Guid);

            Console.WriteLine("Starting audio transmission...");
            startAttempted = true;
            await audioTransmitter.StartTransmitting();

            var generator = new PcmAudioGenerator();

            // Transmit a 440 Hz sine wave for 30 seconds
            const int durationSeconds = 30;
            byte[] audioData = generator.GenerateSineWave(440.0, durationSeconds);

            // Determine optimal payload size for the AudioTransmitter
            int payloadSize = audioTransmitter.IdealPayloadSize;

            int offset = 0;
            while (offset < audioData.Length)
            {
                int size = Math.Min(payloadSize, audioData.Length - offset);

                // Queue audio data for transmission
                audioTransmitter.QueueBuffer(audioData, offset, size);
                offset += size;
            }

            Console.WriteLine($"Queued {audioData.Length} bytes of audio data.");
            await Task.Delay(TimeSpan.FromSeconds(durationSeconds), token);
            Console.WriteLine("Audio transmission complete.");
        }
        finally
        {
            try
            {
                if (startAttempted)
                {
                    audioTransmitter.StopTransmitting();
                    // Stop is asynchronous. Allow the session to close before disposal.
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
            finally
            {
                audioTransmitter.Dispose();
            }
        }
    }
}
