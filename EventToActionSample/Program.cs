// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Entities.Collections;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            var engine = new Engine();

            var state = await engine.LogOnAsync(server, username, password);

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
                    var customEvent = configuration.CustomEventService.GetCustomEvent(eventToAction.CustomEventId.Value);
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
}