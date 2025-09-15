// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Queries.LicensePlateManagement;

namespace Genetec.Dap.CodeSamples;

public class LprHitQuerySample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        var hits = await QueryHits(engine);
        DisplayHits(engine, hits);
    }

    private async Task<List<HitRecord>> QueryHits(Engine engine)
    {
        var query = (HitQuery)engine.ReportManager.CreateReportQuery(ReportType.LprHit);
        query.TimeRange.SetTimeRange(DateTime.Now.AddDays(-1), DateTime.Now);
        query.MaximumResultCount = 50;

        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(MapToHitRecord).ToList();

        HitRecord MapToHitRecord(DataRow row) => new()
        {
            HitId = row.Field<Guid>(HitQuery.HitIdColumnName),
            LprRuleId = row.Field<Guid>(HitQuery.LprRuleIdColumnName),
            Timestamp = row.Field<DateTime>(HitQuery.TimestampColumnName),
            TimestampUtc = row.Field<DateTime>(HitQuery.TimestampUtcColumnName),
            TimeZoneId = row.Field<string>(HitQuery.TimeZoneIdColumnName),
            UserActionType = row.Field<int>(HitQuery.UserActionTypeColumnName),
            HitType = (HitType)row.Field<int>(HitQuery.HitTypeColumnName),
            ReasonType = (HitReason)row.Field<int>(HitQuery.ReasonTypeColumnName),
            ExtraInfo = row.Field<string>(HitQuery.ExtraInfoColumnName),
            Read1Id = row.Field<Guid>(HitQuery.Read1IdColumnName),
            Read1Plate = row.Field<string>(HitQuery.Read1PlateColumnName),
            Read1PlateState = row.Field<string>(HitQuery.Read1PlateStateColumnName),
            Read1Timestamp = row.Field<DateTime>(HitQuery.Read1TimestampColumnName),
            Read1TimestampUtc = row.Field<DateTime>(HitQuery.Read1TimestampUtcColumnName),
            Read1UnitId = row.Field<Guid>(HitQuery.Read1UnitIdColumnName),
            Read1PatrollerId = row.Field<Guid>(HitQuery.Read1PatrollerIdColumnName)
        };
    }

    private void DisplayHits(Engine engine, List<HitRecord> hits)
    {
        Console.WriteLine($"Found {hits.Count} LPR hits:");

        foreach (var hit in hits)
        {
            var rule = engine.GetEntity(hit.LprRuleId);
            var unit = engine.GetEntity(hit.Read1UnitId);

            Console.WriteLine($"{hit.TimestampUtc:yyyy-MM-dd HH:mm:ss} - {hit.Read1Plate} ({hit.Read1PlateState})");
            Console.WriteLine($"  Rule: {rule?.Name ?? "Unknown"}");
            Console.WriteLine($"  Unit: {unit?.Name ?? "Unknown"}");
            Console.WriteLine($"  Hit Type: {hit.HitType}, Reason: {hit.ReasonType}");
            Console.WriteLine($"  Extra Info: {hit.ExtraInfo ?? "N/A"}");
            Console.WriteLine();
        }
    }
}