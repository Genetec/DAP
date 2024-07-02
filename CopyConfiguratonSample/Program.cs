// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.EventsArgs;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

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

            void DisplayResult(CopyConfigResultEventArgs result)
            {
                Console.WriteLine("Copy Configuration Result:");
                Console.WriteLine($"Succeeded: {result.Succeeded}");

                if (!result.Succeeded)
                {
                    Console.WriteLine($"Error: {result.Error}");
                }

                foreach (var status in result.ResultDetails)
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
    }
}
