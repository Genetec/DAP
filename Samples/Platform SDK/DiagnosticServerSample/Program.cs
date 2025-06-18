// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
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
}
