// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.Intrusion;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries.IntrusionDetection;
using Columns = IntrusionEventTable.Columns;

public class IntrusionDetectionReportHandler : DatabaseReportHandler<IntrusionDetectionReportQuery>
{
    public IntrusionDetectionReportHandler(IEngine engine, Role role, SampleDatabaseManager databaseManager) : base(engine, role, databaseManager)
    {
    }

    protected override string TableName => IntrusionEventTable.Name;

    protected override string SelectColumns =>
        $"{Columns.EventTimestamp}, {Columns.EventType}, {Columns.IntrusionUnitId}, {Columns.IntrusionAreaId}, {Columns.DeviceId}, " +
        $"{Columns.SourceGuid}, {Columns.OccurrencePeriod}, {Columns.TimeZoneId}, {Columns.InitiatorId}";

    protected override string TimestampColumn => Columns.EventTimestamp;

    protected override async Task AddFiltersAsync(ICollection<string> conditions, SqlCommand command, IntrusionDetectionReportQuery query)
    {
        SqlFilterBuilder.AddEventTypeFilter(conditions, Columns.EventType, query);

        // Entity filter: a record matches when any of its entity columns is a selected entity.
        // This handler serves both the intrusion area and the intrusion unit activity reports.
        if (query.QueryEntities.Count > 0)
        {
            QueryEntitySelection selection = await QueryEntityExpander.ExpandIntrusionSelectionAsync(
                Engine,
                query.QueryEntities,
                query.ExcludedExpansionEntities);

            if (selection.IsUnrestricted)
            {
                if (selection.Excluded.Count > 0)
                {
                    string excluded = SqlFilterBuilder.AddGuidList(command, selection.Excluded, "ExcludedEntity");
                    conditions.Add($"({Columns.IntrusionAreaId} NOT IN ({excluded}) AND {Columns.IntrusionUnitId} NOT IN ({excluded}) AND {Columns.SourceGuid} NOT IN ({excluded}))");
                }
            }
            else if (selection.Included.Count > 0)
            {
                string parameterNames = SqlFilterBuilder.AddGuidList(command, selection.Included, "Entity");
                conditions.Add($"({Columns.IntrusionAreaId} IN ({parameterNames}) OR {Columns.IntrusionUnitId} IN ({parameterNames}) OR {Columns.SourceGuid} IN ({parameterNames}))");
            }
            else
            {
                conditions.Add("1 = 0");
            }
        }
    }

    protected override void AddRow(DataTable table, SqlDataReader reader)
    {
        DataRow row = table.NewRow();
        row[IntrusionDetectionReportQuery.TimestampUtcColumnName] = reader.GetUtcDateTime(Columns.EventTimestamp);
        row[IntrusionDetectionReportQuery.EventTypeColumnName] = reader.GetInt32(Columns.EventType);
        row[IntrusionDetectionReportQuery.IntrusionUnitIdColumnName] = reader.GetGuid(Columns.IntrusionUnitId);
        row[IntrusionDetectionReportQuery.IntrusionAreaIdColumnName] = reader.GetGuid(Columns.IntrusionAreaId);
        row[IntrusionDetectionReportQuery.DeviceIdColumnName] = reader.GetGuid(Columns.DeviceId);
        row[IntrusionDetectionReportQuery.SourceGuidColumnName] = reader.GetGuid(Columns.SourceGuid);
        row[IntrusionDetectionReportQuery.OccurrencePeriodColumnName] = reader.GetInt32(Columns.OccurrencePeriod);
        row[IntrusionDetectionReportQuery.TimeZoneIdColumnName] = reader.GetString(Columns.TimeZoneId);
        row[IntrusionDetectionReportQuery.InitiatorIdColumnName] = reader.GetGuid(Columns.InitiatorId);
        table.Rows.Add(row);
    }
}
