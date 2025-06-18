// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Sdk;
    using Sdk.Entities;
    using Sdk.Entities.CustomEvents;
    using Sdk.Events;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";
            
            // TODO: Replace with your own custom event ID
            const int customEventId = 1000;

            // TODO: Replace with your own camera GUID
            Guid cameraGuid = new Guid("YOUR_CAMERA_GUID_HERE");

            using var engine = new Engine();

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state == ConnectionStateCode.Success)
            {
                var config = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
                ICustomEventService customEventService = config.CustomEventService;

                IReadOnlyList<CustomEvent> events = customEventService.CustomEvents;
                DisplayCustomEvents(events);

                CustomEvent customEvent = await CreateOrGetCustomEvent(customEventService, customEventId);
                RaiseCustomEvent(customEvent);
            }
            else
            {
                Console.WriteLine($"Logon failed: {state}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            void DisplayCustomEvents(IReadOnlyList<CustomEvent> events)
            {
                Console.WriteLine($"Total Number of Custom Events: {events.Count}");

                foreach (var group in events.GroupBy(e => e.SourceEntityType).OrderBy(g => g.Key))
                {
                    Console.WriteLine($"\nSource Entity Type: {group.Key}");
                    Console.WriteLine(new string('-', 30));

                    foreach (var customEvent in group.OrderBy(e => e.Id))
                    {
                        Console.WriteLine($"Name: {customEvent.Name}");
                        Console.WriteLine($"  ID: {customEvent.Id}");
                        Console.WriteLine($"  Local: {customEvent.LocalCustomEvent}");

                        if (engine.GetEntity(customEvent.Owner) is Role owner)
                            Console.WriteLine($"  Owner: {owner.Name}");
                        else
                            Console.WriteLine("  Owner: LocalSystem");

                        Console.WriteLine();
                    }
                }
            }

            async Task<CustomEvent> CreateOrGetCustomEvent(ICustomEventService customEventService, int eventId)
            {
                CustomEvent customEvent = customEventService.CustomEvents.FirstOrDefault(@event => @event.Id == eventId);
                if (customEvent is null)
                {
                    Console.WriteLine($"Creating new custom event with ID: {eventId}");

                    ICustomEventBuilder builder = customEventService.CreateCustomEventBuilder();

                    customEvent = builder.SetEntityType(EntityType.Camera)
                        .SetId(eventId)
                        .SetName("Camera custom event")
                        .Build();

                    await customEventService.AddAsync(customEvent);
                }
                else
                {
                    Console.WriteLine($"Using existing custom event with ID: {eventId}");
                }
                return customEvent;
            }

            void RaiseCustomEvent(CustomEvent customEvent)
            {
                Console.WriteLine("Raising custom event");

                Entity source = engine.GetEntity(cameraGuid);
                if (source is null)
                {
                    Console.WriteLine($"Error: No entity found with GUID {cameraGuid}");
                    return;
                }

                var eventInstance = (CustomEventInstance)engine.ActionManager.BuildEvent(EventType.CustomEvent, cameraGuid);
                eventInstance.Id = new CustomEventId(customEvent.Id);
                eventInstance.Message = "Custom event message";
                eventInstance.ExtraHiddenPayload = "Custom event extra payload";

                engine.ActionManager.RaiseEvent(eventInstance);
            }
        }
    }
}
