// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;

namespace Genetec.Dap.CodeSamples;

public class ActionMonitoringSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        engine.ActionReceived += (sender, args) =>
        {
            Genetec.Sdk.Actions.Action action = args.Action;

            Console.WriteLine("[Action Received]");
            Console.WriteLine($"\tAction Type: {args.ActionType}");
            Console.WriteLine($"\tAction Name: {action.ActionName}");
            Console.WriteLine($"\tType: {action.Type}");

            // Display recipient information
            if (action.Recipient != Guid.Empty)
            {
                Entity recipient = engine.GetEntity(action.Recipient);
                if (recipient != null)
                {
                    Console.WriteLine($"\tRecipient: {recipient.Name} ({recipient.EntityType})");
                }
                else
                {
                    Console.WriteLine($"\tRecipient ID: {action.Recipient}");
                }
            }

            // Display all recipients
            if (action.AllRecipients != null)
            {
                var recipientsList = string.Join(", ", action.AllRecipients);
                if (!string.IsNullOrEmpty(recipientsList))
                {
                    Console.WriteLine($"\tAll Recipients: {recipientsList}");
                }
            }

            // Display recipient entity types
            if (action.RecipientEntityTypes != null)
            {
                var entityTypes = string.Join(", ", action.RecipientEntityTypes);
                if (!string.IsNullOrEmpty(entityTypes))
                {
                    Console.WriteLine($"\tRecipient Entity Types: {entityTypes}");
                }
            }

            // Display schedule information
            if (action.Schedule != Guid.Empty)
            {
                Entity schedule = engine.GetEntity(action.Schedule);
                if (schedule != null)
                {
                    Console.WriteLine($"\tSchedule: {schedule.Name}");
                }
                else
                {
                    Console.WriteLine($"\tSchedule ID: {action.Schedule}");
                }
            }

            // Display source event information
            if (action.SourceEvent != null)
            {
                Console.WriteLine($"\tSource Event: {action.SourceEvent.Type}");
                Console.WriteLine($"\tSource Event Time: {action.SourceEvent.Timestamp}");
                if (action.SourceEvent.SourceEntity != Guid.Empty)
                {
                    Entity sourceEntity = engine.GetEntity(action.SourceEvent.SourceEntity);
                    if (sourceEntity != null)
                    {
                        Console.WriteLine($"\tSource Entity: {sourceEntity.Name} ({sourceEntity.EntityType})");
                    }
                }
            }

            Console.WriteLine(new string('-', 50));
        };

        Console.WriteLine("Listening to all actions...");
        Console.WriteLine();

        await Task.Delay(Timeout.Infinite, token); // Keep the sample running to listen for actions
    }
}