// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media;

SdkResolver.Initialize();

await RunSample();

Console.WriteLine("Press any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";
    const string cameraGuid = "your-camera-guid-here"; // Replace with your camera's GUID

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"Logon failed: {state}");
        return;
    }

    if (engine.GetEntity(new Guid(cameraGuid)) is not Camera camera)
    {
        Console.WriteLine($"Camera {cameraGuid} not found");
        return;
    }

    byte[] snapshot = await GetCameraSnapshot(engine, camera.Guid);

    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    string fileName = $"{camera.Name}_{timestamp}.bmp";
    System.IO.File.WriteAllBytes(fileName, snapshot);
}

async Task<byte[]> GetCameraSnapshot(Engine engine, Guid camera)
{
    var completion = new TaskCompletionSource<byte[]>();

    using (var videoSourceFilter = new VideoSourceFilter())
    {
        videoSourceFilter.FrameDecoded += OnFrameDecoded;
        videoSourceFilter.PlayerStateChanged += OnPlayerStateChanged;
        try
        {
            videoSourceFilter.Initialize(engine, camera);
            videoSourceFilter.PlayLive();
            return await completion.Task;
        }
        finally
        {
            videoSourceFilter.FrameDecoded -= OnFrameDecoded;
            videoSourceFilter.Stop();
        }
    }

    void OnFrameDecoded(object sender, FrameDecodedEventArgs args)
    {

        using (args)
        {
            using var decodedFrame = args.DecodedFrameContent?.GetBitmap();
            if (decodedFrame is null)
                return;

            Console.WriteLine("Saving snapshot...");

            using var bitmap = decodedFrame.Bitmap;
            using var memoryStream = new MemoryStream();

            try
            {
                bitmap.Save(memoryStream, ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }

            memoryStream.Position = 0;
            completion.TrySetResult(memoryStream.ToArray());
        }
    }

    void OnPlayerStateChanged(object sender, PlayerStateChangedEventArgs e)
    {
        Console.WriteLine($"State changed: {e.State}");

        switch (e.State)
        {
            case PlayerState.NoVideoSequenceAvailable:
            case PlayerState.InsufficientSecurityInformation:
            case PlayerState.Error:
                {
                    var videoSourceFilter = (VideoSourceFilter)sender;
                    completion.TrySetException(new Exception(videoSourceFilter.ErrorDetails?.Details));
                    break;
                }
        }
    }
}