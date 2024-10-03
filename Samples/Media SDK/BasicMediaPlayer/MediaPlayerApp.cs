// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Sdk.Media;
using MediaPlayer = Sdk.Media.MediaPlayer;

public class MediaPlayerApp(string filePath)
{
    private MediaPlayer m_player;
    private Window m_window;

    public void Run()
    {
        m_player = new MediaPlayer();
        m_player.OpenFile(filePath);

        m_window = new Window
        {
            Width = 800,
            Height = 600,
            Background = Brushes.Black,
            Content = m_player
        };
        m_window.Loaded += (sender, arg) => m_player.PlayFile();
        m_window.KeyDown += OnKeyDown;

        m_player.PlayerStateChanged += (sender, arg) => UpdateTitle();
        m_player.PlaySpeedChanged += (sender, arg) => UpdateTitle();

        var application = new Application();
        application.Run(m_window);
        m_player.Dispose();
    }

    private void UpdateTitle()
    {
        m_window.Title = $"MediaPlayer - {filePath} - {m_player.State}" +
                         $"{(m_player.PlaySpeed != PlaySpeed.Speed1X ? $" ({m_player.PlaySpeed})" : "")}";
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
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
                m_player.Rewind();
                break;
            case Key.I:
                m_player.ShowSpecialOverlay(OverlayType.Statistics);
                break;
            case Key.M:
                m_player.IsAudioEnabled = !m_player.IsAudioEnabled;
                break;
        }
    }

    private void TogglePlayPause()
    {
        switch (m_player.State)
        {
            case PlayerState.Playing:
                m_player.Pause();
                break;
            case PlayerState.Paused:
                m_player.ResumePlaying();
                break;
            default:
                m_player.PlayFile();
                break;
        }
    }

    private void SeekBackward()
    {
        if (m_player.LastRenderedFrameTime != DateTime.MinValue)
        {
            m_player.Seek(m_player.LastRenderedFrameTime.AddSeconds(-10));
        }
    }

    private void SeekForward()
    {
        if (m_player.LastRenderedFrameTime != DateTime.MinValue)
        {
            m_player.Seek(m_player.LastRenderedFrameTime.AddSeconds(10));
        }
    }

    private void IncreasePlaySpeed()
    {
        int currentIndex = Array.IndexOf(Enum.GetValues(typeof(PlaySpeed)), m_player.PlaySpeed);
        if (currentIndex < Enum.GetValues(typeof(PlaySpeed)).Length - 1)
        {
            m_player.PlaySpeed = (PlaySpeed)Enum.GetValues(typeof(PlaySpeed)).GetValue(currentIndex + 1);
        }
    }

    private void DecreasePlaySpeed()
    {
        int currentIndex = Array.IndexOf(Enum.GetValues(typeof(PlaySpeed)), m_player.PlaySpeed);
        if (currentIndex > 0)
        {
            m_player.PlaySpeed = (PlaySpeed)Enum.GetValues(typeof(PlaySpeed)).GetValue(currentIndex - 1);
        }
    }
}