// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples.Ingestion;

using System;
using System.Runtime.Serialization;

/// <summary>POST /custom-events. Stores an occurrence of a defined custom event.</summary>
[DataContract]
public sealed class CustomEventIngestion
{
    [DataMember(Name = "customEventId")] public int? CustomEventId { get; set; }
    [DataMember(Name = "source")] public Guid? Source { get; set; }
    [DataMember(Name = "timestamp")] public string Timestamp { get; set; }
    [DataMember(Name = "message")] public string Message { get; set; }
}

// One payload per report domain. Each maps to the columns of its table (see
// Resources\CreationScript.sql and Server\ReportHandlers\Tables.cs). The property names below
// are the exact JSON field names external clients send. Event types are named Security Center
// EventType values (or a customEventId); the other "type" fields are Security Center enumeration
// values passed as integers, so the API does not have to enumerate every domain enum.

/// <summary>POST /access-control-events. Feeds the Cardholder, Credential, Door, Area, Elevator, and Unit activity reports.</summary>
[DataContract]
public class AccessControlEventIngestion
{
    [DataMember(Name = "timestamp")] public string Timestamp { get; set; }
    [DataMember(Name = "eventType")] public string EventType { get; set; }
    [DataMember(Name = "customEventId")] public int? CustomEventId { get; set; }
    [DataMember(Name = "source")] public Guid? Source { get; set; }
    [DataMember(Name = "unit")] public Guid? Unit { get; set; }
    [DataMember(Name = "device")] public Guid? Device { get; set; }
    [DataMember(Name = "accessPoint")] public Guid? AccessPoint { get; set; }
    [DataMember(Name = "credential")] public Guid? Credential { get; set; }
    [DataMember(Name = "cardholder")] public Guid? Cardholder { get; set; }
    [DataMember(Name = "credential2")] public Guid? Credential2 { get; set; }
    [DataMember(Name = "accessPointGroup")] public Guid? AccessPointGroup { get; set; }
    [DataMember(Name = "timeZone")] public string TimeZone { get; set; }
    [DataMember(Name = "occurrencePeriod")] public int? OccurrencePeriod { get; set; }
    [DataMember(Name = "customEventMessage")] public string CustomEventMessage { get; set; }
}

/// <summary>POST /zone-activities. Feeds the Zone activity report.</summary>
[DataContract]
public class ZoneActivityIngestion
{
    [DataMember(Name = "timestamp")] public string Timestamp { get; set; }
    [DataMember(Name = "eventType")] public string EventType { get; set; }
    [DataMember(Name = "customEventId")] public int? CustomEventId { get; set; }
    [DataMember(Name = "zone")] public Guid? Zone { get; set; }
    [DataMember(Name = "localTimestamp")] public string LocalTimestamp { get; set; }
    [DataMember(Name = "timeZoneId")] public string TimeZoneId { get; set; }
    [DataMember(Name = "offlinePeriod")] public int? OfflinePeriod { get; set; }
}

/// <summary>POST /intrusion-events. Feeds the Intrusion area and Intrusion unit activity reports.</summary>
[DataContract]
public class IntrusionEventIngestion
{
    [DataMember(Name = "timestamp")] public string Timestamp { get; set; }
    [DataMember(Name = "eventType")] public string EventType { get; set; }
    [DataMember(Name = "customEventId")] public int? CustomEventId { get; set; }
    [DataMember(Name = "intrusionUnit")] public Guid? IntrusionUnit { get; set; }
    [DataMember(Name = "intrusionArea")] public Guid? IntrusionArea { get; set; }
    [DataMember(Name = "device")] public Guid? Device { get; set; }
    [DataMember(Name = "source")] public Guid? Source { get; set; }
    [DataMember(Name = "occurrencePeriod")] public int? OccurrencePeriod { get; set; }
    [DataMember(Name = "timeZoneId")] public string TimeZoneId { get; set; }
    [DataMember(Name = "initiator")] public Guid? Initiator { get; set; }
}

/// <summary>
/// POST /video-events. Feeds the Camera events and Video motion reports. Video events are
/// built-in event types only; unlike the other domains, the video report's event-type column is
/// unsigned, so it cannot carry a negated custom event id, and no customEventId field is offered.
/// </summary>
[DataContract]
public class VideoEventIngestion
{
    [DataMember(Name = "timestamp")] public string Timestamp { get; set; }
    [DataMember(Name = "camera")] public Guid? Camera { get; set; }
    [DataMember(Name = "archiveSource")] public Guid? ArchiveSource { get; set; }
    [DataMember(Name = "eventType")] public string EventType { get; set; }

    // Value and Capabilities are read back by the video report as unsigned 32-bit, so the
    // contract is uint (0..4294967295) even though the column is BIGINT.
    [DataMember(Name = "value")] public uint? Value { get; set; }
    [DataMember(Name = "notes")] public string Notes { get; set; }
    [DataMember(Name = "xmlData")] public string XmlData { get; set; }
    [DataMember(Name = "capabilities")] public uint? Capabilities { get; set; }
    [DataMember(Name = "timeZone")] public string TimeZone { get; set; }
    [DataMember(Name = "thumbnail")] public string Thumbnail { get; set; }
}

