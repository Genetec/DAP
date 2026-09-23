// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

using Microsoft.Data.SqlClient;
using Sdk;
using Sdk.Entities;
using Sdk.Plugin.Queries.Rows.Trails;
using Sdk.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Columns = AuditTrailTable.Columns;

public class AuditTrailsReportHandler : DatabaseReportHandler<AuditTrailQuery, AuditTrailRow>
{
    public AuditTrailsReportHandler(IEngine engine, Role role, SampleDatabaseManager databaseManager) : base(engine, role, databaseManager)
    {
    }

    protected override string TableName => AuditTrailTable.Name;

    protected override string SelectColumns =>
        $"{Columns.EventTimestamp}, {Columns.ModificationType}, {Columns.AuditFormat}, {Columns.OldValue}, {Columns.NewValue}, {Columns.Description}, " +
        $"{Columns.EntityGuid}, {Columns.EntityType}, {Columns.EntityName}, {Columns.InitiatorGuid}, {Columns.InitiatorType}, {Columns.InitiatorName}, " +
        $"{Columns.ApplicationType}, {Columns.ApplicationName}, {Columns.MachineName}";

    protected override string TimestampColumn => Columns.EventTimestamp;

    protected override Task AddFiltersAsync(ICollection<string> conditions, SqlCommand command, AuditTrailQuery query)
    {
        SqlFilterBuilder.AddIntFilter(conditions, Columns.EntityType, query.EntityTypes.Select(entityType => (int)entityType).ToList());
        SqlFilterBuilder.AddIntFilter(conditions, Columns.ApplicationType, query.Applications.Select(application => (int)application).ToList());
        SqlFilterBuilder.AddGuidFilter(conditions, command, Columns.EntityGuid, query.QueryEntities, "Entity");
        SqlFilterBuilder.AddGuidFilter(conditions, command, Columns.InitiatorGuid, query.Users, "User");
        return Task.CompletedTask;
    }

    protected override AuditTrailRow MapRecord(SqlDataReader reader)
    {
        var row = new AuditTrailRow(Engine)
            .SetAuditAttributes((AuditTrailModificationType)reader.GetInt32(Columns.ModificationType), (AuditFormat)reader.GetInt32(Columns.AuditFormat))
            .SetModification(reader.GetString(Columns.OldValue), reader.GetString(Columns.NewValue), reader.GetString(Columns.Description), reader.GetUtcDateTime(Columns.EventTimestamp))
            .SetEntity((EntityType)reader.GetInt32(Columns.EntityType), reader.GetString(Columns.EntityName))
            .SetInitiator((EntityType)reader.GetInt32(Columns.InitiatorType), reader.GetString(Columns.InitiatorName))
            .SetInitiatorApplication((ApplicationType)reader.GetInt32(Columns.ApplicationType), reader.GetString(Columns.ApplicationName), reader.GetString(Columns.MachineName));

        Guid entityGuid = reader.GetGuidOrDefault(Columns.EntityGuid);
        if (entityGuid != Guid.Empty && Engine.GetEntity(entityGuid) is not null)
        {
            row.SetEntity(entityGuid);
        }

        Guid initiatorGuid = reader.GetGuidOrDefault(Columns.InitiatorGuid);
        if (initiatorGuid != Guid.Empty && Engine.GetEntity(initiatorGuid) is not null)
        {
            row.SetInitiator(initiatorGuid);
        }

        return row;
    }
}
