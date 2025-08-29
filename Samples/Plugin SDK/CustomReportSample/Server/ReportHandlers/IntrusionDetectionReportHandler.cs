// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries.IntrusionDetection;

public class IntrusionDetectionReportHandler : ReportHandler<IntrusionDetectionReportQuery, IntrusionDetectionRecord>
{
    public IntrusionDetectionReportHandler(IEngine engine, Role role) : base(engine, role) { }

    protected override void FillDataRow(DataRow row, IntrusionDetectionRecord record)
    {
        row[IntrusionDetectionReportQuery.TimestampUtcColumnName] = record.Timestamp;
        row[IntrusionDetectionReportQuery.EventTypeColumnName] = record.EventType;
        row[IntrusionDetectionReportQuery.IntrusionUnitIdColumnName] = record.IntrusionUnitId;
        row[IntrusionDetectionReportQuery.IntrusionAreaIdColumnName] = record.IntrusionAreaId;
        row[IntrusionDetectionReportQuery.DeviceIdColumnName] = record.DeviceId;
        row[IntrusionDetectionReportQuery.SourceGuidColumnName] = record.SourceGuid;
        row[IntrusionDetectionReportQuery.OccurrencePeriodColumnName] = record.OccurrencePeriod;
        row[IntrusionDetectionReportQuery.TimeZoneIdColumnName] = record.TimeZoneId;
        row[IntrusionDetectionReportQuery.InitiatorIdColumnName] = record.InitiatorId;
    }

    protected override async IAsyncEnumerable<IntrusionDetectionRecord> GetRecordsAsync(IntrusionDetectionReportQuery query)
    {
        // TODO: Implement the actual data retrieval logic here.

        // This method should:
        // 1. Parse the IntrusionDetectionReportQuery to determine the query parameters
        //    (e.g., time range, excluded/included expansion entities, event types)
        // 2. Use these parameters to fetch relevant records from your data source
        //    (e.g., database, external API)
        // 3. Yield return each IntrusionDetectionRecord as it's retrieved,
        //    allowing for efficient streaming of large datasets

        // Consider implementing batched database queries or paginated API calls for large datasets
        // to avoid loading all data into memory at once.

        // For now, we're using placeholder code to demonstrate the structure:
        await Task.Yield(); // Simulates asynchronous work (remove in actual implementation)
        yield break; // No records returned in this example (remove in actual implementation)
    }
}