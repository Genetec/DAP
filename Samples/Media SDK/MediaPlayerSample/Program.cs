// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Genetec.Sdk.Media;

namespace Genetec.Dap.CodeSamples;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sdk;
using Sdk.Entities;
using Sdk.Queries;
using Application = System.Windows.Application;
using MediaPlayer = Sdk.Media.MediaPlayer;

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

        if (state != ConnectionStateCode.Success)
        {
            Console.WriteLine($"Logon failed: {state}");
            return;
        }

        await LoadCameras(engine);

        var camera = engine.GetEntities(EntityType.Camera).OfType<Camera>().FirstOrDefault();
        if (camera is null)
        {
            Console.WriteLine("No camera found");
            return;
        }

        DisplayControls();

        var thread = new Thread(RunWpfApplication);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        void RunWpfApplication()
        {
            var player = new MediaPlayer();

            var window = new Window { Width = 800, Height = 600, Background = Brushes.Black, Content = player };
            window.Loaded += OnLoaded;
            window.KeyDown += OnKeyDown;

            player.PlayerStateChanged += (sender, arg) => UpdateTitle();
            player.PlaySpeedChanged += (sender, arg) => UpdateTitle();
            player.LivePlaybackModeToggled += (sender, arg) => UpdateTitle();

            var application = new Application();
            application.Run(window);
            player.Dispose();

            void OnLoaded(object sender, RoutedEventArgs arg)
            {
                player.Initialize(engine, camera.Guid, StreamingType.Live);
                player.PlayLive();
            }

            void UpdateTitle()
            {
                window.Title = $"MediaPlayer - {camera.Name} - {player.State}" +
                               $"{(player.PlaySpeed != PlaySpeed.Speed1X ? $" ({player.PlaySpeed})" : "")}" +
                               $" ({(player.IsPlayingLiveStream ? "live" : "playback")})";

            }

            void OnKeyDown(object sender, KeyEventArgs e)
            {
                switch (e.Key)
                {
                    case Key.Space:
                        TogglePlayPause();
                        break;
                    case Key.Left:
                        SeekBackward();
                        break;
                    case Key.Right:
                        SeekForward();
                        break;
                    case Key.Up:
                        IncreasePlaySpeed();
                        break;
                    case Key.Down:
                        DecreasePlaySpeed();
                        break;
                    case Key.R:
                        player.Rewind();
                        break;
                    case Key.L:
                        player.PlayLive();
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

            void SeekBackward()
            {
                if (player.LastRenderedFrameTime != DateTime.MinValue)
                {
                    player.Seek(player.LastRenderedFrameTime.AddSeconds(-10));
                }
            }

            void SeekForward()
            {
                if (player.IsPlayingLiveStream)
                    return;

                if (player.LastRenderedFrameTime != DateTime.MinValue)
                {
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
                    player.PlaySpeed = (PlaySpeed)Enum.GetValues(typeof(PlaySpeed)).GetValue(currentIndex - 1);
                }
            }
        }
    }

    private static void UpdateTitle(object sender, EventArgs e)
    {
        throw new NotImplementedException();
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
        Console.WriteLine();
    }
}