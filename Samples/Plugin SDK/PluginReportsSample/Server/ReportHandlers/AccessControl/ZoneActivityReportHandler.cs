// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.AccessControl;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries;
using Columns = ZoneActivityTable.Columns;

public class ZoneActivityReportHandler : DatabaseReportHandler<ZoneActivityQuery>
{
    public ZoneActivityReportHandler(IEngine engine, Role role, SampleDatabaseManager databaseManager) : base(engine, role, databaseManager)
    {
    }

    protected override string TableName => ZoneActivityTable.Name;

    protected override string SelectColumns =>
        $"{Columns.EventTimestamp}, {Columns.EventType}, {Columns.EventId}, {Columns.EventTimestampLocal}, {Columns.TimeZoneId}, {Columns.ZoneId}, {Columns.OfflinePeriod}";

    protected override string TimestampColumn => Columns.EventTimestamp;

    protected override async Task AddFiltersAsync(ICollection<string> conditions, SqlCommand command, ZoneActivityQuery query)
    {
        SqlFilterBuilder.AddEventTypeFilter(conditions, Columns.EventType, query);

        if (query.Zones.Count > 0 || query.IncludedExpansionEntities.Count > 0)
        {
            QueryEntitySelection selection = await QueryEntityExpander.ExpandZoneSelectionAsync(
                Engine,
                query.Zones,
                query.IncludedExpansionEntities,
                query.ExcludedExpansionEntities);

            if (selection.IsUnrestricted)
            {
                if (selection.Excluded.Count > 0)
                {
                    string excluded = SqlFilterBuilder.AddGuidList(command, selection.Excluded, "ExcludedZone");
                    conditions.Add($"{Columns.ZoneId} NOT IN ({excluded})");
                }
            }
            else if (selection.Included.Count > 0)
            {
                SqlFilterBuilder.AddGuidFilter(conditions, command, Columns.ZoneId, selection.Included, "Zone");
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
        row[ZoneActivityQuery.ZoneActivityTimeStampColumnName] = reader.GetUtcDateTime(Columns.EventTimestamp);
        row[ZoneActivityQuery.ZoneActivityEventTypeColumnName] = reader.GetInt32(Columns.EventType);
        row[ZoneActivityQuery.ZoneActivityEventIdColumnName] = reader.GetInt32(Columns.EventId);
        row[ZoneActivityQuery.ZoneActivityTimeStampLocalColumnName] = reader.GetDateTime(reader.GetOrdinal(Columns.EventTimestampLocal));
        row[ZoneActivityQuery.ZoneActivityTimeZoneColumnName] = reader.GetString(Columns.TimeZoneId);
        row[ZoneActivityQuery.ZoneActivityZoneIdColumnName] = reader.GetGuid(Columns.ZoneId);
        row[ZoneActivityQuery.ZoneActivityOccurrencePeriodColumnName] = reader.GetInt32(Columns.OfflinePeriod);
        table.Rows.Add(row);
    }
}
