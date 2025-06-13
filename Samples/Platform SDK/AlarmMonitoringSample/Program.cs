// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Threading.Tasks;
    using Genetec.Sdk;
    using Genetec.Sdk.Entities;

    class Program
    {
        static Program() => SdkResolver.Initialize();

        static async Task Main()
        {
            const string server = "localhost";
            const string username = "admin";
            const string password = "";

            using var engine = new Engine();

            engine.AlarmTriggered += OnAlarmTriggered;
            engine.AlarmAcknowledged += OnAlarmAcknowledged;
            engine.AlarmInvestigating += OnAlarmInvestigating;
            engine.AlarmSourceConditionCleared += OnAlarmSourceConditionCleared;

            ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

            if (state != ConnectionStateCode.Success)
                Console.WriteLine($"Logon failed: {state}");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            void OnAlarmTriggered(object sender, AlarmTriggeredEventArgs e)
            {
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
                var alarm = (Alarm)engine.GetEntity(e.AlarmGuid);

                Console.WriteLine("[Alarm Source Condition Cleared]");
                Console.WriteLine($"\tName: {alarm.Name}");
                Console.WriteLine($"\tInstance ID: {e.InstanceId}");
                Console.WriteLine($"\tAcknowledgement Required: {e.AcknowledgeActionRequired}");
                Console.WriteLine(new string('-', 50));
            }
        }
    }
}
