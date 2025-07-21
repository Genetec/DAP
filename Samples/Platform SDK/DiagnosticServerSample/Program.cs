// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Sdk.Diagnostics;

class Program
{
    static Program() => SdkResolver.Initialize();

    static async Task Main()
    {
        await InitializeAndStartServer();

        using var cancellationTokenSource = new CancellationTokenSource();

        // Handle Ctrl+C to cancel the loop
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        try
        {
            LogDebugMessage(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Ignore task canceled
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        Cleanup();
    }

    static async Task LogDebugMessage(CancellationToken token)
    {
        using var logger = new InstanceLogger();

        while (!token.IsCancellationRequested)
        {
            logger.LogDebugMessage();
            await Task.Delay(1000, token);
        }

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
            DiagnosticServer.Instance.AddFileTracing([new LoggerTraces("Genetec.Dap.CodeSamples", Sdk.Diagnostics.Logging.Core.LogSeverity.All)]);

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