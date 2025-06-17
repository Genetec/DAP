// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.AccessControl;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries.AccessControl;

public class AccessControlReportHandler : ReportHandler<AccessControlReportQuery, AccessControlReportRecord>
{
    public AccessControlReportHandler(IEngine engine, Role role) : base(engine, role)
    {
    }

    protected override void FillDataRow(DataRow row, AccessControlReportRecord record)
    {
        row[AccessControlReportQuery.TimestampColumnName] = record.Timestamp;
        row[AccessControlReportQuery.EventTypeColumnName] = record.EventType;
        row[AccessControlReportQuery.UnitGuidColumnName] = record.UnitGuid;
        row[AccessControlReportQuery.DeviceGuidColumnName] = record.DeviceGuid;
        row[AccessControlReportQuery.APGuidColumnName] = record.APGuid;
        row[AccessControlReportQuery.SourceGuidColumnName] = record.SourceGuid;
        row[AccessControlReportQuery.CredentialGuidColumnName] = record.CredentialGuid;
        row[AccessControlReportQuery.CardholderGuidColumnName] = record.CardholderGuid;
        row[AccessControlReportQuery.Credential2GuidColumnName] = record.Credential2Guid;
        row[AccessControlReportQuery.TimeZoneColumnName] = record.TimeZone;
        row[AccessControlReportQuery.OccurrencePeriodColumnName] = record.OccurrencePeriod;
        row[AccessControlReportQuery.AccessPointGroupGuidColumnName] = record.AccessPointGroupGuid;
        row[AccessControlReportQuery.CustomEventMessageColumnName] = record.CustomEventMessage;
    }

    protected override async IAsyncEnumerable<AccessControlReportRecord> GetRecordsAsync(AccessControlReportQuery query)
    {
        // TODO: Implement the actual data retrieval logic here.

        // This method should:
        // 1. Parse the AccessControlReportQuery to determine the query parameters
        //    (e.g., time range, included/excluded expansion entities, offline period types, access decision authorities)
        // 2. Use these parameters to fetch relevant records from your data source
        //    (e.g., database, external API)
        // 3. Yield return each AccessControlReportRecord as it's retrieved,
        //    allowing for efficient streaming of large datasets

        // Consider implementing batched database queries or paginated API calls for large datasets
        // to avoid loading all data into memory at once.

        // For now, we're using placeholder code to demonstrate the structure:
        await Task.Yield(); // Simulates asynchronous work (remove in actual implementation)
        yield break; // No records returned in this example (remove in actual implementation)
    }
}
