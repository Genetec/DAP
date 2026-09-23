// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Dap.CodeSamples.Ingestion;
using Genetec.Dap.CodeSamples.Properties;
using Genetec.Dap.CodeSamples.Server.ReportHandlers;
using Microsoft.Data.SqlClient;
using Sdk.Plugin;
using Sdk.Plugin.Objects;

/// <summary>
/// Manages the plugin database that stores the ingested events. It creates the schema, exposes
/// one insert method per report domain, and offers a per-table cleanup threshold to administrators.
/// </summary>
public class SampleDatabaseManager : DatabaseManager
{
    // One cleanup threshold per time-stamped event table. HealthStatistics is excluded because it
    // holds aggregate rows, not an ever-growing event log; audit trails default to no cleanup.
    private static readonly Dictionary<string, (string Title, string TimestampColumn, bool DefaultIsEnabled, int DefaultRetentionPeriod)> s_cleanupThresholds = new()
    {
        [ActivityTrailTable.Name] = ("Keep activity trails", ActivityTrailTable.Columns.EventTimestamp, true, 90),
        [AuditTrailTable.Name] = ("Keep audit trails", AuditTrailTable.Columns.EventTimestamp, false, 0),
        [AccessControlEventTable.Name] = ("Keep access control events", AccessControlEventTable.Columns.EventTimestamp, true, 90),
        [ZoneActivityTable.Name] = ("Keep zone activities", ZoneActivityTable.Columns.EventTimestamp, true, 90),
        [IntrusionEventTable.Name] = ("Keep intrusion events", IntrusionEventTable.Columns.EventTimestamp, true, 90),
        [VideoEventTable.Name] = ("Keep video events", VideoEventTable.Columns.EventTime, true, 90),
        [HealthEventTable.Name] = ("Keep health events", HealthEventTable.Columns.EventTimestamp, true, 90),
        ["CustomEvents"] = ("Keep custom events", "EventTimestamp", true, 90)
    };

    /// <summary>Gets the current database configuration, used to create SQL connections.</summary>
    public DatabaseConfiguration Configuration { get; private set; }

    /// <summary>Gets the current state of the database.</summary>
    public DatabaseState State { get; private set; }

    public override string GetSpecificCreationScript(string databaseName) => Resources.CreationScript;

    public override void SetDatabaseInformation(DatabaseConfiguration databaseConfiguration) => Configuration = databaseConfiguration;

    public override void OnDatabaseStateChanged(DatabaseNotification notification) => State = notification.State;

