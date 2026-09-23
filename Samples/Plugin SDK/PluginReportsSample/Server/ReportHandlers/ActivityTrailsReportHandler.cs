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
using Columns = ActivityTrailTable.Columns;

public class ActivityTrailsReportHandler : DatabaseReportHandler<ActivityTrailsQuery, ActivityTrailRow>
{
    public ActivityTrailsReportHandler(IEngine engine, Role role, SampleDatabaseManager databaseManager) : base(engine, role, databaseManager)
    {
    }

    protected override string TableName => ActivityTrailTable.Name;

    protected override string SelectColumns =>
        $"{Columns.EventTimestamp}, {Columns.ActivityType}, {Columns.Description}, {Columns.EntityGuid}, {Columns.EntityType}, {Columns.EntityName}, " +
        $"{Columns.InitiatorGuid}, {Columns.InitiatorType}, {Columns.InitiatorName}, {Columns.ApplicationType}, {Columns.ApplicationName}, {Columns.MachineName}";

    protected override string TimestampColumn => Columns.EventTimestamp;

    protected override Task AddFiltersAsync(ICollection<string> conditions, SqlCommand command, ActivityTrailsQuery query)
    {
        SqlFilterBuilder.AddIntFilter(conditions, Columns.ActivityType, query.Activities.Select(activity => (int)activity).ToList());
        SqlFilterBuilder.AddIntFilter(conditions, Columns.ApplicationType, query.Applications.Select(application => (int)application).ToList());
        SqlFilterBuilder.AddStringFilter(conditions, command, Columns.Description, query.Description, StringSearchMode.Contains, "@Description");
        SqlFilterBuilder.AddGuidFilter(conditions, command, Columns.EntityGuid, query.ImpactedEntities, "ImpactedEntity");
        SqlFilterBuilder.AddGuidFilter(conditions, command, Columns.InitiatorGuid, query.InitiatorEntities, "Initiator");
        SqlFilterBuilder.AddIntFilter(conditions, Columns.EntityType, query.ImpactedEntityTypes.Select(entityType => (int)entityType).ToList());
        SqlFilterBuilder.AddIntFilter(conditions, Columns.InitiatorType, query.InitiatorEntityTypes.Select(entityType => (int)entityType).ToList());
        return Task.CompletedTask;
    }

    protected override ActivityTrailRow MapRecord(SqlDataReader reader)
    {
        var row = new ActivityTrailRow(Engine)
            .SetActivity((ActivityType)reader.GetInt32(Columns.ActivityType), reader.GetString(Columns.Description), reader.GetUtcDateTime(Columns.EventTimestamp))
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
