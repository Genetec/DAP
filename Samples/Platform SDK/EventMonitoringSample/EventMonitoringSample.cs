// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class EventMonitoringSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        engine.SetEventFilter(new[]
        {
            EventType.UnitConnected,
            EventType.UnitDisconnected,
            EventType.InterfaceOnline,
            EventType.InterfaceOffline,
            EventType.EntityWarning
        });

        engine.EventReceived += (sender, e) =>
        {
            Entity entity = engine.GetEntity(e.SourceGuid);

            switch (e.EventType)
            {
                case EventType.UnitConnected:
                    Console.WriteLine($"{entity.Name} connected.");
                    break;
                case EventType.UnitDisconnected:
                    Console.WriteLine($"{entity.Name} disconnected.");
                    break;
                case EventType.InterfaceOnline:
                    Console.WriteLine($"{entity.Name} interface online.");
                    break;
                case EventType.InterfaceOffline:
                    Console.WriteLine($"{entity.Name} interface offline.");
                    break;
                case EventType.EntityWarning:
                    Console.WriteLine($"{entity.Name} warning.");
                    break;
            }
        };

        await LoadEntities(engine, token, EntityType.Unit);

        Console.WriteLine($"Listening to events: {string.Join(",", engine.GetEventFilter())}");

        await Task.Delay(Timeout.Infinite, token); // Keep the sample running to listen for events
    }
}