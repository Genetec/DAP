// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Threading;
using System.Threading.Tasks;
using Sdk.Media.Overlay;

public class TimecodeOverlay
{
    private readonly Guid m_cameraId;
    private readonly Guid m_layerId = new Guid("69A64ACE-6DDC-4142-AD04-06690D8591B3");
    private CancellationTokenSource m_cancellationTokenSource;
    private Task m_task;

    public TimecodeOverlay(Guid cameraId) => m_cameraId = cameraId;

    public void Stop()
    {
        m_cancellationTokenSource?.Cancel();
        m_cancellationTokenSource?.Dispose();
    }

    public void Start()
    {
        if (m_task is { IsCompleted: false })
            return;

        m_cancellationTokenSource = new CancellationTokenSource();
        var token = m_cancellationTokenSource.Token;

        m_task = Task.Run(async () =>
        {
            Overlay overlay = OverlayFactory.Get(m_cameraId, "Timecode");

            if (overlay.DrawingSurfaceWidth == 0 || overlay.DrawingSurfaceHeight == 0)
                overlay.Initialize(drawingSurfaceHeight: 960, drawingSurfaceWidth: 1280);

            await overlay.WaitUntilReadyForUpdate();

            Layer layer = overlay.CreateLayer(m_layerId, "Timecode");

            while (!token.IsCancellationRequested)
            {
                layer.DrawText(DateTime.Now.ToString("F"), "Arial", 18, "Red", 0, 0);

                if (!layer.Update())
                    Console.WriteLine("Update failed");

                await Task.Delay(1000, token);
            }
        }, token);
    }
}