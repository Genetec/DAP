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

public class HealthEventsReportHandler : DatabaseReportHandler<HealthEventQuery, HealthEvent>
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

    protected override HealthEvent MapRecord(SqlDataReader reader)
        => new()
        {
            Timestamp = reader.GetUtcDateTime(Columns.EventTimestamp),
            HealthEventId = reader.GetInt32(Columns.HealthEventId),
            EventSourceTypeId = reader.GetInt32(Columns.EventSourceTypeId),
            SourceEntityGuid = reader.GetGuid(Columns.SourceEntityGuid),
            EventDescription = reader.GetString(Columns.EventDescription),
            MachineName = reader.GetString(Columns.MachineName),
            SeverityId = reader.GetInt32(Columns.SeverityId),
            ErrorNumber = reader.GetInt32(Columns.ErrorNumber),
            Occurrence = reader.GetInt64(Columns.Occurrence),
            ObserverEntity = reader.GetGuid(Columns.ObserverEntity)
        };

    protected override void FillDataRow(DataRow row, HealthEvent record)
    {
        row[HealthEventQuery.HealthEventIdColumnName] = record.HealthEventId;
        row[HealthEventQuery.EventSourceTypeIdColumnName] = record.EventSourceTypeId;
        row[HealthEventQuery.SourceEntityGuidColumnName] = record.SourceEntityGuid;
        row[HealthEventQuery.EventDescriptionColumnName] = record.EventDescription;
        row[HealthEventQuery.MachineNameColumnName] = record.MachineName;
        row[HealthEventQuery.TimestampColumnName] = record.Timestamp;
        row[HealthEventQuery.SeverityIdColumnName] = record.SeverityId;
        row[HealthEventQuery.ErrorNumberColumnName] = record.ErrorNumber;
        row[HealthEventQuery.OccurrenceColumnName] = record.Occurrence;
        row[HealthEventQuery.ObserverEntityColumnName] = record.ObserverEntity;
    }
}
