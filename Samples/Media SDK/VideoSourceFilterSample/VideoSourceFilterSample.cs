// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media;

namespace Genetec.Dap.CodeSamples;

using System.Drawing;

public class VideoSourceFilterSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        const string cameraGuid = "YOUR_CAMERA_GUID_HERE"; // TODO: Replace with your actual camera GUID

        if (engine.GetEntity(new Guid(cameraGuid)) is not Camera camera)
        {
            Console.WriteLine($"Camera {cameraGuid} not found");
            return;
        }

        byte[] snapshot = await GetCameraSnapshot(engine, camera.Guid);

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{camera.Name}_{timestamp}.bmp";
        System.IO.File.WriteAllBytes(fileName, snapshot);

        Console.WriteLine($"Snapshot saved: {fileName}");
    }

    private async Task<byte[]> GetCameraSnapshot(Engine engine, Guid camera)
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
                using RgbDecodedFrame decodedFrame = args.DecodedFrameContent?.GetBitmap();
                if (decodedFrame is null)
                    return;

                Console.WriteLine("Saving snapshot...");

                using Bitmap bitmap = decodedFrame.Bitmap;
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
}