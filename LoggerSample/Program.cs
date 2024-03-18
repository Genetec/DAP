// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Sdk.Diagnostics;
    using Sdk.Diagnostics.Logging.Core;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const int diagnosticServerPort = 4523;
            const int webServerPort = 6023;
            const string password = ""; // The password is optional, by default, it's an empty string

            try
            {
                DiagnosticServer.Instance.InitializeServer(diagnosticServerPort: diagnosticServerPort, webServerPort: webServerPort, password: password);

                string url = $"http://localhost:{webServerPort}/Console";

                Console.WriteLine($"Console started: {url}");

                Process.Start(url);

                Log();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            if (DiagnosticServer.IsInstanceCreated && DiagnosticServer.Instance.IsInitialized)
            {
                DiagnosticServer.Instance.Dispose();
            }

            void Log()
            {
                Logger logger = Logger.CreateClassLogger(typeof(Program));
                logger.TraceDebug("This is a debug message");
            }
        }
    }
}
