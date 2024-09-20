// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Threading.Tasks;
using Sdk;

class Program
{
    static Program() => SdkResolver.Initialize();

    static async Task Main()
    {
        const string server = "localhost";
        const string username = "admin";
        const string password = "";

        using var engine = new Engine();

        engine.LoginManager.ConnectionRetry = -1;

        engine.LoginManager.LogonStatusChanged += (sender, args) => Console.WriteLine($"Status changed: {args.Status}");
        engine.LoginManager.LoggedOn += (sender, e) => Console.WriteLine($"Logged on to {e.ServerName} using {e.UserName}");
        engine.LoginManager.LoggingOff += (sender, e) => Console.WriteLine("Logging off");
        engine.LoginManager.LoggedOff += (sender, e) => Console.WriteLine($"Logged off. AutoReconnect={e.AutoReconnect}");
        engine.LoginManager.LogonFailed += (sender, e) => Console.WriteLine($"Logon failed: {e.FormattedErrorMessage}");

        ConnectionStateCode state = await engine.LoginManager.LogOnAsync(server, username, password);
        if (state != ConnectionStateCode.Success)
        {
            Console.WriteLine($"logon failed: {state}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}