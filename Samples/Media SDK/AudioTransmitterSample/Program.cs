// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

using System;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media;

SdkResolver.Initialize();

await RunSample();

Console.Write("Press any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    const string server = "localhost";  // Specify the IP address or hostname of your Security Center server.
    const string username = "admin";    // Enter the username for Security Center authentication.
    const string password = "";         // Provide the corresponding password for the specified username.
    const string cameraGuid = "";       // Enter the GUID of the camera to access its stream.

    using var engine = new Engine();
    engine.LogonStatusChanged += (_, args) => Console.Write($"\rConnection status: {args.Status}".PadRight(Console.WindowWidth));
    engine.LogonFailed += (_, args) => Console.WriteLine($"\rError: {args.FormattedErrorMessage}".PadRight(Console.WindowWidth));
    engine.LoggedOn += (_, args) => Console.WriteLine($"\rConnected to {args.ServerName}".PadRight(Console.WindowWidth));

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
    {
        return;
    }

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