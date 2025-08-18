using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

class AuditTrailsSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        DateTime from = DateTime.Now.AddDays(-30);
        DateTime to = DateTime.Now;
        IEnumerable<ApplicationType> applicationTypes = [ApplicationType.Sdk];
        IEnumerable<Guid> users = [engine.LoggedUser.Guid];
        int maximumResultCount = 500;

        ICollection<AuditTrail> auditTrails = await GetAuditTrails(engine, from, to, applicationTypes, users, maximumResultCount);

        DisplayAuditTrails(auditTrails);
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
}