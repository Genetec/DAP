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

public class TimecodeOverlay
{
    private readonly Guid m_cameraId;
    private readonly Guid m_layerId = new Guid("69A64ACE-6DDC-4142-AD04-06690D8591B3");
    private CancellationTokenSource m_cancellationTokenSource;
    private Task m_task; 

    public TimecodeOverlay(Guid cameraId)
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

        m_cancellationTokenSource = new CancellationTokenSource();
        var token = m_cancellationTokenSource.Token;

        m_task = Task.Run(async () =>
        {
            Overlay overlay = OverlayFactory.Get(m_cameraId, "Timecode");

            if (overlay.DrawingSurfaceWidth == 0 || overlay.DrawingSurfaceHeight == 0)
                overlay.Initialize(1280, 960);

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