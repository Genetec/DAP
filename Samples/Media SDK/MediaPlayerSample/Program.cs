// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Media;
using Genetec.Sdk.Queries;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Camera = Genetec.Sdk.Entities.Camera;
using MediaPlayer = Genetec.Sdk.Media.MediaPlayer;

const string server = "localhost";  // TODO: Update with your Security Center server IP address or hostname
const string username = "admin";    // TODO: Update with your Security Center username
const string password = "";         // TODO: Update with your Security Center password

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"Logon failed: {state}");
        return;
    }

    await LoadCameras();

    DisplayControls();

    var thread = new Thread(RunWpfApplication);
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    void RunWpfApplication()
    {
        MediaPlayer player = new();
        CancellationTokenSource cts = new();
        Camera currentCamera = null;

        Window window = new() { Width = 800, Height = 600, Background = Brushes.Black, Content = player };
        window.Loaded += OnLoaded;
        window.KeyDown += OnKeyDown;

        player.PlayerStateChanged += (sender, arg) => UpdateTitle();
        player.PlaySpeedChanged += (sender, arg) => UpdateTitle();
        player.LivePlaybackModeToggled += (sender, arg) => UpdateTitle();

        Application application = new();
        application.Run(window);
        player.Dispose();

        void OnLoaded(object sender, RoutedEventArgs arg) => _ = CycleCameras(cts.Token);

        void UpdateTitle()
        {
            window.Title = $"MediaPlayer - {currentCamera?.Name} - {player.State}{(player.PlaySpeed != PlaySpeed.Speed1X ? $" ({player.PlaySpeed})" : "")} ({(player.IsPlayingLiveStream ? "live" : "playback")})";
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.O:
                    StopCameraCycling();
                    OpenFile();
                    break;
                case Key.Space:
                    StopCameraCycling();
                    TogglePlayPause();
                    break;
                case Key.Left:
                    StopCameraCycling();
                    SeekBackward();
                    break;
                case Key.Right:
                    StopCameraCycling();
                    SeekForward();
                    break;
                case Key.Up:
                    StopCameraCycling();
                    IncreasePlaySpeed();
                    break;
                case Key.Down:
                    StopCameraCycling();
                    DecreasePlaySpeed();
                    break;
                case Key.R:
                    StopCameraCycling();
                    player.Rewind();
                    break;
                case Key.L:
                    StopCameraCycling();
                    Console.WriteLine("Playing live stream...");
                    player.PlayLive();
                    break;
                case Key.C:
                    ToggleCameraCycling();
                    break;
                case Key.I:
                    player.ShowSpecialOverlay(OverlayType.Statistics);
                    break;
                case Key.M:
                    player.IsAudioEnabled = !player.IsAudioEnabled;
                    break;
                case Key.N:
                    StopCameraCycling();
                    SwitchToNextCamera();
                    break;
            }
        }

        void TogglePlayPause()
        {
            switch (player.State)
            {
                case PlayerState.Paused:
                    player.ResumePlaying();
                    break;
                default:
                    player.Pause();
                    break;
            }
        }

        void ToggleCameraCycling()
        {
            if (cts is null || cts.IsCancellationRequested)
            {
                cts = new CancellationTokenSource();
                _ = CycleCameras(cts.Token);
            }
            else
            {
                StopCameraCycling();
            }
        }

        void StopCameraCycling()
        {
            if (cts is null || cts.IsCancellationRequested)
                return;

            Console.WriteLine("Stopping camera cycling...");
            cts.Cancel();
        }

        void SeekBackward()
        {
            if (player.LastRenderedFrameTime != DateTime.MinValue)
            {
                Console.WriteLine("Seeking backward by 10 seconds...");
                player.Seek(player.LastRenderedFrameTime.AddSeconds(-10));
            }
        }

        void SeekForward()
        {
            if (player.IsPlayingLiveStream)
                return;

            if (player.LastRenderedFrameTime != DateTime.MinValue)
            {
                Console.WriteLine("Seeking forward by 10 seconds...");
                player.Seek(player.LastRenderedFrameTime.AddSeconds(10));
            }
        }

        void IncreasePlaySpeed()
        {
            if (player.IsPlayingLiveStream)
                return;

            int currentIndex = Array.IndexOf(Enum.GetValues(typeof(PlaySpeed)), player.PlaySpeed);
            if (currentIndex < Enum.GetValues(typeof(PlaySpeed)).Length - 1)
            {
                Console.WriteLine("Increasing playback speed...");
                player.PlaySpeed = (PlaySpeed)Enum.GetValues(typeof(PlaySpeed)).GetValue(currentIndex + 1);
            }
        }

        void DecreasePlaySpeed()
        {
            if (player.IsPlayingLiveStream)
                return;

            int currentIndex = Array.IndexOf(Enum.GetValues(typeof(PlaySpeed)), player.PlaySpeed);
            if (currentIndex > 0)
            {
                Console.WriteLine("Decreasing playback speed...");
                player.PlaySpeed = (PlaySpeed)Enum.GetValues(typeof(PlaySpeed)).GetValue(currentIndex - 1);
            }
        }

        void OpenFile()
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Video files (*.mp4;*..g64;*..g64x)|*.mp4;*.g64;*..g64x|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog().GetValueOrDefault())
            {
                player.OpenFile(openFileDialog.FileName);
                player.PlayFile();
            }
        }

        void DoCameraSwitch()
        {
            // Retrieve an ordered list of cameras.
            List<Camera> cameras = engine.GetEntities(EntityType.Camera)
                .OfType<Camera>()
                .OrderBy(camera => camera.Name)
                .ToList();

            if (cameras.Count == 0)
            {
                Console.WriteLine("No cameras available.");
                return;
            }

            // If there's only one camera, ensure that this camera is playing.
            if (cameras.Count == 1)
            {
                if (currentCamera == null || currentCamera.Guid != cameras[0].Guid)
                {
                    currentCamera = cameras[0];
                    player.Initialize(engine, currentCamera.Guid, StreamingType.Live);
                    player.PlayLive();
                }
                return;
            }

            // More than one camera exists.
            int currentIndex = currentCamera != null ? cameras.IndexOf(currentCamera) : -1;
            if (currentIndex == -1)
            {
                // No valid current camera found: initialize with the first camera.
                currentCamera = cameras[0];
                player.Initialize(engine, currentCamera.Guid, StreamingType.Live);
                player.PlayLive();

                // Prepare the next camera (camera at index 1).
                int nextIndex = 1;
                player.PrepareNextVideoInSequence(engine, cameras[nextIndex].Guid, Guid.Empty, StreamingType.Live, null);
            }
            else
            {
                // Determine the next camera and the one after that.
                int nextIndex = (currentIndex + 1) % cameras.Count;
                Camera nextCamera = cameras[nextIndex];
                int nextNextIndex = (nextIndex + 1) % cameras.Count;

                // Switch to the next camera.
                player.SwitchToNextVideoInSequence();

                // Prepare the camera after the next camera.
                player.PrepareNextVideoInSequence(engine, cameras[nextNextIndex].Guid, Guid.Empty, StreamingType.Live, null);

                // Update the current camera reference.
                currentCamera = nextCamera;
            }
        }

        async Task CycleCameras(CancellationToken token)
        {
            Console.WriteLine("Starting camera cycling...");

            while (true)
            {
                DoCameraSwitch();
                await Task.Delay(5000, token); // Wait 5 seconds before switching
            }
        }

        void SwitchToNextCamera()
        {
            // Retrieve an ordered list of cameras.
            List<Camera> cameras = engine.GetEntities(EntityType.Camera)
                .OfType<Camera>()
                .OrderBy(camera => camera.Name)
                .ToList();

            if (cameras.Count == 0)
            {
                return;
            }

            if (cameras.Count == 1)
            {
                if (currentCamera == null || currentCamera.Guid != cameras[0].Guid)
                {
                    currentCamera = cameras[0];
                    player.Initialize(engine, currentCamera.Guid, StreamingType.Live);
                    player.PlayLive();
                }

                return;
            }

            int currentIndex = currentCamera != null ? cameras.IndexOf(currentCamera) : -1;
            if (currentIndex == -1)
            {
                // No valid current camera found: initialize with the first camera.
                currentCamera = cameras[0];
                player.Initialize(engine, currentCamera.Guid, StreamingType.Live);
                player.PlayLive();

                // Prepare the next camera (camera at index 1).
                int nextIndex = 1;
                player.PrepareNextVideoInSequence(engine, cameras[nextIndex].Guid, Guid.Empty, StreamingType.Live, null);
            }
            else
            {
                // Determine the next camera and the one after that.
                int nextIndex = (currentIndex + 1) % cameras.Count;
                Camera nextCamera = cameras[nextIndex];
                nextIndex = (nextIndex + 1) % cameras.Count;

                // Switch to the next camera.
                player.SwitchToNextVideoInSequence();

                // Prepare the camera after the next camera.
                player.PrepareNextVideoInSequence(engine, cameras[nextIndex].Guid, Guid.Empty, StreamingType.Live, null);

                currentCamera = nextCamera;
            }
        }
    }

    Task LoadCameras()
    {
        Console.WriteLine("Loading cameras...");

        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Camera);

        return Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }
}

void DisplayControls()
{
    Console.WriteLine("Keyboard Controls:");
    Console.WriteLine();

    Console.WriteLine("Playback Controls:");
    Console.WriteLine("  Space: Play/Pause");
    Console.WriteLine("  R: Rewind/Reverse");
    Console.WriteLine("  M: Toggle audio mute");
    Console.WriteLine("  O: Open video file");
    Console.WriteLine();

    Console.WriteLine("Navigation Controls:");
    Console.WriteLine("  Left Arrow: Seek backward by 10 seconds");
    Console.WriteLine("  Right Arrow: Seek forward by 10 seconds");
    Console.WriteLine("  Up Arrow: Increase playback speed");
    Console.WriteLine("  Down Arrow: Decrease playback speed");
    Console.WriteLine();

    Console.WriteLine("Camera Controls:");
    Console.WriteLine("  L: Switch to live mode");
    Console.WriteLine("  C: Toggle camera cycling");
    Console.WriteLine("  N: Switch to next camera");
    Console.WriteLine("  I: Toggle statistics overlay");
    Console.WriteLine();
}