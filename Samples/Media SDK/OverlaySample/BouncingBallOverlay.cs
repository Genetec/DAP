// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Threading;
using System.Threading.Tasks;
using Sdk.Media.Overlay;

public class BouncingBallOverlay
{
    private readonly Guid m_cameraId;
    private readonly Guid m_layerId = new("69A64ACE-6DDC-4142-AD04-06690D8591B3");
    private CancellationTokenSource m_cancellationTokenSource;
    private Task m_task;

    public BouncingBallOverlay(Guid cameraId) => m_cameraId = cameraId;

    public void Stop()
    {
        m_cancellationTokenSource?.Cancel();
        m_cancellationTokenSource?.Dispose();
    }

    public void Start()
    {
        if (m_task is { IsCompleted: false })
        {
            return;
        }

        m_cancellationTokenSource?.Dispose();
        m_cancellationTokenSource = new CancellationTokenSource();

        CancellationToken token = m_cancellationTokenSource.Token;

        m_task = Task.Run(async () =>
        {
            var ball = new BouncingBall(50, 50, 50, 50, 25) { CanvasHeight = 960, CanvasWidth = 1280 };

            Overlay overlay = OverlayFactory.Get(m_cameraId, "Bouncing ball");

            if (overlay.DrawingSurfaceWidth == 0 || overlay.DrawingSurfaceHeight == 0)
            {
                overlay.Initialize(960, 1280);
            }

            await overlay.WaitUntilReadyForUpdate();

            Layer layer = overlay.CreateLayer(m_layerId, "Bouncing ball");

            while (!token.IsCancellationRequested)
            {
                ball.Draw(layer);

                if (!layer.Update())
                {
                    Console.WriteLine("Update failed");
                }

                await Task.Delay(10, token);
            }
        }, token);
    }
}