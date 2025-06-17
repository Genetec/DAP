// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading.Tasks;
    using Sdk.Entities;
    using Sdk;
    using Sdk.Media;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                var camera = (Camera)engine.GetEntity(EntityType.Camera, 1);

                using var videoSourceFilter = new VideoSourceFilter();

                videoSourceFilter.FrameDecoded += (sender, args) =>
                {
                    using (args)
                    {
                        Console.WriteLine($"Frame Time: {args.FrameTime}, IsKeyFrame: {args.IsKeyFrame}");
                    }
                };

                videoSourceFilter.Initialize(engine, camera.Guid);
                videoSourceFilter.PlayLive();
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task<byte[]> GetCameraSnapshot(Engine engine, Guid camera)
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
}
