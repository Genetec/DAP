// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;

class AlarmMonitoringSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        engine.AlarmTriggered += OnAlarmTriggered;
        engine.AlarmAcknowledged += OnAlarmAcknowledged;
        engine.AlarmInvestigating += OnAlarmInvestigating;
        engine.AlarmSourceConditionCleared += OnAlarmSourceConditionCleared;

        await Task.Delay(Timeout.Infinite, token); // Keep the sample running to listen for events
    }

    void OnAlarmTriggered(object sender, AlarmTriggeredEventArgs e)
    {
        var engine = (Engine)sender;
        var alarm = (Alarm)engine.GetEntity(e.AlarmGuid);

        Console.WriteLine("[Alarm Triggered]");
        Console.WriteLine($"\tName: {alarm.Name}");
        Console.WriteLine($"\tInstance ID: {e.InstanceId}");
        Console.WriteLine($"\tTrigger time: {e.TriggerTimestamp}");
        Console.WriteLine($"\tContext: {e.DynamicAlarmContent?.Context}");
        Console.WriteLine($"\tTrigger Event: {e.TriggerEvent}");
        Console.WriteLine($"\tOccurence Period: {e.OfflinePeriod}");

        if (engine.GetEntity(e.SourceGuid) is { } entity)
        {
            Console.WriteLine($"\tSource: {entity.Name}");
            Console.WriteLine($"\tSource entity type: {entity.EntityType}");
        }

        Console.WriteLine(new string('-', 50));
    }

    void OnAlarmAcknowledged(object sender, AlarmAcknowledgedEventArgs e)
    {
        var engine = (Engine)sender;
        var alarm = (Alarm)engine.GetEntity(e.AlarmGuid);

        Console.WriteLine("[Alarm Acknowledged]");
        Console.WriteLine($"\tName: {alarm.Name}");
        Console.WriteLine($"\tInstance ID: {e.InstanceId}");
        Console.WriteLine($"\tAcknowledged on: {e.AckTime}");

        if (engine.GetEntity(e.AckBy) is { } entity)
        {
            Console.WriteLine($"\tAcknowledged by: {entity.Name}");
        }

        Console.WriteLine(new string('-', 50));
    }

    void OnAlarmInvestigating(object sender, AlarmInvestigatingEventArgs e)
    {
        var engine = (Engine)sender;
        var alarm = (Alarm)engine.GetEntity(e.AlarmGuid);

        Console.WriteLine("[Alarm Investigating]");
        Console.WriteLine($"\tName: {alarm.Name}");
        Console.WriteLine($"\tInstance ID: {e.InstanceId}");
        Console.WriteLine($"\tInvestigated on: {e.InvestigatedTime}");

        if (engine.GetEntity(e.InvestigatedBy) is { } entity)
        {
            Console.WriteLine($"\tInvestigated by: {entity.Name}");
        }

        Console.WriteLine(new string('-', 50));
    }

    void OnAlarmSourceConditionCleared(object sender, AlarmSourceConditionClearedEventArgs e)
    {
        var engine = (Engine)sender;
        var alarm = (Alarm)engine.GetEntity(e.AlarmGuid);

        Console.WriteLine("[Alarm Source Condition Cleared]");
        Console.WriteLine($"\tName: {alarm.Name}");
        Console.WriteLine($"\tInstance ID: {e.InstanceId}");
        Console.WriteLine($"\tAcknowledgement Required: {e.AcknowledgeActionRequired}");
        Console.WriteLine(new string('-', 50));
    }
}