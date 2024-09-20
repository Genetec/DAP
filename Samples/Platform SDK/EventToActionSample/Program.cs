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
using Sdk.Entities.Collections;
using Sdk.Entities.CustomEvents;

class Program
{
    static Program() => SdkResolver.Initialize();

    static async Task Main()
    {
        const string server = "localhost";
        const string username = "admin";
        const string password = "";

        var engine = new Engine();

        ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

        if (state == ConnectionStateCode.Success)
        {
            PrintEventToActions(engine);
        }
        else
        {
            Console.WriteLine($"Logon failed: {state}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void PrintEventToActions(Engine engine)
    {
        var configuration = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);

        foreach (EventToAction eventToAction in configuration.EventToActions)
        {
            Console.WriteLine($"Id: {eventToAction.Id}");

            if (eventToAction.EventType == EventType.CustomEvent && eventToAction.CustomEventId.HasValue)
            {
                CustomEvent customEvent = configuration.CustomEventService.GetCustomEvent(eventToAction.CustomEventId.Value);
                Console.WriteLine($"Event: {customEvent.Name} ({customEvent.SourceEntityType})");
            }
            else
            {
                Console.WriteLine($"Event: {eventToAction.EventType}");
            }

            if (!string.IsNullOrEmpty(eventToAction.CustomCondition))
            {
                Console.WriteLine($"Custom Condition: {eventToAction.CustomCondition}");
            }

            Console.WriteLine($"Action: {eventToAction.Action.Type}");
        }
    }
}