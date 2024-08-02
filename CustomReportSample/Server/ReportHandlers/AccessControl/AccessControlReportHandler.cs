// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers.AccessControl
{
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
            // For now, we're using placeholder code to demonstrate the structure:
            await Task.Yield(); // Simulates asynchronous work (remove in actual implementation)
            yield break; // No records returned in this example (remove in actual implementation)
        }
    }
}

