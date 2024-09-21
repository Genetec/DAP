// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Genetec.Dap.CodeSamples;

class Program
{
    static Program() => SdkResolver.Initialize();

    [STAThread]
    static void Main()
    {
        string filePath = ReadFilePath();
        var application = new Application();
        application.Run(new MediaPlayerWindow(filePath));
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
}

class MediaPlayerWindow : Window
{
    public MediaPlayerWindow(string filePath)
    {
        Title = $"MediaPlayer - {filePath}";
        Width = 800;
        Height = 600;
        Background = Brushes.Black;

        var player = new Genetec.Sdk.Media.MediaPlayer();
        Content = player;

        Loaded += (_, _) =>
        {
            player.OpenFile(filePath);
            player.PlayFile();
        };

        Closed += (_, _) =>
        {
            player.Stop();
            player.Dispose();
        };
    }
}