// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.CopyConfiguration;
using Genetec.Sdk.EventsArgs;

const string server = "localhost";
const string username = "admin";
const string password = "";

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state == ConnectionStateCode.Success)
    {
        // TODO: Replace the following example GUIDs with the actual source and destination GUIDs.
        // These GUIDs should correspond to the entities you want to copy the configuration from and to.
        var source = new Guid("00000000-0000-0000-0000-000000000000");
        var destination = new Guid("00000000-0000-0000-0000-000000000000");

        // TODO: Define any options required for the copy operation.
        // This array should contain the specific options you need for your copy configuration.
        CopyConfigOption[] options = { 
            // Add your CopyConfigOption values here
        };

        var progress = new Progress<int>(percent => Console.WriteLine($"Progress: {percent}%"));

        try
        {
            CopyConfigResultEventArgs result = await engine.EntityManager.CopyConfigurationAsync(source, new[] { destination }, progress, options);
            DisplayResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during configuration copy: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

void DisplayResult(CopyConfigResultEventArgs result)
{
    Console.WriteLine("Copy Configuration Result:");
    Console.WriteLine($"Succeeded: {result.Succeeded}");

    if (!result.Succeeded)
    {
        Console.WriteLine($"Error: {result.Error}");
    }

    foreach (CopyConfigStatus status in result.ResultDetails)
    {
        Console.WriteLine($"Entity GUID: {status.EntityGuid}");
        Console.WriteLine($"Succeeded: {status.Succeeded}");

        if (!status.Succeeded)
        {
            Console.WriteLine($"Error Message: {status.ErrorMessage}");

            if (status.Exception != null)
            {
                Console.WriteLine($"Exception: {status.Exception.Message}");
            }
        }
    }
}
