// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Threading.Tasks;
using Sdk;
using Sdk.Entities;
using Sdk.Media.Reader;

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
            var camera = (Camera)engine.GetEntity(EntityType.Camera, 1);

            await using var reader = PlaybackStreamReader.CreateVideoReader(engine, camera.Guid);
            await reader.ConnectAsync();
            await reader.SeekAsync(DateTime.UtcNow.AddMinutes(-1));
                
            while (true)
            {
                RawDataContent content = await reader.ReadAsync();
                if (content is null)
                {
                    Console.WriteLine("End reached");
                    break;
                }

                using (content)
                {
                    Console.WriteLine($"Frame time {content.FrameTime}, Format: {content.Format}, Data size: {content.Data.Count}");
                }
            }
        }
        else
        {
            Console.WriteLine($"logon failed: {state}");
        }
            
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}