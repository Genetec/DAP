﻿// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Genetec.Sdk.Queries;
    using Genetec.Sdk;
    using Genetec.Sdk.Entities;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            ConnectionStateCode loginState = await engine.LogOnAsync(server, username, password);

            if (loginState == ConnectionStateCode.Success)
            {
                // Load all files into the Engine's entity cache
                await LoadFiles();

                // Get all audio files from the Engine's entity cache
                IEnumerable<File> audioFiles = engine.GetEntities(EntityType.File).OfType<File>().Where(file => file.FileType == FileType.Audio);

                foreach (File file in audioFiles)
                {
                    Console.WriteLine($"Playing sound: {file.Name}");

                    engine.ActionManager.PlaySound(UserGroup.AdministratorsUserGroupGuid, file.Guid);

                    await Task.Delay(2000); // Wait 2 seconds between each sound
                }
            }
            else
            {
                Console.WriteLine($"Login failed: {loginState}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            async Task LoadFiles()
            {
                Console.WriteLine("Loading files...");

                var query = (EntityConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.EntityConfiguration);
                query.EntityTypeFilter.Add(EntityType.File);
                await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
            }
        }
    }
}
