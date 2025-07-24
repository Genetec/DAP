// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

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
        const string server = "localhost"; // TODO: Replace with the IP address or hostname of your Security Center server.
        const string username = "admin"; // TODO: Replace with the username for Security Center authentication.
        const string password = ""; // TODO: Replace with the corresponding password for the specified username.

        // TODO: Replace with your own custom event ID
        const int customEventId = 1000;

        // TODO: Replace with your own camera GUID
        Guid cameraGuid = new Guid("00000001-0000-babe-0000-26551ec56587");

        // Create two engines for sender and receiver
        // The sender will raise the custom event, and the receiver will listen for it
        // Both engines will connect to the same Security Center server
        using var sender = new Engine();
        using var receiver = new Engine();

        // Set up event handler to listen for custom events
        // This will only receive custom events raised by any client connected to the same server
        receiver.SetEventFilter([EventType.CustomEvent]);
        receiver.EventReceived += (_, e) =>
        {
            if (e.EventType == EventType.CustomEvent && e.Event is CustomEventInstance eventInstance)
            {
                string payLoad = eventInstance.ExtraHiddenPayload;

                Entity source = receiver.GetEntity(eventInstance.SourceEntity);
                Console.WriteLine($"Custom event received from {source.Name} with payload: {payLoad}");
            }
        };

        // Log on both engines
        ConnectionStateCode[] states = await Task.WhenAll(sender.LogOnAsync(server, username, password), receiver.LogOnAsync(server, username, password));

        // Check if both engines are successfully logged on
        if (states.All(state => state == ConnectionStateCode.Success))
        {
            // Ensure the camera is loaded into the receiver engine entity cache otherwise it will not receive the event
            // By calling GetEntity, we ensure that the camera is loaded into the entity cache.
            if (receiver.GetEntity(cameraGuid) is not Camera)
            {
                Console.WriteLine($"Camera not found: {cameraGuid}");
                return;
            }

            var config = (SystemConfiguration)sender.GetEntity(SystemConfiguration.SystemConfigurationGuid);
            ICustomEventService customEventService = config.CustomEventService;

            IReadOnlyList<CustomEvent> events = customEventService.CustomEvents;
            DisplayCustomEvents(events);

            CustomEvent customEvent = await GetOrCreateCustomEvent(customEventService, customEventId);
            RaiseCustomEvent(customEvent);
        }
        else
        {
            Console.WriteLine($"Logon failed - Sender: {states[0]}, Receiver: {states[1]}");
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

                    if (sender.GetEntity(customEvent.Owner) is Role owner)
                        Console.WriteLine($"  Owner: {owner.Name}");
                    else
                        Console.WriteLine("  Owner: LocalSystem");

                    Console.WriteLine();
                }
            }
        }

        async Task<CustomEvent> GetOrCreateCustomEvent(ICustomEventService customEventService, int eventId)
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

            var eventInstance = (CustomEventInstance)sender.ActionManager.BuildEvent(EventType.CustomEvent, cameraGuid);
            eventInstance.Id = new CustomEventId(customEvent.Id);
            eventInstance.Message = "Custom event message";
            eventInstance.ExtraHiddenPayload = "Extra hidden payload";

            sender.ActionManager.RaiseEvent(eventInstance);
        }
    }
}