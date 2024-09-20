// Copyright 2024 Genetec
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sdk;
using Sdk.Workflows;

class Program
{
    static Program() => SdkResolver.Initialize();

    static async Task Main()
    {
        const string server = "localhost";
        const string username = "admin";
        const string password = "";

        using var engine = new Engine();

        ConnectionStateCode state = await engine.LoginManager.LogOnAsync(server, username, password);

        if (state == ConnectionStateCode.Success)
        {
            await DisplayLicenseUsageAsync();
        }
        else
        {
            Console.WriteLine($"logon failed: {state}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        async Task DisplayLicenseUsageAsync()
        {
            Console.WriteLine("License Usage Information:");

            Dictionary<string, LicenseUsage> usages = await engine.LicenseManager.GetEveryLicenseItemUsageAsync();
            foreach (var usage in usages)
            {
                Console.WriteLine($"{usage.Key,-40} {usage.Value.CurrentCount} / {usage.Value.MaximumCount}");
            }
        }
    }
}