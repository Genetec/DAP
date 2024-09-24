using System;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media.Reader;

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";
    const string cameraGuid = "your-camera-guid-here"; // Replace with your camera's GUID

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state == ConnectionStateCode.Success)
    {
        if (engine.GetEntity(new Guid(cameraGuid)) is not Camera camera)
        {
            Console.WriteLine($"Camera {cameraGuid} not found");
            return;
        }

        Console.WriteLine("\nReading video stream:");
        await using var videoReader = PlaybackStreamReader.CreateVideoReader(engine, camera.Guid);
        await ReadStream(videoReader);

        Console.WriteLine("\nReading audio stream:");
        await using var audioReader = PlaybackStreamReader.CreateAudioReader(engine, camera.Guid);
        await ReadStream(audioReader);

        // Get the metadata stream. In this example, we are only reading the first metadata stream
        MetadataStreamInfo metadataStreamInfo = camera.MetadataStreams.FirstOrDefault();
        if (metadataStreamInfo is not null)
        {
            Console.WriteLine("\nReading metadata stream:");
            await using var metadataReader = PlaybackStreamReader.CreateReader(engine, camera.Guid, metadataStreamInfo.StreamId);
            await ReadStream(metadataReader);
        }
        else
        {
            Console.WriteLine("\nNo metadata stream available");
        }
    }
    else
    {
        Console.WriteLine($"logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();

    // Method to open and read from a stream
    async Task ReadStream(PlaybackStreamReader reader)
    {
        // Connect to the stream
        await reader.ConnectAsync();

        // Seek to one minute ago
        await reader.SeekAsync(DateTime.UtcNow.AddMinutes(-1));

        while (true)
        {
            // Read data from the stream
            RawDataContent content = await reader.ReadAsync();
            if (content is null)
            {
                // if the content is null, it means we have reached the end of the stream
                Console.WriteLine("  End of stream reached");
                break;
            }

            using (content)
            {
                // Get the MediaType name
                string mediaTypeName = GetMediaTypeName(content.MediaType);

                // Output frame information
                Console.WriteLine($"Frame time {content.FrameTime}, Format: {content.Format}, MediaType: {mediaTypeName}, Data size: {content.Data.Count}");
            }
        }
    }

    string GetMediaTypeName(Guid mediaTypeGuid)
    {
        if (mediaTypeGuid == MediaTypes.Video) return "Video";
        if (mediaTypeGuid == MediaTypes.AudioIn) return "AudioIn";
        if (mediaTypeGuid == MediaTypes.AudioOut) return "AudioOut";
        if (mediaTypeGuid == MediaTypes.Metadata) return "Metadata";
        if (mediaTypeGuid == MediaTypes.Ptz) return "Ptz";
        if (mediaTypeGuid == MediaTypes.AgentPtz) return "AgentPtz";
        if (mediaTypeGuid == MediaTypes.OverlayUpdate) return "OverlayUpdate";
        if (mediaTypeGuid == MediaTypes.OverlayStream) return "OverlayStream";
        if (mediaTypeGuid == MediaTypes.EncryptionKey) return "EncryptionKey";
        if (mediaTypeGuid == MediaTypes.CollectionEvents) return "CollectionEvents";
        if (mediaTypeGuid == MediaTypes.ArchiverEvents) return "ArchiverEvents";
        if (mediaTypeGuid == MediaTypes.OnvifAnalyticsStream) return "OnvifAnalyticsStream";
        if (mediaTypeGuid == MediaTypes.BoschVcaStream) return "BoschVcaStream";
        if (mediaTypeGuid == MediaTypes.FusionStream) return "FusionStream";
        if (mediaTypeGuid == MediaTypes.FusionStreamEvents) return "FusionStreamEvents";
        if (mediaTypeGuid == MediaTypes.OriginalVideo) return "OriginalVideo";
        if (mediaTypeGuid == MediaTypes.Block) return "Block";
        return "Unknown";
    }
}