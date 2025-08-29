// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sdk;
using Sdk.Queries;

public class ActivityTrailsSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        DateTime from = DateTime.Now.AddDays(-30);
        DateTime to = DateTime.Now;
        IEnumerable<ActivityType> activities = [ActivityType.UserLoggedOn, ActivityType.UserLogonFailed];
        IEnumerable<ApplicationType> applicationTypes = [ApplicationType.Sdk];
        IEnumerable<Guid> initiators = [engine.LoggedUser.Guid];
        int maximumResultCount = 500;

        ICollection<ActivityTrail> activityTrails = await GetActivityTrails(engine, from, to, activities, applicationTypes, initiators, maximumResultCount);

        DisplayActivityTrails(activityTrails);
    }

    async Task<ICollection<ActivityTrail>> GetActivityTrails(Engine engine, DateTime from, DateTime to, IEnumerable<ActivityType> activities, IEnumerable<ApplicationType> applicationTypes, IEnumerable<Guid> initiators, int maximumResultCount)
    {
        var query = (ActivityTrailsQuery)engine.ReportManager.CreateReportQuery(ReportType.ActivityTrailsReport);
        query.MaximumResultCount = maximumResultCount;
        query.TimeRange.SetTimeRange(from, to);
        query.Activities.AddRange(activities);
        query.Applications.AddRange(applicationTypes);
        query.InitiatorEntities.AddRange(initiators);

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

        return args.Data.AsEnumerable().Select(CreateFromDataRow).ToList();

        ActivityTrail CreateFromDataRow(DataRow row) => new()
        {
            InitiatorEntityId = row.Field<Guid>(ActivityTrailsQuery.InitiatorEntityIdColumnName),
            InitiatorEntityName = row.Field<string>(ActivityTrailsQuery.InitiatorEntityNameColumnName),
            InitiatorEntityType = (EntityType)row.Field<int>(ActivityTrailsQuery.InitiatorEntityTypeColumnName),
            InitiatorEntityTypeName = row.Field<string>(ActivityTrailsQuery.InitiatorEntityTypeNameColumnName),
            InitiatorEntityVersion = row.Field<string>(ActivityTrailsQuery.InitiatorEntityVersionColumnName),
            Description = row.Field<string>(ActivityTrailsQuery.DescriptionColumnName),
            ActivityType = (ActivityType)row.Field<int>(ActivityTrailsQuery.ActivityTypeColumnName),
            ActivityTypeName = row.Field<string>(ActivityTrailsQuery.ActivityTypeNameColumnName),
            ImpactedEntityId = row.Field<Guid>(ActivityTrailsQuery.ImpactedEntityIdColumnName),
            ImpactedEntityName = row.Field<string>(ActivityTrailsQuery.ImpactedEntityNameColumnName),
            ImpactedEntityType = (EntityType)row.Field<int>(ActivityTrailsQuery.ImpactedEntityTypeColumnName),
            ImpactedEntityTypeName = row.Field<string>(ActivityTrailsQuery.ImpactedEntityTypeNameColumnName),
            InitiatorMachineName = row.Field<string>(ActivityTrailsQuery.InitiatorMachineNameColumnName),
            InitiatorApplicationType = (ApplicationType)row.Field<int>(ActivityTrailsQuery.InitiatorApplicationTypeColumnName),
            InitiatorApplicationName = row.Field<string>(ActivityTrailsQuery.InitiatorApplicationNameColumnName),
            InitiatorApplicationVersion = row.Field<string>(ActivityTrailsQuery.InitiatorApplicationVersionColumnName),
            EventTimestamp = row.Field<DateTime>(ActivityTrailsQuery.EventTimestampColumnName)
        };
    }

    void DisplayActivityTrails(ICollection<ActivityTrail> activityTrails)
    {
        Console.WriteLine($"Total Activity Trails: {activityTrails.Count}");
        Console.WriteLine();

        foreach ((ActivityTrail trail, int index) in activityTrails.Select((t, i) => (t, i + 1)))
        {
            Console.WriteLine($"Activity Trail #{index}:");
            Console.WriteLine("------------------------");
            DisplayToConsole(trail);
            Console.WriteLine("------------------------");
            Console.WriteLine();
        }

        void DisplayToConsole(ActivityTrail activityTrail)
        {
            Console.WriteLine($"Initiator: {activityTrail.InitiatorEntityName} (ID: {activityTrail.InitiatorEntityId})");
            Console.WriteLine($"Entity Type: {activityTrail.InitiatorEntityTypeName}");
            Console.WriteLine($"Description: {activityTrail.Description}");
            Console.WriteLine($"Activity: {activityTrail.ActivityTypeName}");
            Console.WriteLine($"Impacted: {activityTrail.ImpactedEntityName} (ID: {activityTrail.ImpactedEntityId})");
            Console.WriteLine($"Machine: {activityTrail.InitiatorMachineName}");
            Console.WriteLine($"Application: {activityTrail.InitiatorApplicationName}");
            Console.WriteLine($"Timestamp: {activityTrail.EventTimestamp}");
        }
    }
}