    public async Task InsertCustomEventAsync(CustomEventIngestion e, DateTime timestamp, CancellationToken cancellationToken)
    {
        using SqlConnection connection = Configuration.CreateSqlDatabaseConnection();
        using var command = new SqlCommand("dbo.InsertCustomEvent", connection) { CommandType = CommandType.StoredProcedure };

        command.Parameters.Add("@EventTimestamp", SqlDbType.DateTime2).Value = timestamp;
        command.Parameters.Add("@CustomEventId", SqlDbType.Int).Value = e.CustomEventId.Value;
        command.Parameters.Add("@SourceGuid", SqlDbType.UniqueIdentifier).Value = e.Source.Value;
        command.Parameters.Add("@Message", SqlDbType.NVarChar, -1).Value = e.Message;

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task InsertAccessControlEventAsync(AccessControlEventIngestion e, int eventType, DateTime timestamp, CancellationToken cancellationToken)
    {
        using SqlConnection connection = Configuration.CreateSqlDatabaseConnection();
        using var command = new SqlCommand("dbo.InsertAccessControlEvent", connection) { CommandType = CommandType.StoredProcedure };

        command.Parameters.Add("@EventTimestamp", SqlDbType.DateTime2).Value = timestamp;
        command.Parameters.Add("@EventType", SqlDbType.Int).Value = eventType;
        command.Parameters.Add("@SourceGuid", SqlDbType.UniqueIdentifier).Value = e.Source ?? Guid.Empty;
        command.Parameters.Add("@CardholderGuid", SqlDbType.UniqueIdentifier).Value = (object)e.Cardholder ?? DBNull.Value;
        command.Parameters.Add("@CredentialGuid", SqlDbType.UniqueIdentifier).Value = (object)e.Credential ?? DBNull.Value;
        command.Parameters.Add("@UnitGuid", SqlDbType.UniqueIdentifier).Value = (object)e.Unit ?? DBNull.Value;
        command.Parameters.Add("@DeviceGuid", SqlDbType.UniqueIdentifier).Value = (object)e.Device ?? DBNull.Value;
        command.Parameters.Add("@APGuid", SqlDbType.UniqueIdentifier).Value = (object)e.AccessPoint ?? DBNull.Value;
        command.Parameters.Add("@Credential2Guid", SqlDbType.UniqueIdentifier).Value = (object)e.Credential2 ?? DBNull.Value;
        command.Parameters.Add("@AccessPointGroupGuid", SqlDbType.UniqueIdentifier).Value = (object)e.AccessPointGroup ?? DBNull.Value;
        command.Parameters.Add("@TimeZone", SqlDbType.NVarChar, 128).Value = string.IsNullOrEmpty(e.TimeZone) ? "UTC" : e.TimeZone;
        command.Parameters.Add("@OccurrencePeriod", SqlDbType.Int).Value = e.OccurrencePeriod ?? 0;
        command.Parameters.Add("@CustomEventMessage", SqlDbType.NVarChar, -1).Value = (object)e.CustomEventMessage ?? DBNull.Value;

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task InsertZoneActivityAsync(ZoneActivityIngestion e, int eventType, DateTime timestamp, DateTime localTimestamp, CancellationToken cancellationToken)
    {
        using SqlConnection connection = Configuration.CreateSqlDatabaseConnection();
        using var command = new SqlCommand("dbo.InsertZoneActivity", connection) { CommandType = CommandType.StoredProcedure };

        command.Parameters.Add("@EventTimestamp", SqlDbType.DateTime2).Value = timestamp;
        command.Parameters.Add("@EventType", SqlDbType.Int).Value = eventType;
        command.Parameters.Add("@EventId", SqlDbType.Int).Value = eventType;
        command.Parameters.Add("@EventTimestampLocal", SqlDbType.DateTime2).Value = localTimestamp;
        command.Parameters.Add("@TimeZoneId", SqlDbType.NVarChar, 128).Value = string.IsNullOrEmpty(e.TimeZoneId) ? "UTC" : e.TimeZoneId;
        command.Parameters.Add("@ZoneId", SqlDbType.UniqueIdentifier).Value = e.Zone ?? Guid.Empty;
        command.Parameters.Add("@OfflinePeriod", SqlDbType.Int).Value = e.OfflinePeriod ?? 0;

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task InsertIntrusionEventAsync(IntrusionEventIngestion e, int eventType, DateTime timestamp, CancellationToken cancellationToken)
    {
        using SqlConnection connection = Configuration.CreateSqlDatabaseConnection();
        using var command = new SqlCommand("dbo.InsertIntrusionEvent", connection) { CommandType = CommandType.StoredProcedure };

        command.Parameters.Add("@EventTimestamp", SqlDbType.DateTime2).Value = timestamp;
        command.Parameters.Add("@EventType", SqlDbType.Int).Value = eventType;
        command.Parameters.Add("@IntrusionUnitId", SqlDbType.UniqueIdentifier).Value = e.IntrusionUnit ?? Guid.Empty;
        command.Parameters.Add("@IntrusionAreaId", SqlDbType.UniqueIdentifier).Value = e.IntrusionArea ?? Guid.Empty;
        command.Parameters.Add("@DeviceId", SqlDbType.UniqueIdentifier).Value = e.Device ?? Guid.Empty;
        command.Parameters.Add("@SourceGuid", SqlDbType.UniqueIdentifier).Value = e.Source ?? Guid.Empty;
        command.Parameters.Add("@OccurrencePeriod", SqlDbType.Int).Value = e.OccurrencePeriod ?? 0;
        command.Parameters.Add("@TimeZoneId", SqlDbType.NVarChar, 128).Value = string.IsNullOrEmpty(e.TimeZoneId) ? "UTC" : e.TimeZoneId;
        command.Parameters.Add("@InitiatorId", SqlDbType.UniqueIdentifier).Value = e.Initiator ?? Guid.Empty;

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task InsertVideoEventAsync(VideoEventIngestion e, int eventType, DateTime eventTime, byte[] thumbnail, CancellationToken cancellationToken)
    {
        using SqlConnection connection = Configuration.CreateSqlDatabaseConnection();
        using var command = new SqlCommand("dbo.InsertVideoEvent", connection) { CommandType = CommandType.StoredProcedure };

        command.Parameters.Add("@EventTime", SqlDbType.DateTime2).Value = eventTime;
        command.Parameters.Add("@CameraGuid", SqlDbType.UniqueIdentifier).Value = e.Camera ?? Guid.Empty;
        command.Parameters.Add("@ArchiveSourceGuid", SqlDbType.UniqueIdentifier).Value = e.ArchiveSource ?? Guid.Empty;
        command.Parameters.Add("@EventType", SqlDbType.Int).Value = eventType;
        command.Parameters.Add("@Value", SqlDbType.BigInt).Value = (long)(e.Value ?? 0u);
        command.Parameters.Add("@Capabilities", SqlDbType.BigInt).Value = (long)(e.Capabilities ?? 0u);
        command.Parameters.Add("@TimeZone", SqlDbType.NVarChar, 128).Value = string.IsNullOrEmpty(e.TimeZone) ? "UTC" : e.TimeZone;
        command.Parameters.Add("@Notes", SqlDbType.NVarChar, -1).Value = (object)e.Notes ?? DBNull.Value;
        command.Parameters.Add("@XmlData", SqlDbType.NVarChar, -1).Value = (object)e.XmlData ?? DBNull.Value;
        command.Parameters.Add("@Thumbnail", SqlDbType.VarBinary, -1).Value = (object)thumbnail ?? DBNull.Value;

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task InsertHealthEventAsync(HealthEventIngestion e, DateTime timestamp, CancellationToken cancellationToken)
    {
        using SqlConnection connection = Configuration.CreateSqlDatabaseConnection();
        using var command = new SqlCommand("dbo.InsertHealthEvent", connection) { CommandType = CommandType.StoredProcedure };

        command.Parameters.Add("@EventTimestamp", SqlDbType.DateTime2).Value = timestamp;
        command.Parameters.Add("@HealthEventId", SqlDbType.Int).Value = e.HealthEventId ?? 0;
        command.Parameters.Add("@EventSourceTypeId", SqlDbType.Int).Value = e.EventSourceTypeId ?? 0;
        command.Parameters.Add("@SourceEntityGuid", SqlDbType.UniqueIdentifier).Value = e.Source ?? Guid.Empty;
        command.Parameters.Add("@EventDescription", SqlDbType.NVarChar, -1).Value = e.Description ?? string.Empty;
        command.Parameters.Add("@MachineName", SqlDbType.NVarChar, 512).Value = e.MachineName ?? string.Empty;
        command.Parameters.Add("@SeverityId", SqlDbType.Int).Value = e.SeverityId ?? 0;
        command.Parameters.Add("@ErrorNumber", SqlDbType.Int).Value = e.ErrorNumber ?? 0;
        command.Parameters.Add("@Occurrence", SqlDbType.BigInt).Value = e.Occurrence ?? 0;
        command.Parameters.Add("@ObserverEntity", SqlDbType.UniqueIdentifier).Value = e.Observer ?? Guid.Empty;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = e.IsActive ?? false;

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task InsertHealthStatisticAsync(HealthStatisticsIngestion e, DateTime lastErrorTimestamp, CancellationToken cancellationToken)
    {
        using SqlConnection connection = Configuration.CreateSqlDatabaseConnection();
        using var command = new SqlCommand("dbo.InsertHealthStatistic", connection) { CommandType = CommandType.StoredProcedure };

        command.Parameters.Add("@SourceEntityGuid", SqlDbType.UniqueIdentifier).Value = e.Source ?? Guid.Empty;
        command.Parameters.Add("@EventSourceType", SqlDbType.Int).Value = e.EventSourceType ?? 0;
        command.Parameters.Add("@FailureCount", SqlDbType.Int).Value = e.FailureCount ?? 0;
        command.Parameters.Add("@RtpPacketLoss", SqlDbType.Int).Value = e.RtpPacketLoss ?? 0;
        command.Parameters.Add("@CalculationStatus", SqlDbType.Int).Value = e.CalculationStatus ?? 0;
        command.Parameters.Add("@UnexpectedDowntimeTicks", SqlDbType.BigInt).Value = TimeSpan.FromSeconds(e.UnexpectedDowntimeSeconds ?? 0).Ticks;
        command.Parameters.Add("@ExpectedDowntimeTicks", SqlDbType.BigInt).Value = TimeSpan.FromSeconds(e.ExpectedDowntimeSeconds ?? 0).Ticks;
        command.Parameters.Add("@UptimeTicks", SqlDbType.BigInt).Value = TimeSpan.FromSeconds(e.UptimeSeconds ?? 0).Ticks;
        command.Parameters.Add("@Mttr", SqlDbType.Real).Value = e.Mttr ?? 0f;
        command.Parameters.Add("@Mtbf", SqlDbType.Real).Value = e.Mtbf ?? 0f;
        command.Parameters.Add("@Availability", SqlDbType.Real).Value = e.Availability ?? 0f;
        command.Parameters.Add("@LastErrorTimestamp", SqlDbType.DateTime2).Value = lastErrorTimestamp;
        command.Parameters.Add("@ObserverEntity", SqlDbType.UniqueIdentifier).Value = e.Observer ?? Guid.Empty;

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task InsertActivityTrailAsync(ActivityTrailIngestion e, DateTime timestamp, CancellationToken cancellationToken)
    {
        using SqlConnection connection = Configuration.CreateSqlDatabaseConnection();
        using var command = new SqlCommand("dbo.InsertActivityTrail", connection) { CommandType = CommandType.StoredProcedure };

        command.Parameters.Add("@EventTimestamp", SqlDbType.DateTime2).Value = timestamp;
        command.Parameters.Add("@ActivityType", SqlDbType.Int).Value = e.ActivityType ?? 0;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = e.Description ?? string.Empty;
        command.Parameters.Add("@EntityGuid", SqlDbType.UniqueIdentifier).Value = (object)e.Entity ?? DBNull.Value;
        command.Parameters.Add("@EntityType", SqlDbType.Int).Value = e.EntityType ?? 0;
        command.Parameters.Add("@EntityName", SqlDbType.NVarChar, 512).Value = e.EntityName ?? string.Empty;
        command.Parameters.Add("@InitiatorGuid", SqlDbType.UniqueIdentifier).Value = (object)e.Initiator ?? DBNull.Value;
        command.Parameters.Add("@InitiatorType", SqlDbType.Int).Value = e.InitiatorType ?? 0;
        command.Parameters.Add("@InitiatorName", SqlDbType.NVarChar, 512).Value = e.InitiatorName ?? string.Empty;
        command.Parameters.Add("@ApplicationType", SqlDbType.Int).Value = e.ApplicationType ?? 0;
        command.Parameters.Add("@ApplicationName", SqlDbType.NVarChar, 512).Value = e.ApplicationName ?? string.Empty;
        command.Parameters.Add("@MachineName", SqlDbType.NVarChar, 512).Value = e.MachineName ?? string.Empty;

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task InsertAuditTrailAsync(AuditTrailIngestion e, DateTime timestamp, CancellationToken cancellationToken)
    {
        using SqlConnection connection = Configuration.CreateSqlDatabaseConnection();
        using var command = new SqlCommand("dbo.InsertAuditTrail", connection) { CommandType = CommandType.StoredProcedure };

        command.Parameters.Add("@EventTimestamp", SqlDbType.DateTime2).Value = timestamp;
        command.Parameters.Add("@ModificationType", SqlDbType.Int).Value = e.ModificationType ?? 0;
        command.Parameters.Add("@AuditFormat", SqlDbType.Int).Value = e.AuditFormat ?? 0;
        command.Parameters.Add("@OldValue", SqlDbType.NVarChar, -1).Value = e.OldValue ?? string.Empty;
        command.Parameters.Add("@NewValue", SqlDbType.NVarChar, -1).Value = e.NewValue ?? string.Empty;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = e.Description ?? string.Empty;
        command.Parameters.Add("@EntityGuid", SqlDbType.UniqueIdentifier).Value = (object)e.Entity ?? DBNull.Value;
        command.Parameters.Add("@EntityType", SqlDbType.Int).Value = e.EntityType ?? 0;
        command.Parameters.Add("@EntityName", SqlDbType.NVarChar, 512).Value = e.EntityName ?? string.Empty;
        command.Parameters.Add("@InitiatorGuid", SqlDbType.UniqueIdentifier).Value = (object)e.Initiator ?? DBNull.Value;
        command.Parameters.Add("@InitiatorType", SqlDbType.Int).Value = e.InitiatorType ?? 0;
        command.Parameters.Add("@InitiatorName", SqlDbType.NVarChar, 512).Value = e.InitiatorName ?? string.Empty;
        command.Parameters.Add("@ApplicationType", SqlDbType.Int).Value = e.ApplicationType ?? 0;
        command.Parameters.Add("@ApplicationName", SqlDbType.NVarChar, 512).Value = e.ApplicationName ?? string.Empty;
        command.Parameters.Add("@MachineName", SqlDbType.NVarChar, 512).Value = e.MachineName ?? string.Empty;

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public override IEnumerable<DatabaseCleanupThreshold> GetDatabaseCleanupThresholds()
    {
        foreach (KeyValuePair<string, (string Title, string TimestampColumn, bool DefaultIsEnabled, int DefaultRetentionPeriod)> threshold in s_cleanupThresholds)
        {
            yield return new DatabaseCleanupThreshold(threshold.Key, threshold.Value.Title, threshold.Value.DefaultIsEnabled, threshold.Value.DefaultRetentionPeriod);
        }
    }

    public override void DatabaseCleanup(string name, int retentionPeriod)
    {
        if (State != DatabaseState.Connected || !s_cleanupThresholds.TryGetValue(name, out (string Title, string TimestampColumn, bool DefaultIsEnabled, int DefaultRetentionPeriod) threshold))
        {
            return;
        }

        // The threshold name is the table name; both come from the same constants.
        using SqlConnection connection = Configuration.CreateSqlDatabaseConnection();
        using var command = new SqlCommand($"DELETE FROM {name} WHERE {threshold.TimestampColumn} < @Cutoff", connection);
        command.Parameters.Add("@Cutoff", SqlDbType.DateTime2).Value = DateTime.UtcNow.AddDays(-retentionPeriod);

        connection.Open();
        command.ExecuteNonQuery();
    }
}
