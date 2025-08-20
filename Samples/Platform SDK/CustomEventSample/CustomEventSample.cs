// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.CustomEvents;
using Genetec.Sdk.Events;

namespace Genetec.Dap.CodeSamples;

public class CustomEventSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // TODO: Replace with your own custom event ID
        const int customEventId = 1000;

        Guid cameraGuid = new Guid("YOUR_CAMERA_GUID_HERE"); // TODO: Replace with your own camera GUID

        var config = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
        ICustomEventService customEventService = config.CustomEventService;

        IReadOnlyList<CustomEvent> events = customEventService.CustomEvents;
        DisplayCustomEvents(events, engine);

        CustomEvent customEvent = await CreateOrGetCustomEvent(customEventService, customEventId);
        RaiseCustomEvent(customEvent, engine, cameraGuid);
    }

    private void DisplayCustomEvents(IReadOnlyList<CustomEvent> events, Engine engine)
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

    private async Task<CustomEvent> CreateOrGetCustomEvent(ICustomEventService customEventService, int eventId)
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

    private void RaiseCustomEvent(CustomEvent customEvent, Engine engine, Guid cameraGuid)
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