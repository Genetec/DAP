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
        DateTime from = DateTime.Now.AddDays(-30);
        DateTime to = DateTime.Now;
        IEnumerable<ActivityType> activities = new [] { ActivityType.UserLoggedOn, ActivityType.UserLogonFailed };
        IEnumerable<ApplicationType> applicationTypes = new [] { ApplicationType.Sdk };
        IEnumerable<Guid> initiators = new[] { engine.LoggedUser.Guid };
        int maximumResultCount = 500;

        ICollection<ActivityTrail> activityTrails = await GetActivityTrails(engine, from, to, activities, applicationTypes, initiators, maximumResultCount);

        DisplayActivityTrails(activityTrails);
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
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

record ActivityTrail
{
    public Guid InitiatorEntityId { get; set; }
    public string InitiatorEntityName { get; set; }
    public EntityType InitiatorEntityType { get; set; }
    public string InitiatorEntityTypeName { get; set; }
    public string InitiatorEntityVersion { get; set; }
    public string Description { get; set; }
    public ActivityType ActivityType { get; set; }
    public string ActivityTypeName { get; set; }
    public Guid ImpactedEntityId { get; set; }
    public string ImpactedEntityName { get; set; }
    public EntityType ImpactedEntityType { get; set; }
    public string ImpactedEntityTypeName { get; set; }
    public string InitiatorMachineName { get; set; }
    public ApplicationType InitiatorApplicationType { get; set; }
    public string InitiatorApplicationName { get; set; }
    public string InitiatorApplicationVersion { get; set; }
    public DateTime EventTimestamp { get; set; }
}