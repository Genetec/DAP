// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static Program() => SdkResolver.Initialize();

    static async Task Main()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        // Handle Ctrl+C to cancel the loop
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        try
        {
            await LogDebugMessage(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Ignore task canceled
        }
    }

    static async Task LogDebugMessage(CancellationToken token)
    {
        using var logger = new InstanceLogger();

        while (!token.IsCancellationRequested)
        {
            logger.LogDebugMessage();

            StaticLogger.LogDebugMessage();

            await Task.Delay(1000, token);
        }
    }
}