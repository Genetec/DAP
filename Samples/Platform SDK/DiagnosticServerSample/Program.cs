// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Sdk.Diagnostics;

class Program
{
    static Program() => SdkResolver.Initialize();

    static async Task Main()
    {
        await InitializeAndStartServer();

        LogDebugMessage();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        Cleanup();
    }

    static void LogDebugMessage()
    {
        using var logger = new InstanceLogger();
        logger.LogDebugMessage();

        StaticLogger.LogDebugMessage();
    }

    static async Task InitializeAndStartServer()
    {
        const int diagnosticServerPort = 4523;
        const int webServerPort = 6023;
        const string password = ""; // The password is optional, by default, it's an empty string

        try
        {
            DiagnosticServer.Instance.InitializeServer(diagnosticServerPort, webServerPort, password);

            var url = $"http://localhost:{webServerPort}/Console";
            Console.WriteLine($"Console started: {url}");
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

            // Wait for a few seconds to ensure the web browser has enough time to load the page
            await Task.Delay(5000); // Adjust the delay as necessary
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing server: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    static void Cleanup()
    {
        if (DiagnosticServer.IsInstanceCreated && DiagnosticServer.Instance.IsInitialized)
        {
            DiagnosticServer.Instance.Dispose();
            Console.WriteLine("Diagnostic server disposed.");
        }
    }
}