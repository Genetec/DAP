// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdk;
using Sdk.Workflows;

class LicenseManagerSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        Console.WriteLine("License Usage Information:");

        Dictionary<string, LicenseUsage> usages = await engine.LicenseManager.GetEveryLicenseItemUsageAsync();
        foreach (var usage in usages)
        {
            Console.WriteLine($"{usage.Key,-40} {usage.Value.CurrentCount} / {usage.Value.MaximumCount}");
        }
    }
}