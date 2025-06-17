// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

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

public class ActivityTrailsReportHandler : ReportHandler<ActivityTrailsQuery, ActivityTrailRow>
{
    public ActivityTrailsReportHandler(IEngine engine, Role role) : base(engine, role)
    {
    }

    protected override async Task ProcessBatch(DataTable table, IAsyncEnumerable<ActivityTrailRow> batch)
    {
        await foreach (ActivityTrailRow row in batch)
        {
            table.AddIRow(row);
        }
    }

    protected override async IAsyncEnumerable<ActivityTrailRow> GetRecordsAsync(ActivityTrailsQuery query)
    {
        // TODO: Implement the actual data retrieval logic here.

        // This method should:
        // 1. Parse the ActivityTrailsQuery to determine the query parameters
        //    (e.g., time range, event types, etc.)
        // 2. Use these parameters to fetch relevant records from your data source
        //    (e.g., database, external API)
        // 3. Yield return each ActivityTrailRow as it's retrieved,
        //    allowing for efficient streaming of large datasets

        // Consider implementing batched database queries or paginated API calls for large datasets
        // to avoid loading all data into memory at once.

        // Sample data - in a real implementation, you would fetch this from a database or other data source
        var activities = new List<(ActivityType Type, string Description, DateTime Timestamp, EntityType EntityType, string EntityName, string InitiatorName)>
        {
            (ActivityType.UserLogonFailed, "Failed login attempt for user 'jsmith'", DateTime.UtcNow.AddHours(-2), EntityType.User, "jsmith", "jsmith"),
            (ActivityType.UserLoggedOn, "User 'jsmith' logged on successfully", DateTime.UtcNow.AddHours(-1.9), EntityType.User, "jsmith", "jsmith"),
            (ActivityType.PtzCommands, "PTZ commands executed on 'Entrance Cam'", DateTime.UtcNow.AddHours(-1.7), EntityType.Camera, "Entrance Cam", "operator1"),
            (ActivityType.AlarmTriggered, "Motion detected on 'Entrance Cam'", DateTime.UtcNow.AddHours(-1.6), EntityType.Camera, "Entrance Cam", "system"),
            (ActivityType.AlarmAcknowledged, "Alarm acknowledged by operator", DateTime.UtcNow.AddHours(-1.5), EntityType.Alarm, "Motion Alarm", "operator2"),
            (ActivityType.VideoExport, "Video exported from 'Entrance Cam'", DateTime.UtcNow.AddHours(-1.4), EntityType.Camera, "Entrance Cam", "investigator1"),
            (ActivityType.DoorManuallyUnlocked, "Main entrance door manually unlocked", DateTime.UtcNow.AddHours(-1.2), EntityType.Door, "Main Entrance", "guard1"),
            (ActivityType.ArchiveBackupStarted, "Started backup of video archives", DateTime.UtcNow.AddHours(-1.1), EntityType.Role, "Main Archiver", "system"),
            (ActivityType.UserLoggedOff, "User 'jsmith' logged off", DateTime.UtcNow.AddHours(-1), EntityType.User, "jsmith", "jsmith")
        };

        // Simulate some async operation (e.g., database access)
        await Task.Delay(100);

        foreach ((ActivityType Type, string Description, DateTime Timestamp, EntityType EntityType, string EntityName, string InitiatorName) activity in activities)
        {
            yield return new ActivityTrailRow(Engine)
                .SetActivity(activity.Type, activity.Description, activity.Timestamp)
                .SetEntity(activity.EntityType, activity.EntityName)
                .SetInitiator(EntityType.User, activity.InitiatorName)
                .SetInitiatorApplication(ApplicationType.SecurityDesk, "Security Desk", Environment.MachineName);
        }
    }
}
