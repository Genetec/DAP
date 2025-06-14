// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Threading;
using System.Threading.Tasks;
using Sdk;

class Program
{
    static Program() => SdkResolver.Initialize();

    static async Task Main()
    {
        // Connection parameters for your Security Center server
        const string server = "localhost";  // Specify the IP address or hostname of your Security Center server.
        const string username = "admin";    // Enter the username for Security Center authentication.
        const string password = "";         // Provide the corresponding password for the specified username.

        using var engine = new Engine();

        engine.LoginManager.ConnectionRetry = -1; // Auto-reconnect indefinitely

        engine.LoginManager.LogonStatusChanged += (sender, args) => Console.WriteLine($"Status changed: {args.Status}");
        engine.LoginManager.LoggedOn += (sender, e) => Console.WriteLine($"Logged on to server '{e.ServerName}' as user '{e.UserName}'");
        engine.LoginManager.LoggingOff += (sender, e) => Console.WriteLine("Logging off");
        engine.LoginManager.LoggedOff += (sender, e) => Console.WriteLine($"Logged off. AutoReconnect={e.AutoReconnect}");
        engine.LoginManager.LogonFailed += (sender, e) => Console.WriteLine($"Logon Failed | Error Message: {e.FormattedErrorMessage} | Error Code: {e.FailureCode}");

        // Set up cancellation support
        using var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            Console.WriteLine("Cancelling...");
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        Console.WriteLine($"Logging to {server}... Press Ctrl+C to cancel");

        ConnectionStateCode state = await engine.LoginManager.LogOnAsync(server, username, password, cancellationTokenSource.Token);
        if (state != ConnectionStateCode.Success)
        {
            Console.WriteLine($"logon failed: {state}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(true);
    }
}