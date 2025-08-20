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
class AudioTransmitterSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        const string cameraGuid = "YOUR_CAMERA_GUID_HERE"; // TODO : Replace with your actual camera GUID

        if (engine.GetEntity(new Guid(cameraGuid)) is not Camera camera)
        {
            Console.WriteLine($"Camera {cameraGuid} not found");
            return;
        }

        var audioTransmitter = new AudioTransmitter();
        try
        {
            audioTransmitter.Initialize(engine, camera.Guid);

            Console.WriteLine("Starting audio transmission...");
            await audioTransmitter.StartTransmitting();

            var generator = new PcmAudioGenerator();

            // Transmit a 440 Hz sine wave for 30 seconds
            byte[] audioData = generator.GenerateSineWave(440.0, 30.0);

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

            Console.WriteLine($"Transmitted {audioData.Length} bytes of audio data.");

            audioTransmitter.StopTransmitting();
        }
        finally
        {
            audioTransmitter.Dispose();
        }
    }
}