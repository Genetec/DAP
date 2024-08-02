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

    public class HealthStatisticsReportHandler : ReportHandler<HealthStatisticsQuery, HealthStatistics>
    {
        public HealthStatisticsReportHandler(IEngine engine, Role role) : base(engine, role)
        {
        }

        protected override void FillDataRow(DataRow row, HealthStatistics record)
        {
            row[HealthStatisticsQuery.FailureCountColumnName] = record.FailureCount;
            row[HealthStatisticsQuery.RtpPacketLossColumnName] = record.RTPPacketLoss;
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

        protected override async IAsyncEnumerable<HealthStatistics> GetRecordsAsync(HealthStatisticsQuery query)
        {
            // TODO: Implement the logic to retrieve the health statistics
            await Task.Yield(); // Simulates asynchronous work (remove this in actual implementation)
            yield break; // No records returned in this example (remove this in actual implementation)
        }
    }
}
