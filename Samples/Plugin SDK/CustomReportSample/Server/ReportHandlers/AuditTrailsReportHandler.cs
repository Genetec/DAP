// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Sdk;
using Sdk.Entities;
using Sdk.Plugin.Queries.Rows.Extensions;
using Sdk.Plugin.Queries.Rows.Trails;
using Sdk.Queries;

public class AuditTrailsReportHandler(IEngine engine, Role role) : ReportHandler<AuditTrailQuery, AuditTrailRow>(engine, role)
{
    protected override async Task ProcessBatch(DataTable table, IAsyncEnumerable<AuditTrailRow> batch)
    {
        await foreach (AuditTrailRow row in batch)
        {
            table.AddIRow(row);
        }
    }

    protected override async IAsyncEnumerable<AuditTrailRow> GetRecordsAsync(AuditTrailQuery query)
    {
        // TODO: Implement the actual data retrieval logic here.

        // This method should:
        // 1. Parse the AuditTrailQuery to determine the query parameters
        //    (e.g., time range, entities, applications, instigator, etc.)
        // 2. Use these parameters to fetch relevant records from your data source
        //    (e.g., database, external API)
        // 3. Yield return each AuditTrailRow as it's retrieved,
        //    allowing for efficient streaming of large datasets

        // Consider implementing batched database queries or paginated API calls for large datasets
        // to avoid loading all data into memory at once.

        // Sample data - in a real implementation, you would fetch this from a database or other data source
        var records = new List<(AuditTrailModificationType ModificationType, AuditFormat Format, string OldValue, string NewValue, string Description, DateTime Timestamp, EntityType EntityType, string EntityName, string InitiatorName)>
        {
            (AuditTrailModificationType.CardholderModified, AuditFormat.EntityPropertyFormatter, "No access", "Level 1 access", "Updated cardholder access level", DateTime.UtcNow.AddDays(-7), EntityType.Cardholder, "John Doe", "admin"),
            (AuditTrailModificationType.CardholderRenamed, AuditFormat.EntityPropertyFormatter, "John Smith", "John Doe", "Corrected cardholder name", DateTime.UtcNow.AddDays(-6), EntityType.Cardholder, "John Doe", "hr_manager"),
            (AuditTrailModificationType.CoverageModified, AuditFormat.EntityPropertyFormatter, "9AM-5PM", "24/7", "Extended camera coverage", DateTime.UtcNow.AddDays(-5), EntityType.Camera, "Main Entrance Cam", "security_manager"),
            (AuditTrailModificationType.CredentialStateModified, AuditFormat.EntityPropertyFormatter, "Active", "Suspended", "Suspended employee badge", DateTime.UtcNow.AddDays(-4), EntityType.Credential, "Badge001", "hr_manager"),
            (AuditTrailModificationType.EntityDeleted, AuditFormat.EntityDeletedFormatter, "", "", "Removed obsolete access rule", DateTime.UtcNow.AddDays(-3), EntityType.AccessRule, "Temporary Contractor Access", "security_admin"),
            (AuditTrailModificationType.EntityRenamed, AuditFormat.EntityPropertyFormatter, "Back Door", "Emergency Exit", "Updated door name for clarity", DateTime.UtcNow.AddDays(-2), EntityType.Door, "Emergency Exit", "facility_manager"),
            (AuditTrailModificationType.MembershipAdded, AuditFormat.EntityPropertyFormatter, "", "Security Team", "Added user to security group", DateTime.UtcNow.AddDays(-1), EntityType.User, "jane.doe@example.com", "admin"),
            (AuditTrailModificationType.MembershipRemoved, AuditFormat.EntityPropertyFormatter, "IT Support", "", "Removed user from IT support group", DateTime.UtcNow.AddHours(-12), EntityType.User, "john.smith@example.com", "it_manager"),
            (AuditTrailModificationType.PrivilegeModified, AuditFormat.EntityPropertyFormatter, "Read", "Read/Write", "Updated user privileges", DateTime.UtcNow.AddHours(-6), EntityType.User, "alice@example.com", "system_admin"),
            (AuditTrailModificationType.RoleUpgraded, AuditFormat.EntityPropertyFormatter, "Operator", "Supervisor", "Promoted user role", DateTime.UtcNow.AddHours(-1), EntityType.User, "bob@example.com", "hr_director")
        };

        // Simulate some async operation (e.g., database access)
        await Task.Delay(100);

        foreach ((AuditTrailModificationType ModificationType, AuditFormat Format, string OldValue, string NewValue, string Description, DateTime Timestamp, EntityType EntityType, string EntityName, string InitiatorName) record in records)
        {
           yield return new AuditTrailRow(Engine)
                .SetAuditAttributes(record.ModificationType, record.Format)
                .SetModification(record.OldValue, record.NewValue, record.Description, record.Timestamp)
                .SetEntity(record.EntityType, record.EntityName)
                .SetInitiator(EntityType.User, record.InitiatorName)
                .SetInitiatorApplication(ApplicationType.SecurityDesk, "Security Desk", Environment.MachineName);
        }
    }
}