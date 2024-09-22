// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Genetec.Sdk.Media;

namespace Genetec.Dap.CodeSamples;

using MediaPlayer = Sdk.Media.MediaPlayer;

class Program
{
    static Program() => SdkResolver.Initialize();

    [STAThread]
    static void Main()
    {
        DisplayControls();

        string filePath = ReadFilePath();

        var player = new MediaPlayer();
        player.OpenFile(filePath);

        var window = new Window
        {
            Width = 800,
            Height = 600,
            Background = Brushes.Black,
            Content = player
        };
        window.Loaded += (sender, arg) => player.PlayFile();
        window.KeyDown += OnKeyDown;

        player.PlayerStateChanged += (sender, arg) => UpdateTitle();
        player.PlaySpeedChanged += (sender, arg) => UpdateTitle();

        var application = new Application();
        application.Run(window);
        player.Dispose();

        void UpdateTitle()
        {
            window.Title = $"MediaPlayer - {filePath} - {player.State}" +
                           $"{(player.PlaySpeed != PlaySpeed.Speed1X ? $" ({player.PlaySpeed})" : "")}";
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
                case PlayerState.Playing:
                    player.Pause();
                    break;
                case PlayerState.Paused:
                    player.ResumePlaying();
                    break;
                default:
                    player.PlayFile();
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
            if (player.LastRenderedFrameTime != DateTime.MinValue)
            {
                player.Seek(player.LastRenderedFrameTime.AddSeconds(10));
            }
        }

        void IncreasePlaySpeed()
        {
            int currentIndex = Array.IndexOf(Enum.GetValues(typeof(PlaySpeed)), player.PlaySpeed);
            if (currentIndex < Enum.GetValues(typeof(PlaySpeed)).Length - 1)
            {
                player.PlaySpeed = (PlaySpeed)Enum.GetValues(typeof(PlaySpeed)).GetValue(currentIndex + 1);
            }
        }

        void DecreasePlaySpeed()
        {
            int currentIndex = Array.IndexOf(Enum.GetValues(typeof(PlaySpeed)), player.PlaySpeed);
            if (currentIndex > 0)
            {
                player.PlaySpeed = (PlaySpeed)Enum.GetValues(typeof(PlaySpeed)).GetValue(currentIndex - 1);
            }
        }
    }

    static string ReadFilePath()
    {
        while (true)
        {
            Console.Write("Please enter the path to a .g64, .g64x, or .mp4 file: ");
            string filePath = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("File path cannot be empty. Please try again.");
                continue;
            }
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"The file {filePath} does not exist. Please try again.");
                continue;
            }
            return filePath;
        }
    }

    static void DisplayControls()
    {
        Console.WriteLine("Basic MediaPlayer sample");
        Console.WriteLine("Space: Play/Pause");
        Console.WriteLine("Left Arrow: Seek backward by 10 seconds");
        Console.WriteLine("Right Arrow: Seek forward by 10 seconds");
        Console.WriteLine("Up Arrow: Increase playback speed");
        Console.WriteLine("Down Arrow: Decrease playback speed");
        Console.WriteLine("R: Rewind to the beginning");
        Console.WriteLine("I: Toggle statistics overlay");
        Console.WriteLine("M: Toggle audio mute");
    }
}