// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

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
        IEnumerable<ApplicationType> applicationTypes = new[] { ApplicationType.Sdk };
        IEnumerable<Guid> users = new[] { engine.LoggedUser.Guid };
        int maximumResultCount = 500;

        ICollection<AuditTrail> auditTrails = await GetAuditTrails(engine, from, to, applicationTypes, users, maximumResultCount);

        DisplayAuditTrails(auditTrails);
    }
    else
    {
        Console.WriteLine($"Logon failed: {state}");
    }

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

async Task<ICollection<AuditTrail>> GetAuditTrails(Engine engine, DateTime from, DateTime to, IEnumerable<ApplicationType> applicationTypes, IEnumerable<Guid> users, int maximumResultCount)
{
    var query = (AuditTrailQuery)engine.ReportManager.CreateReportQuery(ReportType.AuditTrailsReport);
    query.MaximumResultCount = maximumResultCount;
    query.TimeRange.SetTimeRange(from, to);
    query.Applications.AddRange(applicationTypes);
    query.Users.AddRange(users);
   
    QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);

    return args.Data.AsEnumerable().Select(CreateFromDataRow).ToList();

    AuditTrail CreateFromDataRow(DataRow row) => new()
    {
        Id = row.Field<int>(AuditTrailQuery.IdColumnName),
        EntityGuid = row.Field<Guid>(AuditTrailQuery.EntityGuidColumnName),
        ModificationTimestamp = row.Field<DateTime>(AuditTrailQuery.ModificationTimestampColumnName),
        ModifiedBy = row.Field<Guid>(AuditTrailQuery.ModifiedByColumnName),
        SourceApplicationGuid = row.Field<Guid>(AuditTrailQuery.SourceApplicationGuidColumnName),
        Name = row.Field<string>(AuditTrailQuery.NameColumnName),
        ModifiedByAsString = row.Field<string>(AuditTrailQuery.ModifiedByAsStringColumnName),
        SourceApplicationAsString = row.Field<string>(AuditTrailQuery.SourceApplicationAsStringColumnName),
        Machine = row.Field<string>(AuditTrailQuery.MachineColumnName),
        SourceApplicationType = row.Field<ApplicationType>(AuditTrailQuery.SourceApplicationTypeColumnName),
        OldValue = row.Field<string>(AuditTrailQuery.OldValueColumnName),
        NewValue = row.Field<string>(AuditTrailQuery.NewValueColumnName),
        CustomFieldId = row.Field<Guid>(AuditTrailQuery.CustomFieldIdColumnName),
        CustomFieldName = row.Field<string>(AuditTrailQuery.CustomFieldNameColumnName),
        CustomFieldValueType = (CustomFieldValueType)row.Field<int>(AuditTrailQuery.CustomFieldValueTypeColumnName),
        ModificationType = row.Field<AuditTrailModificationType>(AuditTrailQuery.AuditTrailModificationTypeColumnName),
        EntityType = row.Field<EntityType>(AuditTrailQuery.EntityTypeColumnName),
        Description = row.Field<string>(AuditTrailQuery.DescriptionColumnName),
        Value = row.Field<string>(AuditTrailQuery.ValueColumnName)
    };
}

void DisplayAuditTrails(ICollection<AuditTrail> auditTrails)
{
    Console.WriteLine($"Total Audit Trails: {auditTrails.Count}");
    Console.WriteLine();

    foreach (AuditTrail trail in auditTrails)
    {
        Console.WriteLine($"Audit Trail ID: {trail.Id}");
        Console.WriteLine($"Entity: {trail.Name} (GUID: {trail.EntityGuid})");
        Console.WriteLine($"Modification Time: {trail.ModificationTimestamp}");
        Console.WriteLine($"Modified By: {trail.ModifiedByAsString} (GUID: {trail.ModifiedBy})");
        Console.WriteLine($"Application: {trail.SourceApplicationAsString} (GUID: {trail.SourceApplicationGuid})");
        Console.WriteLine($"Machine: {trail.Machine}");
        Console.WriteLine($"Modification Type: {trail.ModificationType}");
        Console.WriteLine($"Old Value: {trail.OldValue}");
        Console.WriteLine($"New Value: {trail.NewValue}");
        Console.WriteLine($"Description: {trail.Description}");
        Console.WriteLine("------------------------");
    }
}

record AuditTrail
{
    public int Id { get; set; }
    public Guid EntityGuid { get; set; }
    public DateTime ModificationTimestamp { get; set; }
    public Guid ModifiedBy { get; set; }
    public Guid SourceApplicationGuid { get; set; }
    public string Name { get; set; }
    public string ModifiedByAsString { get; set; }
    public string SourceApplicationAsString { get; set; }
    public string Machine { get; set; }
    public ApplicationType SourceApplicationType { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public Guid CustomFieldId { get; set; }
    public string CustomFieldName { get; set; }
    public CustomFieldValueType CustomFieldValueType { get; set; }
    public AuditTrailModificationType ModificationType { get; set; }
    public EntityType EntityType { get; set; }
    public string Description { get; set; }
    public string Value { get; set; }
}