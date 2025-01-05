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

    using var cancellationTokenSource = new CancellationTokenSource();

    Console.CancelKeyPress += (_, e) =>
    {
        Console.WriteLine("\nCancelling...");
        e.Cancel = true;
        cancellationTokenSource.Cancel();
    };

    try
    {
        await ReadVideoStream(cancellationTokenSource.Token);
        await ReadAudioStream(cancellationTokenSource.Token);
        await ReadMetadataStream(cancellationTokenSource.Token);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Read cancelled");
    }

    async Task ReadVideoStream(CancellationToken cancellationToken)
    {
        Console.WriteLine($"\nReading video stream from camera: {camera.Name}");
        await using var reader = PlaybackStreamReader.CreateVideoReader(engine, camera.Guid);
        await ReadStream(reader, cancellationToken);
    }

    async Task ReadAudioStream(CancellationToken cancellationToken)
    {
        Console.WriteLine($"\nReading audio stream from camera: {camera.Name}");
        await using var reader = PlaybackStreamReader.CreateAudioReader(engine, camera.Guid);
        await ReadStream(reader, cancellationToken);
    }

    async Task ReadMetadataStream(CancellationToken cancellationToken)
    {
        MetadataStreamInfo metadataStreamInfo = camera.MetadataStreams.FirstOrDefault();
        Console.WriteLine($"\nReading metadata stream from camera: {camera.Name}");
        if (metadataStreamInfo is not null)
        {
            await using var reader = PlaybackStreamReader.CreateReader(engine, camera.Guid, metadataStreamInfo.StreamId);
            await ReadStream(reader, cancellationToken);
        }
        else
        {
            Console.WriteLine("No metadata stream available");
        }
    }

    async Task ReadStream(PlaybackStreamReader reader, CancellationToken cancellationToken)
    {
        await reader.ConnectAsync(cancellationToken);
        await reader.SeekAsync(DateTime.UtcNow.AddMinutes(-5), cancellationToken);

        var (frames, bytes, start) = (0, 0L, DateTime.Now);

        while (true)
        {
            RawDataContent rawDataContent = await reader.ReadAsync(cancellationToken);
            if (rawDataContent is null)
            {
                Console.Write("\rEnd of stream reached".PadRight(Console.WindowWidth));
                break;
            }

            using (rawDataContent)
            {
                frames++;
                bytes += rawDataContent.Data.Count;
                Console.Write($"\r{rawDataContent.FrameTime:HH:mm:ss.fff} | {GetMediaTypeName(rawDataContent.MediaType),-12} | {rawDataContent.Format,8} | Size: {rawDataContent.Data.Count,6:N0} B | Avg: {bytes / frames,6:N0} B | FPS: {frames / (DateTime.Now - start).TotalSeconds,5:N1}".PadRight(Console.WindowWidth));
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