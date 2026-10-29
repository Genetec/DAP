// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Server.ReportHandlers;

// Table and column names of the plugin database, the single source of truth used by the
// SELECT statements, the filter conditions, the reader mappings, and the cleanup.
// The schema itself is created by Resources\CreationScript.sql; when changing it,
// keep these constants and the script in sync.

internal static class ActivityTrailTable
{
    public const string Name = "ActivityTrails";

    public static class Columns
    {
        public const string EventTimestamp = "EventTimestamp";
        public const string ActivityType = "ActivityType";
        public const string Description = "Description";
        public const string EntityGuid = "EntityGuid";
        public const string EntityType = "EntityType";
        public const string EntityName = "EntityName";
        public const string InitiatorGuid = "InitiatorGuid";
        public const string InitiatorType = "InitiatorType";
        public const string InitiatorName = "InitiatorName";
        public const string ApplicationType = "ApplicationType";
        public const string ApplicationName = "ApplicationName";
        public const string MachineName = "MachineName";
    }
}

internal static class AuditTrailTable
{
    public const string Name = "AuditTrails";

    public static class Columns
    {
        public const string EventTimestamp = "EventTimestamp";
        public const string ModificationType = "ModificationType";
        public const string AuditFormat = "AuditFormat";
        public const string OldValue = "OldValue";
        public const string NewValue = "NewValue";
        public const string Description = "Description";
        public const string EntityGuid = "EntityGuid";
        public const string EntityType = "EntityType";
        public const string EntityName = "EntityName";
        public const string InitiatorGuid = "InitiatorGuid";
        public const string InitiatorType = "InitiatorType";
        public const string InitiatorName = "InitiatorName";
        public const string ApplicationType = "ApplicationType";
        public const string ApplicationName = "ApplicationName";
        public const string MachineName = "MachineName";
    }
}

internal static class AccessControlEventTable
{
    public const string Name = "AccessControlEvents";

    public static class Columns
    {
        public const string EventTimestamp = "EventTimestamp";
        public const string EventType = "EventType";
        public const string UnitGuid = "UnitGuid";
        public const string DeviceGuid = "DeviceGuid";
        public const string APGuid = "APGuid";
        public const string SourceGuid = "SourceGuid";
        public const string CredentialGuid = "CredentialGuid";
        public const string CardholderGuid = "CardholderGuid";
        public const string Credential2Guid = "Credential2Guid";
        public const string TimeZone = "TimeZone";
        public const string OccurrencePeriod = "OccurrencePeriod";
        public const string AccessPointGroupGuid = "AccessPointGroupGuid";
        public const string CustomEventMessage = "CustomEventMessage";
    }
}

internal static class ZoneActivityTable
{
    public const string Name = "ZoneActivities";

    public static class Columns
    {
        public const string EventTimestamp = "EventTimestamp";
        public const string EventType = "EventType";
        public const string EventId = "EventId";
        public const string EventTimestampLocal = "EventTimestampLocal";
        public const string TimeZoneId = "TimeZoneId";
        public const string ZoneId = "ZoneId";
        public const string OfflinePeriod = "OfflinePeriod";
    }
}

internal static class IntrusionEventTable
{
    public const string Name = "IntrusionEvents";

    public static class Columns
    {
        public const string EventTimestamp = "EventTimestamp";
        public const string EventType = "EventType";
        public const string IntrusionUnitId = "IntrusionUnitId";
        public const string IntrusionAreaId = "IntrusionAreaId";
        public const string DeviceId = "DeviceId";
        public const string SourceGuid = "SourceGuid";
        public const string OccurrencePeriod = "OccurrencePeriod";
        public const string TimeZoneId = "TimeZoneId";
        public const string InitiatorId = "InitiatorId";
    }
}

internal static class VideoEventTable
{
    public const string Name = "VideoEvents";

    public static class Columns
    {
        public const string EventTime = "EventTime";
        public const string CameraGuid = "CameraGuid";
        public const string ArchiveSourceGuid = "ArchiveSourceGuid";
        public const string EventType = "EventType";
        public const string Value = "Value";
        public const string Notes = "Notes";
        public const string XmlData = "XmlData";
        public const string Capabilities = "Capabilities";
        public const string TimeZone = "TimeZone";
        public const string Thumbnail = "Thumbnail";
    }
}
