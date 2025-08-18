namespace Genetec.Dap.CodeSamples;

using Sdk;
using Sdk.Entities;
using Sdk.Media.Overlay;
using Sdk.Queries;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class OverlaySample : SampleBase
{
    private const int canvasWidth = 1280;
    private const int canvasHeight = 960;

    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        OverlayFactory.Initialize(engine);

        Camera camera = await FindSupportedCamera(engine); // Find a camera that supports overlays and is running
        if (camera is null)
        {
            Console.WriteLine("No running camera supporting overlays was found.");
            return;
        }

        Console.WriteLine($"Drawing overlays on camera: {camera.Name}");

        await Task.WhenAll(DrawBouncingBall(camera.Guid, token), DrawTimecode(camera.Guid, token));
    }

    async Task<Camera> FindSupportedCamera(Engine engine)
    {
        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Camera);
        query.Page = 1; // Start at page 1
        query.PageSize = 50; // Query 50 entities at a time

        QueryCompletedEventArgs args;
        do
        {
            args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            Camera camera = args.Data.AsEnumerable().Take(query.PageSize) // Only take the first 50 results
                .Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))) // Convert the GUIDs to entities
                .OfType<Camera>() // Filter out non-camera entities
                .FirstOrDefault(camera => camera.IsOverlaySupported && camera.RunningState == State.Running); // Find the first camera that supports overlays and is running

            if (camera != null)
            {
                return camera;
            }

            query.Page++; // Move to the next page
        } while (args.Data.Rows.Count > query.PageSize); // Continue querying until we reach the end of the results

        return null;
    }

    async Task DrawBouncingBall(Guid cameraId, CancellationToken token)
    {
        const string layerId = "69A64ACE-6DDC-4142-AD04-06690D8591B3"; // Please replace with a unique layer ID for your overlay. Layer ID must be unique and deterministic

        var ball = new BouncingBall(50, 50, 50, 50, 25) { CanvasHeight = canvasHeight, CanvasWidth = canvasWidth };
        Overlay overlay = await InitializeOverlay(cameraId, "Bouncing ball");
        Layer layer = overlay.CreateLayer(new Guid(layerId), "Bouncing ball");

        while (!token.IsCancellationRequested)
        {
            ball.Draw(layer); // Draw the ball on the layer

            if (!layer.Update())
            {
                Console.WriteLine("Update failed");
            }

            await Task.Delay(10, token); // Update every 10 milliseconds
        }
    }

    async Task DrawTimecode(Guid cameraId, CancellationToken token)
    {
        const string layerId = "92AEA5CA-E0F5-4122-872A-DB9A9F7437F7"; // Please replace with a unique layer ID for your overlay. Layer ID must be unique and deterministic

        Overlay overlay = await InitializeOverlay(cameraId, "Timecode");

        Layer layer = overlay.CreateLayer(new Guid(layerId), "Timecode");

        while (!token.IsCancellationRequested)
        {
            layer.DrawText(DateTime.Now.ToString("F"), "Arial", 18, "Red", 0, 0);

            if (!layer.Update())
            {
                Console.WriteLine("Update failed");
            }

            await Task.Delay(1000, token); // Update every second
        }
    }

    async Task<Overlay> InitializeOverlay(Guid camera, string overlayName)
    {
        Overlay overlay = OverlayFactory.Get(camera, overlayName);

        if (overlay.DrawingSurfaceWidth == 0 || overlay.DrawingSurfaceHeight == 0)
        {
            overlay.Initialize(canvasHeight, canvasWidth);
        }

        await overlay.WaitUntilReadyForUpdate();
        return overlay;
    }
}