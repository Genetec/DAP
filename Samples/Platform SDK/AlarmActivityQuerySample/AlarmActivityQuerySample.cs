// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Queries;

public class AlarmActivityQuerySample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Example parameters for GetAlarmInstances
        Guid? specificAlarmId = null; // Set to a specific Guid if you want to filter by alarm ID
        int? specificInstanceId = null; // Set to a specific instance ID if you want to filter by it
        var alarmStates = new[] { AlarmState.Active, AlarmState.AknowledgeRequired }; // Filter by these active and acknowledge required states
        var startTime = DateTime.Now.AddDays(-7); // Filter by alarms triggered in the last 7 days
        var endTime = DateTime.Now;

        ICollection<AlarmInstance> alarmInstances = await GetAlarmInstances(
            engine,
            alarmId: specificAlarmId,
            instanceId: specificInstanceId,
            states: alarmStates,
            startTime: startTime,
            endTime: endTime
        );

        DisplayAlarmInstances(engine, alarmInstances);
    }

    void DisplayAlarmInstances(Engine engine, ICollection<AlarmInstance> alarmInstances)
    {
        Console.WriteLine($"Total Alarm Instances: {alarmInstances.Count}");
        Console.WriteLine();

        foreach (var alarm in alarmInstances)
        {
            DisplayToConsole(alarm);
            Console.WriteLine();
        }

        void DisplayToConsole(AlarmInstance alarm)
        {
            Console.WriteLine($"Instance ID: {alarm.InstanceId}");
            Console.WriteLine($"Alarm: {GetEntityName(alarm.Alarm)}");
            Console.WriteLine($"Trigger Entity: {GetEntityName(alarm.TriggerEntity)}");
            Console.WriteLine($"Trigger Event: {alarm.TriggerEvent}");
            Console.WriteLine($"Trigger Time: {alarm.TriggerTime}");
            Console.WriteLine($"Acked Time: {(alarm.AckedTime == null ? "Not Acked" : alarm.AckedTime)}");
            Console.WriteLine($"Ack By: {(alarm.AckBy == null ? "Not Acked" : GetEntityName(alarm.AckBy.Value))}");
            Console.WriteLine($"Creation Time: {alarm.CreationTime}");
            Console.WriteLine($"Offline Period: {alarm.OfflinePeriod}");
            Console.WriteLine($"Ack Reason: {alarm.AckReason}");
            Console.WriteLine($"External Instance ID: {alarm.ExternalInstanceId}");
            Console.WriteLine($"Investigated By: {(alarm.InvestigatedBy == null ? "Not Investigated" : GetEntityName(alarm.InvestigatedBy.Value))}");
            Console.WriteLine($"Investigated Time: {(alarm.InvestigatedTime == null ? "Not Investigated" : alarm.InvestigatedTime)}");
            Console.WriteLine($"State: {alarm.State}");
            Console.WriteLine($"Has Source Condition: {alarm.HasSourceCondition}");
            Console.WriteLine($"Priority: {alarm.Priority}");
            Console.WriteLine($"Dynamic Context: {alarm.DynamicContext}");

            Console.WriteLine("Attached Entities:");
            foreach (Guid entity in alarm.AttachedEntities)
            {
                Console.WriteLine($"  - {GetEntityName(entity)}");
            }

            Console.WriteLine("Dynamic URLs:");
            foreach (string url in alarm.DynamicUrls)
            {
                Console.WriteLine($"  - {url}");
            }
        }

        string GetEntityName(Guid entityId) => engine.GetEntity(entityId)?.Name ?? "Unknown";
    }

    async Task<ICollection<AlarmInstance>> GetAlarmInstances(Engine engine, Guid? alarmId = null, int? instanceId = null, IEnumerable<AlarmState> states = null, DateTime? startTime = null, DateTime? endTime = null)
    {
        var query = (AlarmActivityQuery)engine.ReportManager.CreateReportQuery(ReportType.AlarmActivity);

        if (alarmId.HasValue)
            query.Alarms.Add(alarmId.Value);

        if (instanceId.HasValue)
            query.InstanceId = instanceId.Value;

        if (states != null)
            query.States.AddRange(states);

        if (startTime.HasValue && endTime.HasValue)
            query.TriggeredTimeRange.SetTimeRange(startTime.Value, endTime.Value);

        AlarmActivityQueryCompletedEventArgs args = (AlarmActivityQueryCompletedEventArgs)await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

        DataTable attachedEntitiesTable = args.DynamicAttachedEntitiesData;
        DataTable urlsTable = args.DynamicUrlsData;

        return args.Data.AsEnumerable().Select(CreateFromDataRow).ToList();

        AlarmInstance CreateFromDataRow(DataRow row)
        {
            int instanceId = row.Field<int>(AlarmActivityQuery.InstanceIdColumnName);

            return new AlarmInstance
            {
                InstanceId = instanceId,
                Alarm = row.Field<Guid>(AlarmActivityQuery.AlarmColumnName),
                TriggerEntity = row.Field<Guid>(AlarmActivityQuery.TriggerEntityColumnName),
                TriggerEvent = (EventType)row.Field<int>(AlarmActivityQuery.TriggerEventColumnName),
                TriggerTime = row.Field<DateTime>(AlarmActivityQuery.TriggerTimeColumnName),
                CreationTime = row.Field<DateTime>(AlarmActivityQuery.CreationTimeColumnName),
                OfflinePeriod = row.Field<OfflinePeriodType>(AlarmActivityQuery.OfflinePriodColumnName),
                AckReason = row.Field<int>(AlarmActivityQuery.AckReasonColumnName),
                ExternalInstanceId = row.Field<int>(AlarmActivityQuery.ExternalInstanceIdColumnName),
                State = (AlarmState)row.Field<byte>(AlarmActivityQuery.StateColumnName),
                HasSourceCondition = row.Field<bool>(AlarmActivityQuery.HasSourceConditionColumnName),
                Priority = row.Field<byte>(AlarmActivityQuery.PriorityColumnName),
                DynamicContext = row.Field<string>(AlarmActivityQuery.DynamicContextColumnName),
                AckedTime = row.Field<DateTime?>(AlarmActivityQuery.AckedTimeColumnName) ?? DateTime.MinValue,
                InvestigatedTime = row.Field<DateTime?>(AlarmActivityQuery.InvestigatedTimeColumnName) ?? DateTime.MinValue,
                AckBy = row.Field<Guid?>(AlarmActivityQuery.AckedByColumnName) ?? Guid.Empty,
                InvestigatedBy = row.Field<Guid?>(AlarmActivityQuery.InvestigatedByColumnName) ?? Guid.Empty,
                AttachedEntities = attachedEntitiesTable.AsEnumerable()
                    .Where(r => r.Field<int>(AlarmActivityQuery.InstanceIdColumnName) == instanceId)
                    .Select(r => r.Field<Guid>(AlarmActivityQuery.DynamicAttachedEntityColumnName))
                    .ToList(),
                DynamicUrls = urlsTable.AsEnumerable()
                    .Where(r => r.Field<int>(AlarmActivityQuery.InstanceIdColumnName) == instanceId)
                    .Select(r => r.Field<string>(AlarmActivityQuery.DynamicUrlColumnName))
                    .ToList()
            };
        }
    }
}