// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Threading;
using System.Threading.Tasks;
using Sdk;
using Sdk.CopyConfiguration;
using Sdk.EventsArgs;

public class CopyConfigurationSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
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
}