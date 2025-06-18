// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;
using Genetec.Sdk.Queries.AccessControl;

const string server = "localhost";
const string username = "admin";
const string password = "";

SdkResolver.Initialize();

await RunSample();

async Task RunSample()
{
    using var engine = new Engine();

    ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

    if (state == ConnectionStateCode.Success)
    {
        DateTime from = DateTime.Now.AddDays(-30);
        DateTime to = DateTime.Now;
        int maxResultCount = 10;
        Guid entity = SystemConfiguration.SystemConfigurationGuid; // All entities

        var reportTypes = new[]
        {
            ReportType.AccessControlUnitActivity,
            ReportType.CredentialActivity,
            ReportType.CardholderActivity,
            ReportType.AreaActivity,
            ReportType.DoorActivity,
            ReportType.ZoneActivity,
            ReportType.ElevatorActivity,
            ReportType.VisitorActivity
        };
  
        foreach (ReportType reportType in reportTypes)
        {
            List<AccessControlEvent> activities = await GetActivities(engine, reportType, from, to, maxResultCount, entity);

            Console.WriteLine($"\nFound {activities.Count} activities.\n");

            DisplayToConsole(engine, activities);
        }
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

async Task<List<AccessControlEvent>> GetActivities(Engine engine, ReportType reportType, DateTime from, DateTime to, int maxResultCount, Guid entityGuid)
{
    Console.WriteLine($"Querying {reportType}...");

    var query = (AccessControlReportQuery)engine.ReportManager.CreateReportQuery(reportType);
    query.TimeRange.SetTimeRange(from, to);
    query.MaximumResultCount = maxResultCount;

    if (query is VisitorActivityQuery visitorQuery)
    {
        visitorQuery.Visitor = entityGuid;
    }
    else
    {
        query.QueryEntities.Add(entityGuid);
    }

    QueryCompletedEventArgs results = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

    return results.Data.AsEnumerable().Select(CreateAccessControlEvent).ToList();

    AccessControlEvent CreateAccessControlEvent(DataRow row) => new()
    {
        Timestamp = DateTime.SpecifyKind(row.Field<DateTime>(AccessControlReportQuery.TimestampColumnName), DateTimeKind.Utc),
        Unit = row.Field<Guid?>(AccessControlReportQuery.UnitGuidColumnName),
        AccessPoint = row.Field<Guid?>(AccessControlReportQuery.APGuidColumnName),
        AccessPointGroup = row.Field<Guid?>(AccessControlReportQuery.AccessPointGroupGuidColumnName),
        Credential = row.Field<Guid?>(AccessControlReportQuery.CredentialGuidColumnName),
        Credential2 = row.Field<Guid?>(AccessControlReportQuery.Credential2GuidColumnName),
        Device = row.Field<Guid?>(AccessControlReportQuery.DeviceGuidColumnName),
        CustomEventMessage = row.Field<string>(AccessControlReportQuery.CustomEventMessageColumnName),
        EventType = row.Field<EventType>(AccessControlReportQuery.EventTypeColumnName),
        Source = row.Field<Guid?>(AccessControlReportQuery.SourceGuidColumnName),
        Cardholder = row.Field<Guid?>(AccessControlReportQuery.CardholderGuidColumnName),
        OccurrencePeriod = row.Field<OfflinePeriodType>(AccessControlReportQuery.OccurrencePeriodColumnName),
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById(row.Field<string>(AccessControlReportQuery.TimeZoneColumnName))
    };
}

void DisplayToConsole(Engine engine, List<AccessControlEvent> activities)
{
    activities.ForEach(activity =>
    {
        Console.WriteLine($"Timestamp: {activity.Timestamp}, EventType: {activity.EventType}");
        Console.WriteLine($"  Unit: {GetEntityName(activity.Unit)}");
        Console.WriteLine($"  Device: {GetEntityName(activity.Device)}");
        Console.WriteLine($"  AccessPointGroup: {GetEntityName(activity.AccessPointGroup)}");
        Console.WriteLine($"  AccessPoint: {GetEntityName(activity.AccessPoint)}");
        Console.WriteLine($"  Credential: {GetEntityName(activity.Credential)}");
        Console.WriteLine($"  Credential2: {GetEntityName(activity.Credential2)}");
        Console.WriteLine($"  Cardholder: {GetEntityName(activity.Cardholder)}");
        Console.WriteLine();
    });

    Console.WriteLine(new string('-', 50));

    string GetEntityName(Guid? entityId) => engine.GetEntity(entityId.GetValueOrDefault())?.Name;
}

class AccessControlEvent
{
    public DateTime Timestamp { get; set; }
    public Guid? Unit { get; set; }
    public Guid? AccessPoint { get; set; }
    public Guid? AccessPointGroup { get; set; }
    public Guid? Credential { get; set; }
    public Guid? Credential2 { get; set; }
    public Guid? Device { get; set; }
    public string CustomEventMessage { get; set; }
    public EventType EventType { get; set; }
    public Guid? Source { get; set; }
    public TimeZoneInfo TimeZone { get; set; }
    public OfflinePeriodType OccurrencePeriod { get; set; }
    public Guid? Cardholder { get; set; }
}
