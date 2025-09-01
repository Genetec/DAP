// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Collections;
using Genetec.Sdk.Entities.CustomEvents;

namespace Genetec.Dap.CodeSamples;

public class EventToActionSample : SampleBase
{
    protected override Task RunAsync(Engine engine, CancellationToken token)
    {
        PrintEventToActions(engine);

        return Task.CompletedTask;
    }

    void PrintEventToActions(Engine engine)
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
            PrintEventToActionDetails(engine, configuration, eventToAction);
            Console.WriteLine();
        }
    }

    void PrintEventToActionDetails(Engine engine, SystemConfiguration configuration, EventToAction eventToAction)
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