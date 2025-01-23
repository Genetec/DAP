// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media.Overlay;
using Genetec.Sdk.Queries;

const string server = "localhost";
const string username = "admin";
const string password = "";

SdkResolver.Initialize();

await RunSample();

Console.Write("Press any key to exit...");
Console.ReadKey(true);

static async Task RunSample()
{
    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"Logon failed: {state}");
        return;
    }

    OverlayFactory.Initialize(engine);

    Camera camera = await FindSupportedCamera();

    if (camera is null)
    {
        Console.WriteLine("No camera found");
        return;
    }

    using var cancellationTokenSource = new CancellationTokenSource();
    Console.WriteLine("Press Ctrl+C to cancel the conversion.");

    Console.CancelKeyPress += (sender, args) =>
    {
        Console.WriteLine("\nCancellation requested.");
        args.Cancel = true; // Prevents the process from terminating
        cancellationTokenSource.Cancel();
    };

    Console.WriteLine($"Drawing overlays on camera: {camera.Name}");

    await Task.WhenAll(DrawBouncingBall(cancellationTokenSource.Token), DrawTimecode(cancellationTokenSource.Token));

    async Task<Camera> FindSupportedCamera()
    {
        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Camera);
        query.Page = 1;
        query.PageSize = 50;

        QueryCompletedEventArgs args;
        do
        {
            var result = args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

            var camera = result.Data.AsEnumerable().Take(query.PageSize)
                .Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid))))
                .OfType<Camera>()
                .FirstOrDefault(camera => camera.IsOverlaySupported && camera.RunningState == State.Running);

            if (camera != null)
            {
                return camera;
            }

            query.Page++;

        } while (args.Error == ReportError.TooManyResults || args.Data.Rows.Count > query.PageSize);

        return null;
    }

    async Task DrawBouncingBall(CancellationToken token = default)
    {
        // Layer IDs should be deterministic per application
        Guid layerId = new("69A64ACE-6DDC-4142-AD04-06690D8591B3");

        var ball = new BouncingBall(50, 50, 50, 50, 25) { CanvasHeight = 960, CanvasWidth = 1280 };

        Overlay overlay = OverlayFactory.Get(camera, "Bouncing ball");

        if (overlay.DrawingSurfaceWidth == 0 || overlay.DrawingSurfaceHeight == 0)
        {
            overlay.Initialize(drawingSurfaceHeight: 960, drawingSurfaceWidth: 1280);
        }

        await overlay.WaitUntilReadyForUpdate();

        Layer layer = overlay.CreateLayer(layerId, "Bouncing ball");

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

    async Task DrawTimecode(CancellationToken token = default)
    {
        // Layer IDs should be deterministic per application
        Guid layerId = new("69A64ACE-6DDC-4142-AD04-06690D8591B3");

        Overlay overlay = OverlayFactory.Get(camera, "Timecode");

        if (overlay.DrawingSurfaceWidth == 0 || overlay.DrawingSurfaceHeight == 0)
        {
            overlay.Initialize(drawingSurfaceHeight: 960, drawingSurfaceWidth: 1280);
        }

        await overlay.WaitUntilReadyForUpdate();

        Layer layer = overlay.CreateLayer(layerId, "Timecode");

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
}
