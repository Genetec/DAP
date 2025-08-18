// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

public class LoginManagerSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // This sample demonstrates the LoginManager functionality which is already implemented 
        // in the SampleBase class. The base class handles connection setup, authentication,
        // event logging, and error handling. We simply wait here until cancellation is requested
        // to keep the application running and observe the login manager events.
        await Task.Delay(-1, token);
    }
}