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

public class ZoneActivityReportHandler : DatabaseReportHandler<ZoneActivityQuery, ZoneActivityRecord>
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

    protected override ZoneActivityRecord MapRecord(SqlDataReader reader)
        => new()
        {
            Timestamp = reader.GetUtcDateTime(Columns.EventTimestamp),
            EventType = reader.GetInt32(Columns.EventType),
            EventId = reader.GetInt32(Columns.EventId),
            TimestampLocal = reader.GetDateTime(reader.GetOrdinal(Columns.EventTimestampLocal)),
            TimeZoneId = reader.GetString(Columns.TimeZoneId),
            ZoneId = reader.GetGuid(Columns.ZoneId),
            OfflinePeriod = reader.GetInt32(Columns.OfflinePeriod)
        };

    protected override void FillDataRow(DataRow row, ZoneActivityRecord record)
    {
        row[ZoneActivityQuery.ZoneActivityTimeStampColumnName] = record.Timestamp;
        row[ZoneActivityQuery.ZoneActivityEventTypeColumnName] = record.EventType;
        row[ZoneActivityQuery.ZoneActivityEventIdColumnName] = record.EventId;
        row[ZoneActivityQuery.ZoneActivityTimeStampLocalColumnName] = record.TimestampLocal;
        row[ZoneActivityQuery.ZoneActivityTimeZoneColumnName] = record.TimeZoneId;
        row[ZoneActivityQuery.ZoneActivityZoneIdColumnName] = record.ZoneId;
        row[ZoneActivityQuery.ZoneActivityOccurrencePeriodColumnName] = record.OfflinePeriod;
    }
}
