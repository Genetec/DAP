using Genetec.Dap.CodeSamples;
// Licensed under the Apache License, Version 2.0

using Genetec.Sdk.Queries;
using Genetec.Sdk;
using System.Threading.Tasks;
using System.Threading;
using System;
using Genetec.Sdk.Entities;
using System.Data;
using System.Linq;

const string server = "localhost";
const string username = "admin";
const string password = "";
const int canvasWidth = 1280;
const int canvasHeight = 960;

SdkResolver.Initialize();
await RunSample();

Console.Write("Press any key to exit...");
Console.ReadKey(true);

static async Task RunSample()
{
    using var engine = new Engine();
    var connectionState = await engine.LogOnAsync(server, username, password);

    if (connectionState != ConnectionStateCode.Success)
    {
        Console.WriteLine($"Logon failed: {connectionState}");
        return;
    }

    OverlayFactory.Initialize(engine);
    var camera = await FindSupportedCamera(engine);

    if (camera is null)
    {
        Console.WriteLine("No running camera supporting overlays was found.");
        return;
    }

    using var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, args) =>
    {
        Console.WriteLine("\nCancellation requested.");
        args.Cancel = true;
        cancellationTokenSource.Cancel();
    };

    Console.WriteLine($"Drawing overlays on camera: {camera.Name}");
    Console.WriteLine("Press Ctrl+C to cancel");

    try
    {
        await Task.WhenAll(DrawBouncingBall(camera, cancellationTokenSource.Token), DrawTimecode(camera, cancellationTokenSource.Token));
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Drawing overlays canceled");
    }

    async Task<Camera> FindSupportedCamera(Engine engine)
    {
        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Camera);
        query.Page = 1; // Start at page 1
        query.PageSize = 50; // Query 50 entities at a time

        QueryCompletedEventArgs queryResult;
        do
        {
            queryResult = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            Camera camera = queryResult.Data.AsEnumerable()
                .Take(query.PageSize) // Only take the first 50 results
                .Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))) // Convert the GUIDs to entities
                .OfType<Camera>() // Filter out non-camera entities
                .FirstOrDefault(camera => camera.IsOverlaySupported && camera.RunningState == State.Running); // Find the first camera that supports overlays and is running

            if (camera != null)
            {
                return camera;
            }

            query.Page++; // Move to the next page

        } while (queryResult.Error == ReportError.TooManyResults || queryResult.Data.Rows.Count > query.PageSize); // Continue querying until we reach the end of the results

        return null;
    }

    async Task DrawBouncingBall(Camera camera, CancellationToken token)
    {
        const string layerId = "69A64ACE-6DDC-4142-AD04-06690D8591B3"; // Layer ID must be unique and deterministic

        var ball = new BouncingBall(50, 50, 50, 50, 25) { CanvasHeight = canvasHeight, CanvasWidth = canvasWidth };
        Overlay overlay = await InitializeOverlay(camera, "Bouncing ball");
        Layer layer = overlay.CreateLayer(new Guid(layerId), "Bouncing ball");

        while (!token.IsCancellationRequested)
        {
            ball.Draw(layer);

            if (!layer.Update())
            {
                Console.WriteLine("Update failed");
            }
            await Task.Delay(10, token);
        }
    }

    async Task DrawTimecode(Camera camera, CancellationToken token)
    {
        const string layerId = "92AEA5CA-E0F5-4122-872A-DB9A9F7437F7"; // Layer ID must be unique and deterministic

        Overlay overlay = await InitializeOverlay(camera, "Timecode");

        Layer layer = overlay.CreateLayer(new Guid(layerId), "Timecode");

        while (!token.IsCancellationRequested)
        {
            layer.DrawText(text: DateTime.Now.ToString("F"), typefaceName: "Arial", emSize: 18, color: "Red", x: 0, y: 0);

            if (!layer.Update())
            {
                Console.WriteLine("Update failed");
            }
            await Task.Delay(1000, token);
        }
    }

    async Task<Overlay> InitializeOverlay(Camera camera, string overlayName)
    {
        var overlay = OverlayFactory.Get(camera, overlayName);

        if (overlay.DrawingSurfaceWidth == 0 || overlay.DrawingSurfaceHeight == 0)
        {
            overlay.Initialize(canvasHeight, canvasWidth);
        }

        await overlay.WaitUntilReadyForUpdate();
        return overlay;
    }
}
