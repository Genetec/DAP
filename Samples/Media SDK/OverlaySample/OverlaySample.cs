// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using Sdk;
using Sdk.Entities;
using Sdk.Media.Overlay;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class OverlaySample : SampleBase
{
    private const int s_canvasWidth = 1280;
    private const int s_canvasHeight = 960;

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

        await Task.WhenAll(
            DrawBouncingBall(camera.Guid, token),
            DrawTimecode(camera.Guid, token),
            DrawRecordingStatus(camera, token));
    }

    private async Task<Camera> FindSupportedCamera(Engine engine, CancellationToken token)
    {
        // Load all cameras into the entity cache first
        await LoadEntities(engine, token, EntityType.Camera);

        // Search through cached entities for a camera that supports overlays and is running
        Camera camera = engine.GetEntities(EntityType.Camera).OfType<Camera>()
            .FirstOrDefault(camera => camera.IsOverlaySupported && camera.RunningState == State.Running);

        return camera;
    }

    private async Task DrawBouncingBall(Guid cameraId, CancellationToken token)
    {
        const string layerId = "69A64ACE-6DDC-4142-AD04-06690D8591B3"; // Replace with a unique layer ID for your overlay. Layer ID must be unique and deterministic

        Overlay overlay = await InitializeOverlay(cameraId, "Bouncing ball");
        Layer layer = overlay.CreateLayer(new Guid(layerId), "Bouncing ball");

        var bouncingBall = new BouncingBall(50, 50, 50, 50, 25) { CanvasHeight = s_canvasHeight, CanvasWidth = s_canvasWidth };

        while (!token.IsCancellationRequested)
        {
            bouncingBall.Draw(layer); // Draw the ball on the layer

            if (!layer.Update())
            {
                Console.WriteLine("Update failed");
            }

            await Task.Delay(10, token); // Update every 10 milliseconds
        }
    }

    private async Task DrawTimecode(Guid cameraId, CancellationToken token)
    {
        const string layerId = "92AEA5CA-E0F5-4122-872A-DB9A9F7437F7"; // Replace with a unique layer ID for your overlay. Layer ID must be unique and deterministic

        Overlay overlay = await InitializeOverlay(cameraId, "Timecode");
        Layer layer = overlay.CreateLayer(new Guid(layerId), "Timecode");

        var timeDisplay = new TimeDisplay();

        while (!token.IsCancellationRequested)
        {
            timeDisplay.Draw(layer);

            if (!layer.Update())
            {
                Console.WriteLine("Update failed");
            }

            await Task.Delay(1000, token); // Update every second
        }
    }

    private async Task DrawRecordingStatus(Camera camera, CancellationToken token)
    {
        const string layerId = "A1B2C3D4-E5F6-7890-1234-567890ABCDEF"; // Replace with a unique layer ID for your overlay. Layer ID must be unique and deterministic

        Overlay overlay = await InitializeOverlay(camera.Guid, "Recording Status");
        Layer layer = overlay.CreateLayer(new Guid(layerId), "Recording Status");

        var recordingStatus = new RecordingStatus(camera);

        while (!token.IsCancellationRequested)
        {
            recordingStatus.Draw(layer);

            if (!layer.Update())
            {
                Console.WriteLine("Update failed");
            }

            await Task.Delay(1000, token); // Update every second
        }
    }

    private async Task<Overlay> InitializeOverlay(Guid camera, string overlayName)
    {
        Overlay overlay = OverlayFactory.Get(camera, overlayName);

        if (overlay.DrawingSurfaceWidth == 0 || overlay.DrawingSurfaceHeight == 0)
        {
            overlay.Initialize(s_canvasHeight, s_canvasWidth);
        }

        await overlay.WaitUntilReadyForUpdate();
        return overlay;
    }
}