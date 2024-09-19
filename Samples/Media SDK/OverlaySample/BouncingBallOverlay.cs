// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Sdk.Media.Overlay;

    public class BouncingBallOverlay
    {
        private readonly Guid m_cameraId;
        private readonly Guid m_layerId = new Guid("69A64ACE-6DDC-4142-AD04-06690D8591B3");
        private Task m_task;
        private CancellationTokenSource m_cancellationTokenSource;

        public BouncingBallOverlay(Guid cameraId)
        {
            m_cameraId = cameraId;
        }

        public void Stop()
        {
            m_cancellationTokenSource?.Cancel();
            m_cancellationTokenSource?.Dispose();
        }

        public void Start()
        {
            if (m_task is { IsCompleted: false })
                return;

            m_cancellationTokenSource?.Dispose();
            m_cancellationTokenSource = new CancellationTokenSource();

            CancellationToken token = m_cancellationTokenSource.Token;

            m_task = Task.Run(async () =>
            {
                var ball = new BouncingBall(50, 50, 50, 50, 25)
                {
                    CanvasHeight = 960,
                    CanvasWidth = 1280
                };

                Overlay overlay = OverlayFactory.Get(m_cameraId, "Bouncing ball");

                if (overlay.DrawingSurfaceWidth == 0 || overlay.DrawingSurfaceHeight == 0)
                {
                    overlay.Initialize(1280, 960);
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
}
