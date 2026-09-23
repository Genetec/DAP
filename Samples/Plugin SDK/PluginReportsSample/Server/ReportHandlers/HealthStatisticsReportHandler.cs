// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries.HealthMonitoring;
using Columns = HealthStatisticsTable.Columns;

public class HealthStatisticsReportHandler : DatabaseReportHandler<HealthStatisticsQuery>
{
    public HealthStatisticsReportHandler(IEngine engine, Role role, SampleDatabaseManager databaseManager) : base(engine, role, databaseManager)
    {
    }

    protected override string TableName => HealthStatisticsTable.Name;

    protected override string SelectColumns =>
        $"{Columns.SourceEntityGuid}, {Columns.EventSourceType}, {Columns.FailureCount}, {Columns.RtpPacketLoss}, {Columns.CalculationStatus}, " +
        $"{Columns.UnexpectedDowntimeTicks}, {Columns.ExpectedDowntimeTicks}, {Columns.UptimeTicks}, {Columns.Mttr}, {Columns.Mtbf}, " +
        $"{Columns.Availability}, {Columns.LastErrorTimestamp}, {Columns.ObserverEntity}";

    // Health statistics are aggregates rather than time-stamped events,
    // so the rows are not filtered by the query's time range.
    protected override string TimestampColumn => null;

    protected override Task AddFiltersAsync(ICollection<string> conditions, SqlCommand command, HealthStatisticsQuery query)
    {
        SqlFilterBuilder.AddGuidFilter(conditions, command, Columns.SourceEntityGuid, query.Sources, "Source");
        SqlFilterBuilder.AddGuidFilter(conditions, command, Columns.ObserverEntity, query.ObserverEntities, "Observer");
        return Task.CompletedTask;
    }

    protected override void AddRow(DataTable table, SqlDataReader reader)
    {
        DataRow row = table.NewRow();
        row[HealthStatisticsQuery.FailureCountColumnName] = reader.GetInt32(Columns.FailureCount);
        row[HealthStatisticsQuery.RtpPacketLossColumnName] = reader.GetInt32(Columns.RtpPacketLoss);
        row[HealthStatisticsQuery.CalculationStatusColumnName] = reader.GetInt32(Columns.CalculationStatus);
        row[HealthStatisticsQuery.SourceEntityGuidColumnName] = reader.GetGuid(Columns.SourceEntityGuid);
        row[HealthStatisticsQuery.EventSourceTypeColumnName] = reader.GetInt32(Columns.EventSourceType);
        row[HealthStatisticsQuery.UnexpectedDowntimeColumnName] = TimeSpan.FromTicks(reader.GetInt64(Columns.UnexpectedDowntimeTicks));
        row[HealthStatisticsQuery.ExpectedDowntimeColumnName] = TimeSpan.FromTicks(reader.GetInt64(Columns.ExpectedDowntimeTicks));
        row[HealthStatisticsQuery.UptimeColumnName] = TimeSpan.FromTicks(reader.GetInt64(Columns.UptimeTicks));
        row[HealthStatisticsQuery.MttrColumnName] = reader.GetFloat(Columns.Mttr);
        row[HealthStatisticsQuery.MtbfColumnName] = reader.GetFloat(Columns.Mtbf);
        row[HealthStatisticsQuery.AvailabilityColumnName] = reader.GetFloat(Columns.Availability);
        row[HealthStatisticsQuery.LastErrorTimestampColumnName] = reader.GetUtcDateTime(Columns.LastErrorTimestamp);
        row[HealthStatisticsQuery.ObserverEntityColumnName] = reader.GetGuid(Columns.ObserverEntity);
        table.Rows.Add(row);
    }
}
