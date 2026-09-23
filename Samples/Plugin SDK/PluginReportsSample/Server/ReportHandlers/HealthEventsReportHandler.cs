// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries.HealthMonitoring;
using Columns = HealthEventTable.Columns;

public class HealthEventsReportHandler : DatabaseReportHandler<HealthEventQuery>
{
    public HealthEventsReportHandler(IEngine engine, Role role, SampleDatabaseManager databaseManager) : base(engine, role, databaseManager)
    {
    }

    protected override string TableName => HealthEventTable.Name;

    protected override string SelectColumns =>
        $"{Columns.EventTimestamp}, {Columns.HealthEventId}, {Columns.EventSourceTypeId}, {Columns.SourceEntityGuid}, {Columns.EventDescription}, " +
        $"{Columns.MachineName}, {Columns.SeverityId}, {Columns.ErrorNumber}, {Columns.Occurrence}, {Columns.ObserverEntity}";

    protected override string TimestampColumn => Columns.EventTimestamp;

    protected override Task AddFiltersAsync(ICollection<string> conditions, SqlCommand command, HealthEventQuery query)
    {
        SqlFilterBuilder.AddIntFilter(conditions, Columns.HealthEventId, query.HealthEvents.Select(healthEvent => (int)healthEvent).ToList());
        SqlFilterBuilder.AddIntFilter(conditions, Columns.SeverityId, query.Severities.Select(severity => (int)severity).ToList());
        SqlFilterBuilder.AddIntFilter(conditions, Columns.EventSourceTypeId, query.EventSourceTypes.Select(sourceType => (int)sourceType).ToList());
        SqlFilterBuilder.AddGuidFilter(conditions, command, Columns.SourceEntityGuid, query.Sources, "Source");
        SqlFilterBuilder.AddGuidFilter(conditions, command, Columns.ObserverEntity, query.ObserverEntities, "Observer");
        SqlFilterBuilder.AddStringFilter(conditions, command, Columns.MachineName, query.MachineName, query.MachineNameSearchMode, "@MachineName");

        if (query.OnlyCurrentlyActiveEvents)
        {
            conditions.Add($"{Columns.IsActive} = 1");
        }

        return Task.CompletedTask;
    }

    protected override void AddRow(DataTable table, SqlDataReader reader)
    {
        DataRow row = table.NewRow();
        row[HealthEventQuery.HealthEventIdColumnName] = reader.GetInt32(Columns.HealthEventId);
        row[HealthEventQuery.EventSourceTypeIdColumnName] = reader.GetInt32(Columns.EventSourceTypeId);
        row[HealthEventQuery.SourceEntityGuidColumnName] = reader.GetGuid(Columns.SourceEntityGuid);
        row[HealthEventQuery.EventDescriptionColumnName] = reader.GetString(Columns.EventDescription);
        row[HealthEventQuery.MachineNameColumnName] = reader.GetString(Columns.MachineName);
        row[HealthEventQuery.TimestampColumnName] = reader.GetUtcDateTime(Columns.EventTimestamp);
        row[HealthEventQuery.SeverityIdColumnName] = reader.GetInt32(Columns.SeverityId);
        row[HealthEventQuery.ErrorNumberColumnName] = reader.GetInt32(Columns.ErrorNumber);
        row[HealthEventQuery.OccurrenceColumnName] = reader.GetInt64(Columns.Occurrence);
        row[HealthEventQuery.ObserverEntityColumnName] = reader.GetGuid(Columns.ObserverEntity);
        table.Rows.Add(row);
    }
}
