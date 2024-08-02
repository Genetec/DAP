// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Genetec.Sdk;
    using Genetec.Sdk.Entities;
    using Genetec.Sdk.Queries.HealthMonitoring;

    public class HealthEventsReportHandler : ReportHandler<HealthEventQuery, HealthEvent>
    {
        public HealthEventsReportHandler(IEngine engine, Role role) : base(engine, role)
        {
        }

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
            // TODO: Implement the logic to retrieve the health events
            await Task.Yield(); // Simulates asynchronous work (remove this in actual implementation)
            yield break; // No records returned in this example (remove this in actual implementation)
        }
    }
}