/// <summary>POST /health-events. Feeds the Health history report.</summary>
[DataContract]
public class HealthEventIngestion
{
    [DataMember(Name = "timestamp")] public string Timestamp { get; set; }
    [DataMember(Name = "healthEventId")] public int? HealthEventId { get; set; }
    [DataMember(Name = "eventSourceTypeId")] public int? EventSourceTypeId { get; set; }
    [DataMember(Name = "source")] public Guid? Source { get; set; }
    [DataMember(Name = "description")] public string Description { get; set; }
    [DataMember(Name = "machineName")] public string MachineName { get; set; }
    [DataMember(Name = "severityId")] public int? SeverityId { get; set; }
    [DataMember(Name = "errorNumber")] public int? ErrorNumber { get; set; }
    [DataMember(Name = "occurrence")] public long? Occurrence { get; set; }
    [DataMember(Name = "observer")] public Guid? Observer { get; set; }
    [DataMember(Name = "isActive")] public bool? IsActive { get; set; }
}

/// <summary>POST /health-statistics. Feeds the Health statistics report.</summary>
[DataContract]
public class HealthStatisticsIngestion
{
    [DataMember(Name = "source")] public Guid? Source { get; set; }
    [DataMember(Name = "eventSourceType")] public int? EventSourceType { get; set; }
    [DataMember(Name = "failureCount")] public int? FailureCount { get; set; }
    [DataMember(Name = "rtpPacketLoss")] public int? RtpPacketLoss { get; set; }
    [DataMember(Name = "calculationStatus")] public int? CalculationStatus { get; set; }
    [DataMember(Name = "unexpectedDowntimeSeconds")] public double? UnexpectedDowntimeSeconds { get; set; }
    [DataMember(Name = "expectedDowntimeSeconds")] public double? ExpectedDowntimeSeconds { get; set; }
    [DataMember(Name = "uptimeSeconds")] public double? UptimeSeconds { get; set; }
    [DataMember(Name = "mttr")] public float? Mttr { get; set; }
    [DataMember(Name = "mtbf")] public float? Mtbf { get; set; }
    [DataMember(Name = "availability")] public float? Availability { get; set; }
    [DataMember(Name = "lastErrorTimestamp")] public string LastErrorTimestamp { get; set; }
    [DataMember(Name = "observer")] public Guid? Observer { get; set; }
}

/// <summary>POST /activity-trails. Feeds the Activity trails report.</summary>
[DataContract]
public class ActivityTrailIngestion
{
    [DataMember(Name = "timestamp")] public string Timestamp { get; set; }
    [DataMember(Name = "activityType")] public int? ActivityType { get; set; }
    [DataMember(Name = "description")] public string Description { get; set; }
    [DataMember(Name = "entity")] public Guid? Entity { get; set; }
    [DataMember(Name = "entityType")] public int? EntityType { get; set; }
    [DataMember(Name = "entityName")] public string EntityName { get; set; }
    [DataMember(Name = "initiator")] public Guid? Initiator { get; set; }
    [DataMember(Name = "initiatorType")] public int? InitiatorType { get; set; }
    [DataMember(Name = "initiatorName")] public string InitiatorName { get; set; }
    [DataMember(Name = "applicationType")] public int? ApplicationType { get; set; }
    [DataMember(Name = "applicationName")] public string ApplicationName { get; set; }
    [DataMember(Name = "machineName")] public string MachineName { get; set; }
}

/// <summary>POST /audit-trails. Feeds the Audit trails report.</summary>
[DataContract]
public class AuditTrailIngestion
{
    [DataMember(Name = "timestamp")] public string Timestamp { get; set; }
    [DataMember(Name = "modificationType")] public int? ModificationType { get; set; }
    [DataMember(Name = "auditFormat")] public int? AuditFormat { get; set; }
    [DataMember(Name = "oldValue")] public string OldValue { get; set; }
    [DataMember(Name = "newValue")] public string NewValue { get; set; }
    [DataMember(Name = "description")] public string Description { get; set; }
    [DataMember(Name = "entity")] public Guid? Entity { get; set; }
    [DataMember(Name = "entityType")] public int? EntityType { get; set; }
    [DataMember(Name = "entityName")] public string EntityName { get; set; }
    [DataMember(Name = "initiator")] public Guid? Initiator { get; set; }
    [DataMember(Name = "initiatorType")] public int? InitiatorType { get; set; }
    [DataMember(Name = "initiatorName")] public string InitiatorName { get; set; }
    [DataMember(Name = "applicationType")] public int? ApplicationType { get; set; }
    [DataMember(Name = "applicationName")] public string ApplicationName { get; set; }
    [DataMember(Name = "machineName")] public string MachineName { get; set; }
}
