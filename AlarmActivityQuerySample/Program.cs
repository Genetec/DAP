// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.EventsArgs;
using Genetec.Sdk.Queries;

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    const string server = "localhost";
    const string username = "admin";
    const string password = "";

    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state == ConnectionStateCode.Success)
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
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
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

class AlarmInstance
{
    public int InstanceId { get; set; }
    public Guid Alarm { get; set; }
    public Guid TriggerEntity { get; set; }
    public EventType TriggerEvent { get; set; }
    public DateTime TriggerTime { get; set; }
    public DateTime? AckedTime { get; set; }
    public Guid? AckBy { get; set; }
    public DateTime CreationTime { get; set; }
    public OfflinePeriodType OfflinePeriod { get; set; }
    public int AckReason { get; set; }
    public int ExternalInstanceId { get; set; }
    public Guid? InvestigatedBy { get; set; }
    public DateTime? InvestigatedTime { get; set; }
    public AlarmState State { get; set; }
    public bool HasSourceCondition { get; set; }
    public byte Priority { get; set; }
    public string DynamicContext { get; set; }
    public List<Guid> AttachedEntities { get; set; } = new();
    public List<string> DynamicUrls { get; set; } = new();
}