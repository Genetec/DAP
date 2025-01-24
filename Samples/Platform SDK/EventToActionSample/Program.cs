// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Collections;
using Genetec.Sdk.Entities.CustomEvents;

const string server = "localhost";
const string username = "admin";
const string password = "";

SdkResolver.Initialize();
await RunSample();

Console.Write("Press any key to exit...");
Console.ReadKey(true);

async Task RunSample()
{
    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state != ConnectionStateCode.Success)
    {
        Console.WriteLine($"Logon failed: {state}");
        return;
    }

    PrintEventToActions();

    void PrintEventToActions()
    {
        var configuration = (SystemConfiguration)engine.GetEntity(SystemConfiguration.SystemConfigurationGuid);
        Console.WriteLine("Event to Actions:");

        if (!configuration.EventToActions.Any())
        {
            Console.WriteLine("No event to actions found.");
            return;
        }

        foreach (EventToAction eventToAction in configuration.EventToActions)
        {
            Console.WriteLine(new string('-', 50));
            PrintEventToActionDetails(eventToAction);
            Console.WriteLine();
        }

        void PrintEventToActionDetails(EventToAction eventToAction)
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

            Console.WriteLine($"Action: {eventToAction.Action.Type}");

            Entity recipient = engine.GetEntity(eventToAction.Action.Recipient);
            Console.WriteLine(recipient != null ? $"Recipient: {recipient.Name}" : "Recipient: None");

            Console.WriteLine(!string.IsNullOrEmpty(eventToAction.CustomCondition)
                ? $"Custom Condition: {eventToAction.CustomCondition}"
                : "Custom Condition: None");

            Entity schedule = engine.GetEntity(eventToAction.Action.Schedule) as Schedule;
            Console.WriteLine(schedule != null ? $"Schedule: {schedule.Name}" : "Schedule: Always");
        }
    }
}