// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

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