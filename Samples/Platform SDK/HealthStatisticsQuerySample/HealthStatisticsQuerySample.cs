// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Queries.HealthMonitoring;

namespace Genetec.Dap.CodeSamples;

public class HealthStatisticsQuerySample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        List<HealthStatisticsRecord> statistics = await QueryHealthStatistics(engine);
        DisplayHealthStatistics(engine, statistics);
    }

    private async Task<List<HealthStatisticsRecord>> QueryHealthStatistics(Engine engine)
    {
        var query = (HealthStatisticsQuery)engine.ReportManager.CreateReportQuery(ReportType.HealthStatistics);
        query.TimeRange.SetTimeRange(DateTime.Now.AddDays(-7), DateTime.Now);
        query.MaximumResultCount = 50;

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(MapToHealthStatistics).ToList();

        HealthStatisticsRecord MapToHealthStatistics(DataRow row) => new()
        {
            SourceEntityGuid = row.Field<Guid>(HealthStatisticsQuery.SourceEntityGuidColumnName),
            EventSourceType = (EventSourceType)row.Field<int>(HealthStatisticsQuery.EventSourceTypeColumnName),
            ObserverEntity = row.Field<Guid>(HealthStatisticsQuery.ObserverEntityColumnName),
            Availability = row.Field<float>(HealthStatisticsQuery.AvailabilityColumnName),
            Uptime = row.Field<TimeSpan>(HealthStatisticsQuery.UptimeColumnName),
            ExpectedDowntime = row.Field<TimeSpan>(HealthStatisticsQuery.ExpectedDowntimeColumnName),
            UnexpectedDowntime = row.Field<TimeSpan>(HealthStatisticsQuery.UnexpectedDowntimeColumnName),
            Mtbf = row.Field<float>(HealthStatisticsQuery.MtbfColumnName),
            Mttr = row.Field<float>(HealthStatisticsQuery.MttrColumnName),
            FailureCount = row.Field<int>(HealthStatisticsQuery.FailureCountColumnName),
            RtpPacketLoss = row.Field<int>(HealthStatisticsQuery.RtpPacketLossColumnName),
            CalculationStatus = (AvailabilityCalculationStatus)row.Field<int>(HealthStatisticsQuery.CalculationStatusColumnName),
            LastErrorTimestamp = row.Field<DateTime>(HealthStatisticsQuery.LastErrorTimestampColumnName),
            LastSeenOnline = row.Field<DateTime>(HealthStatisticsQuery.LastSeenOnlineColumnName)
        };
    }

    private void DisplayHealthStatistics(Engine engine, List<HealthStatisticsRecord> statistics)
    {
        if (!statistics.Any())
        {
            Console.WriteLine("No health statistics found.");
            return;
        }

        Console.WriteLine("Health Statistics:");
        Console.WriteLine(new string('-', 150));
        Console.WriteLine($"{"Source entity",-40} {"Availability",-14} {"Uptime",-18} {"Expected downtime",-19} {"Unexpected downtime",-21} {"MTBF",-12} {"MTTR",-12} {"Failures",-10}");
        Console.WriteLine(new string('-', 150));

        foreach (HealthStatisticsRecord record in statistics)
        {
            string availability = Format(record, record.Availability.ToString("P", CultureInfo.CurrentCulture));
            string uptime = Format(record, record.Uptime.ToString());
            string expectedDowntime = Format(record, record.ExpectedDowntime.ToString());
            string unexpectedDowntime = Format(record, record.UnexpectedDowntime.ToString());
            string mtbf = Format(record, $"{record.Mtbf.ToString("F", CultureInfo.CurrentCulture)} h");
            string mttr = Format(record, $"{record.Mttr.ToString("F", CultureInfo.CurrentCulture)} h");

            Console.WriteLine($"{GetEntityName(record.SourceEntityGuid),-40} {availability,-14} {uptime,-18} {expectedDowntime,-19} {unexpectedDowntime,-21} {mtbf,-12} {mttr,-12} {record.FailureCount,-10}");
        }

        Console.WriteLine(new string('-', 150));
        Console.WriteLine($"Total sources: {statistics.Count}");

        string Format(HealthStatisticsRecord record, string value) => record.CalculationStatus == AvailabilityCalculationStatus.CalculationAvailable ? value : "N/A";

        string GetEntityName(Guid guid) => engine.GetEntity(guid)?.Name ?? "Unknown Entity";
    }
}
