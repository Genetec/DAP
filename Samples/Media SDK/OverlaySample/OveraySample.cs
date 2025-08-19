namespace Genetec.Dap.CodeSamples;

using Sdk;
using Sdk.Entities;
using Sdk.Media.Overlay;
using System;
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

        Camera camera = await FindSupportedCamera(engine, token); // Find a camera that supports overlays and is running
        if (camera is null)
        {
            Console.WriteLine("No running camera supporting overlays was found.");
            return;
        }

        Console.WriteLine($"Drawing overlays on camera: {camera.Name}");

        await Task.WhenAll(DrawBouncingBall(camera.Guid, token), DrawTimecode(camera.Guid, token));
    }

    async Task<Camera> FindSupportedCamera(Engine engine, CancellationToken token)
    {
        // Load all cameras into the entity cache first
        await LoadEntities(engine, token, EntityType.Camera);

        // Search through cached entities for a camera that supports overlays and is running
        Camera camera = engine.GetEntities(EntityType.Camera).OfType<Camera>()
            .FirstOrDefault(camera => camera.IsOverlaySupported && camera.RunningState == State.Running);

        return camera;
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