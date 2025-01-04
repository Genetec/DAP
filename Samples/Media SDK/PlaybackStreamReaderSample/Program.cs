using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media.Reader;

SdkResolver.Initialize();

await RunSample();

Console.WriteLine("Press any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";
    const string cameraGuid = "00000001-0000-babe-0000-26551ec56587"; // Replace with your camera's GUID

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"logon failed: {state}");
        return;
    }

    if (engine.GetEntity(new Guid(cameraGuid)) is not Camera camera)
    {
        Console.WriteLine($"Camera {cameraGuid} not found");
        return;
    }

    using var cancellationTokenSource = new CancellationTokenSource();

    Console.CancelKeyPress += (sender, e) =>
    {
        Console.WriteLine("Cancelling...");
        e.Cancel = true;
        cancellationTokenSource.Cancel();
    };

    try
    {
        Console.WriteLine($"Reading video stream from camera: {camera.Name}");
        await using var videoReader = PlaybackStreamReader.CreateVideoReader(engine, camera.Guid);
        await ReadStream(videoReader, cancellationTokenSource.Token);

        Console.WriteLine("\nReading audio stream...");
        await using var audioReader = PlaybackStreamReader.CreateAudioReader(engine, camera.Guid);
        await ReadStream(audioReader, cancellationTokenSource.Token);

        Console.WriteLine("\nReading metadata stream...");
        // Get the metadata stream. In this example, we are only reading the first metadata stream
        MetadataStreamInfo metadataStreamInfo = camera.MetadataStreams.FirstOrDefault();
        if (metadataStreamInfo is not null)
        {
            Console.WriteLine("\nReading metadata stream:");
            await using var metadataReader = PlaybackStreamReader.CreateReader(engine, camera.Guid, metadataStreamInfo.StreamId);
            await ReadStream(metadataReader, cancellationTokenSource.Token);
        }
        else
        {
            Console.WriteLine("\nNo metadata stream available");
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Read cancelled");
    }

    // Method to open and read from a stream
    async Task ReadStream(PlaybackStreamReader reader, CancellationToken token = default)
    {
        // Connect to the stream
        await reader.ConnectAsync(token);

        // Seek to one minute ago
        await reader.SeekAsync(DateTime.UtcNow.AddMinutes(-5), token);

        while (true)
        {
            // Read data from the stream
            RawDataContent content = await reader.ReadAsync(token);
            if (content is null)
            {
                // if the content is null, it means we have reached the end of the stream
                Console.WriteLine("End of stream reached");
                break;
            }

            using (content)
            {
                // Get the MediaType name
                string mediaTypeName = GetMediaTypeName(content.MediaType);

                // Output frame information
                Console.Write($"\rFrame time: {content.FrameTime:HH:mm:ss.fff} | Format: {content.Format,8} | Type: {mediaTypeName,-12} | Size: {content.Data.Count,8:N0} bytes");
            }
        }
    }

    string GetMediaTypeName(Guid mediaTypeGuid)
    {
        if (mediaTypeGuid == MediaTypes.Video) return "Video";
        if (mediaTypeGuid == MediaTypes.AudioIn) return "Audio In";
        if (mediaTypeGuid == MediaTypes.AudioOut) return "Audio Out";
        if (mediaTypeGuid == MediaTypes.Metadata) return "Metadata";
        if (mediaTypeGuid == MediaTypes.Ptz) return "Ptz";
        if (mediaTypeGuid == MediaTypes.AgentPtz) return "Agent Ptz";
        if (mediaTypeGuid == MediaTypes.OverlayUpdate) return "Overlay Update";
        if (mediaTypeGuid == MediaTypes.OverlayStream) return "Overlay Stream";
        if (mediaTypeGuid == MediaTypes.EncryptionKey) return "Encryption Key";
        if (mediaTypeGuid == MediaTypes.CollectionEvents) return "Collection Events";
        if (mediaTypeGuid == MediaTypes.ArchiverEvents) return "Archiver Events";
        if (mediaTypeGuid == MediaTypes.OnvifAnalyticsStream) return "Onvif Analytics Stream";
        if (mediaTypeGuid == MediaTypes.BoschVcaStream) return "Bosch Vca Stream";
        if (mediaTypeGuid == MediaTypes.FusionStream) return "Fusion Stream";
        if (mediaTypeGuid == MediaTypes.FusionStreamEvents) return "Fusion Stream Events";
        if (mediaTypeGuid == MediaTypes.OriginalVideo) return "Original Video";
        if (mediaTypeGuid == MediaTypes.Block) return "Block";
        return "Unknown";
    }
}