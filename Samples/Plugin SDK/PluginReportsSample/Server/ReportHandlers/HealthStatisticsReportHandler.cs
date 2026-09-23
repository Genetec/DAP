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

public class HealthStatisticsReportHandler : DatabaseReportHandler<HealthStatisticsQuery, HealthStatistics>
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

    protected override HealthStatistics MapRecord(SqlDataReader reader)
        => new()
        {
            SourceEntityGuid = reader.GetGuid(Columns.SourceEntityGuid),
            EventSourceType = reader.GetInt32(Columns.EventSourceType),
            FailureCount = reader.GetInt32(Columns.FailureCount),
            RtpPacketLoss = reader.GetInt32(Columns.RtpPacketLoss),
            CalculationStatus = reader.GetInt32(Columns.CalculationStatus),
            UnexpectedDowntime = TimeSpan.FromTicks(reader.GetInt64(Columns.UnexpectedDowntimeTicks)),
            ExpectedDowntime = TimeSpan.FromTicks(reader.GetInt64(Columns.ExpectedDowntimeTicks)),
            Uptime = TimeSpan.FromTicks(reader.GetInt64(Columns.UptimeTicks)),
            Mttr = reader.GetFloat(Columns.Mttr),
            Mtbf = reader.GetFloat(Columns.Mtbf),
            Availability = reader.GetFloat(Columns.Availability),
            LastErrorTimestamp = reader.GetUtcDateTime(Columns.LastErrorTimestamp),
            ObserverEntity = reader.GetGuid(Columns.ObserverEntity)
        };

    protected override void FillDataRow(DataRow row, HealthStatistics record)
    {
        row[HealthStatisticsQuery.FailureCountColumnName] = record.FailureCount;
        row[HealthStatisticsQuery.RtpPacketLossColumnName] = record.RtpPacketLoss;
        row[HealthStatisticsQuery.CalculationStatusColumnName] = record.CalculationStatus;
        row[HealthStatisticsQuery.SourceEntityGuidColumnName] = record.SourceEntityGuid;
        row[HealthStatisticsQuery.EventSourceTypeColumnName] = record.EventSourceType;
        row[HealthStatisticsQuery.UnexpectedDowntimeColumnName] = record.UnexpectedDowntime;
        row[HealthStatisticsQuery.ExpectedDowntimeColumnName] = record.ExpectedDowntime;
        row[HealthStatisticsQuery.UptimeColumnName] = record.Uptime;
        row[HealthStatisticsQuery.MttrColumnName] = record.Mttr;
        row[HealthStatisticsQuery.MtbfColumnName] = record.Mtbf;
        row[HealthStatisticsQuery.AvailabilityColumnName] = record.Availability;
        row[HealthStatisticsQuery.LastErrorTimestampColumnName] = record.LastErrorTimestamp;
        row[HealthStatisticsQuery.ObserverEntityColumnName] = record.ObserverEntity;
    }
}
