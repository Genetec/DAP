// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;

namespace Genetec.Dap.CodeSamples
{
    class Program
    {
        static Program() => SdkResolver.Initialize();

        [STAThread]
        static void Main()
        {
            DisplayControls();
            string filePath = ReadFilePath();
            var app = new MediaPlayerApp(filePath);
            app.Run();
        }

        private static void DisplayControls()
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

        private static string ReadFilePath()
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
                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine($"The file {filePath} does not exist. Please try again.");
                    continue;
                }
                return filePath;
            }
        }
    }
}