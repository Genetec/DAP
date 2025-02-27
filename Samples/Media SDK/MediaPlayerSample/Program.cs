// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Genetec.Sdk;
using Genetec.Sdk.Media;
using Genetec.Sdk.Queries;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Camera = Genetec.Sdk.Entities.Camera;
using MediaPlayer = Genetec.Sdk.Media.MediaPlayer;

namespace Genetec.Dap.CodeSamples;

class Program
{
    static Program() => SdkResolver.Initialize();

    static async Task Main()
    {
        const string server = "localhost";  // TODO: Update with your Security Center server IP address or hostname
        const string username = "admin";    // TODO: Update with your Security Center username
        const string password = "";         // TODO: Update with your Security Center password

        using var engine = new Engine();

        ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

        if (state != ConnectionStateCode.Success)
        {
            Console.WriteLine($"Logon failed: {state}");
            return;
        }

        await LoadCameras(engine);

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

            async Task CycleCameras(CancellationToken token)
            {
                Console.WriteLine("Starting camera cycling...");

                while (true)
                {
                    List<Camera> cameras = engine.GetEntities(EntityType.Camera).OfType<Camera>().ToList();

                    for (int i = 0; i < cameras.Count; i++)
                    {
                        // Get current camera
                        var currentItem = cameras[i];

                        // Get next camera index (wrap around to beginning if at the end)
                        var nextIndex = (i + 1) % cameras.Count;
                        var nextItem = cameras[nextIndex];

                        if (currentCamera is null)
                        {
                            currentCamera = currentItem;
                            // Initialize the player with the first camera in the sequence
                            player.Initialize(engine, currentCamera.Guid, StreamingType.Live);
                            player.PlayLive();
                        }
                        else
                        {
                            currentCamera = currentItem;
                            // Switch to the next camera in the sequence
                            player.SwitchToNextVideoInSequence();
                        }

                        // Prepare the next camera in the sequence
                        player.PrepareNextVideoInSequence(engine, nextItem.Guid, Guid.Empty, StreamingType.Live, null);

                        await Task.Delay(5000, token); // Wait 5 seconds before switching to the next camera
                    }
                }
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
        }
    }

    static Task LoadCameras(Engine engine)
    {
        Console.WriteLine("Loading cameras...");

        var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
        query.EntityTypeFilter.Add(EntityType.Camera);

        return Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
    }

    static void DisplayControls()
    {
        Console.WriteLine("Keyboard Controls:");
        Console.WriteLine();
        Console.WriteLine("Space: Play/Pause");
        Console.WriteLine("Left Arrow: Seek backward by 10 seconds");
        Console.WriteLine("Right Arrow: Seek forward by 10 seconds");
        Console.WriteLine("Up Arrow: Increase playback speed");
        Console.WriteLine("Down Arrow: Decrease playback speed");
        Console.WriteLine("R: Rewind");
        Console.WriteLine("I: Toggle statistics overlay");
        Console.WriteLine("M: Toggle audio mute");
        Console.WriteLine("C: Toggle camera cycling");
        Console.WriteLine();
    }
}