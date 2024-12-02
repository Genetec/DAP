// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Queries.HealthMonitoring;

class HealthEventsReportHandler(IEngine engine, Role role) : ReportHandler<HealthEventQuery, HealthEvent>(engine, role)
{
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

    protected override async IAsyncEnumerable<HealthEvent> GetRecordsAsync(HealthEventQuery query)
    {
        // TODO: Implement the actual data retrieval logic here.

        // This method should:
        // 1. Parse the HealthEventQuery to determine the query parameters
        //    (e.g., time range, event source types, health event types, severities, sources)
        // 2. Use these parameters to fetch relevant records from your data source
        //    (e.g., database, external API)
        // 3. Yield return each HealthEvent as it's retrieved,
        //    allowing for efficient streaming of large datasets

        // Consider implementing batched database queries or paginated API calls for large datasets
        // to avoid loading all data into memory at once.

        // For now, we're using placeholder code to demonstrate the structure:
        await Task.Yield(); // Simulates asynchronous work (remove in actual implementation)
        yield break; // No records returned in this example (remove in actual implementation)
    }
}