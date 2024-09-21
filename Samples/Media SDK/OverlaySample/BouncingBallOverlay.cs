// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Threading;
using System.Threading.Tasks;
using Sdk.Media.Overlay;

class BouncingBallOverlay(Guid cameraId)
{
    private readonly Guid m_layerId = new("69A64ACE-6DDC-4142-AD04-06690D8591B3");
    private Task m_task;
    private CancellationTokenSource m_cancellationTokenSource;

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

            Overlay overlay = OverlayFactory.Get(cameraId, "Bouncing ball");

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