-- The table and column names are mirrored by the constants in
-- Server\ReportHandlers\Tables.cs; keep both in sync when changing the schema.

CREATE TABLE ActivityTrails
(
    EventTimestamp DATETIME2 NOT NULL,
    ActivityType INT NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    EntityGuid UNIQUEIDENTIFIER NULL,
    EntityType INT NOT NULL,
    EntityName NVARCHAR(512) NOT NULL,
    InitiatorGuid UNIQUEIDENTIFIER NULL,
    InitiatorType INT NOT NULL,
    InitiatorName NVARCHAR(512) NOT NULL,
    ApplicationType INT NOT NULL,
    ApplicationName NVARCHAR(512) NOT NULL,
    MachineName NVARCHAR(512) NOT NULL
);

GO

CREATE INDEX IX_ActivityTrails_EventTimestamp ON ActivityTrails (EventTimestamp);

GO

CREATE TABLE AuditTrails
(
    EventTimestamp DATETIME2 NOT NULL,
    ModificationType INT NOT NULL,
    AuditFormat INT NOT NULL,
    OldValue NVARCHAR(MAX) NOT NULL,
    NewValue NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    EntityGuid UNIQUEIDENTIFIER NULL,
    EntityType INT NOT NULL,
    EntityName NVARCHAR(512) NOT NULL,
    InitiatorGuid UNIQUEIDENTIFIER NULL,
    InitiatorType INT NOT NULL,
    InitiatorName NVARCHAR(512) NOT NULL,
    ApplicationType INT NOT NULL,
    ApplicationName NVARCHAR(512) NOT NULL,
    MachineName NVARCHAR(512) NOT NULL
);

GO

CREATE INDEX IX_AuditTrails_EventTimestamp ON AuditTrails (EventTimestamp);

GO

CREATE TABLE AccessControlEvents
(
    EventTimestamp DATETIME2 NOT NULL,
    EventType INT NOT NULL,
    SourceGuid UNIQUEIDENTIFIER NOT NULL,
    UnitGuid UNIQUEIDENTIFIER NULL,
    DeviceGuid UNIQUEIDENTIFIER NULL,
    APGuid UNIQUEIDENTIFIER NULL,
    CredentialGuid UNIQUEIDENTIFIER NULL,
    CardholderGuid UNIQUEIDENTIFIER NULL,
    Credential2Guid UNIQUEIDENTIFIER NULL,
    AccessPointGroupGuid UNIQUEIDENTIFIER NULL,
    TimeZone NVARCHAR(128) NOT NULL,
    OccurrencePeriod INT NOT NULL,
    CustomEventMessage NVARCHAR(MAX) NULL
);

GO

CREATE INDEX IX_AccessControlEvents_EventTimestamp ON AccessControlEvents (EventTimestamp);

GO

CREATE TABLE ZoneActivities
(
    EventTimestamp DATETIME2 NOT NULL,
    EventType INT NOT NULL,
    EventId INT NOT NULL,
    EventTimestampLocal DATETIME2 NOT NULL,
    TimeZoneId NVARCHAR(128) NOT NULL,
    ZoneId UNIQUEIDENTIFIER NOT NULL,
    OfflinePeriod INT NOT NULL
);

GO

CREATE INDEX IX_ZoneActivities_EventTimestamp ON ZoneActivities (EventTimestamp);

GO

CREATE TABLE IntrusionEvents
(
    EventTimestamp DATETIME2 NOT NULL,
    EventType INT NOT NULL,
    IntrusionUnitId UNIQUEIDENTIFIER NOT NULL,
    IntrusionAreaId UNIQUEIDENTIFIER NOT NULL,
    DeviceId UNIQUEIDENTIFIER NOT NULL,
    SourceGuid UNIQUEIDENTIFIER NOT NULL,
    OccurrencePeriod INT NOT NULL,
    TimeZoneId NVARCHAR(128) NOT NULL,
    InitiatorId UNIQUEIDENTIFIER NOT NULL
);

GO

CREATE INDEX IX_IntrusionEvents_EventTimestamp ON IntrusionEvents (EventTimestamp);

GO

CREATE TABLE VideoEvents
(
    EventTime DATETIME2 NOT NULL,
    CameraGuid UNIQUEIDENTIFIER NOT NULL,
    ArchiveSourceGuid UNIQUEIDENTIFIER NOT NULL,
    EventType INT NOT NULL,
    Value BIGINT NOT NULL,
    Notes NVARCHAR(MAX) NULL,
    XmlData NVARCHAR(MAX) NULL,
    Capabilities BIGINT NOT NULL,
    TimeZone NVARCHAR(128) NOT NULL,
    Thumbnail VARBINARY(MAX) NULL
);

GO

CREATE INDEX IX_VideoEvents_EventTime ON VideoEvents (EventTime);

GO

-- Insert stored procedures: one per table. The plugin writes events through these
-- procedures (see SampleDatabaseManager) instead of building inline INSERT statements.

CREATE PROCEDURE InsertActivityTrail
    @EventTimestamp DATETIME2,
    @ActivityType INT,
    @Description NVARCHAR(MAX),
    @EntityType INT,
    @EntityName NVARCHAR(512),
    @InitiatorType INT,
    @InitiatorName NVARCHAR(512),
    @ApplicationType INT,
    @ApplicationName NVARCHAR(512),
    @MachineName NVARCHAR(512),
    @EntityGuid UNIQUEIDENTIFIER = NULL,
    @InitiatorGuid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ActivityTrails (EventTimestamp, ActivityType, Description, EntityGuid, EntityType, EntityName, InitiatorGuid, InitiatorType, InitiatorName, ApplicationType, ApplicationName, MachineName)
    VALUES (@EventTimestamp, @ActivityType, @Description, @EntityGuid, @EntityType, @EntityName, @InitiatorGuid, @InitiatorType, @InitiatorName, @ApplicationType, @ApplicationName, @MachineName);
END;

GO

CREATE PROCEDURE InsertAuditTrail
    @EventTimestamp DATETIME2,
    @ModificationType INT,
    @AuditFormat INT,
    @OldValue NVARCHAR(MAX),
    @NewValue NVARCHAR(MAX),
    @Description NVARCHAR(MAX),
    @EntityType INT,
    @EntityName NVARCHAR(512),
    @InitiatorType INT,
    @InitiatorName NVARCHAR(512),
    @ApplicationType INT,
    @ApplicationName NVARCHAR(512),
    @MachineName NVARCHAR(512),
    @EntityGuid UNIQUEIDENTIFIER = NULL,
    @InitiatorGuid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AuditTrails (EventTimestamp, ModificationType, AuditFormat, OldValue, NewValue, Description, EntityGuid, EntityType, EntityName, InitiatorGuid, InitiatorType, InitiatorName, ApplicationType, ApplicationName, MachineName)
    VALUES (@EventTimestamp, @ModificationType, @AuditFormat, @OldValue, @NewValue, @Description, @EntityGuid, @EntityType, @EntityName, @InitiatorGuid, @InitiatorType, @InitiatorName, @ApplicationType, @ApplicationName, @MachineName);
END;

GO

CREATE PROCEDURE InsertAccessControlEvent
    @EventTimestamp DATETIME2,
    @EventType INT,
    @SourceGuid UNIQUEIDENTIFIER,
    @CardholderGuid UNIQUEIDENTIFIER = NULL,
    @CredentialGuid UNIQUEIDENTIFIER = NULL,
    @UnitGuid UNIQUEIDENTIFIER = NULL,
    @DeviceGuid UNIQUEIDENTIFIER = NULL,
    @APGuid UNIQUEIDENTIFIER = NULL,
    @Credential2Guid UNIQUEIDENTIFIER = NULL,
    @AccessPointGroupGuid UNIQUEIDENTIFIER = NULL,
    @TimeZone NVARCHAR(128) = 'UTC',
    @OccurrencePeriod INT = 0,
    @CustomEventMessage NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AccessControlEvents (EventTimestamp, EventType, SourceGuid, UnitGuid, DeviceGuid, APGuid, CredentialGuid, CardholderGuid, Credential2Guid, AccessPointGroupGuid, TimeZone, OccurrencePeriod, CustomEventMessage)
    VALUES (@EventTimestamp, @EventType, @SourceGuid, @UnitGuid, @DeviceGuid, @APGuid, @CredentialGuid, @CardholderGuid, @Credential2Guid, @AccessPointGroupGuid, @TimeZone, @OccurrencePeriod, @CustomEventMessage);
END;

GO

CREATE PROCEDURE InsertZoneActivity
    @EventTimestamp DATETIME2,
    @EventType INT,
    @EventId INT,
    @EventTimestampLocal DATETIME2,
    @TimeZoneId NVARCHAR(128),
    @ZoneId UNIQUEIDENTIFIER,
    @OfflinePeriod INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ZoneActivities (EventTimestamp, EventType, EventId, EventTimestampLocal, TimeZoneId, ZoneId, OfflinePeriod)
    VALUES (@EventTimestamp, @EventType, @EventId, @EventTimestampLocal, @TimeZoneId, @ZoneId, @OfflinePeriod);
END;

GO

CREATE PROCEDURE InsertIntrusionEvent
    @EventTimestamp DATETIME2,
    @EventType INT,
    @IntrusionUnitId UNIQUEIDENTIFIER,
    @IntrusionAreaId UNIQUEIDENTIFIER,
    @DeviceId UNIQUEIDENTIFIER,
    @SourceGuid UNIQUEIDENTIFIER,
    @OccurrencePeriod INT,
    @TimeZoneId NVARCHAR(128),
    @InitiatorId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO IntrusionEvents (EventTimestamp, EventType, IntrusionUnitId, IntrusionAreaId, DeviceId, SourceGuid, OccurrencePeriod, TimeZoneId, InitiatorId)
    VALUES (@EventTimestamp, @EventType, @IntrusionUnitId, @IntrusionAreaId, @DeviceId, @SourceGuid, @OccurrencePeriod, @TimeZoneId, @InitiatorId);
END;

GO

CREATE PROCEDURE InsertVideoEvent
    @EventTime DATETIME2,
    @CameraGuid UNIQUEIDENTIFIER,
    @ArchiveSourceGuid UNIQUEIDENTIFIER,
    @EventType INT,
    @Value BIGINT,
    @Capabilities BIGINT,
    @TimeZone NVARCHAR(128),
    @Notes NVARCHAR(MAX) = NULL,
    @XmlData NVARCHAR(MAX) = NULL,
    @Thumbnail VARBINARY(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO VideoEvents (EventTime, CameraGuid, ArchiveSourceGuid, EventType, Value, Notes, XmlData, Capabilities, TimeZone, Thumbnail)
    VALUES (@EventTime, @CameraGuid, @ArchiveSourceGuid, @EventType, @Value, @Notes, @XmlData, @Capabilities, @TimeZone, @Thumbnail);
END;

GO

CREATE TABLE dbo.CustomEvents
(
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EventTimestamp DATETIME2 NOT NULL,
    CustomEventId INT NOT NULL,
    SourceGuid UNIQUEIDENTIFIER NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    ExtraHiddenPayload NVARCHAR(MAX) NULL
);

GO

CREATE INDEX IX_CustomEvents_Query ON dbo.CustomEvents (CustomEventId, EventTimestamp) INCLUDE (SourceGuid);

GO

CREATE PROCEDURE dbo.InsertCustomEvent
    @EventTimestamp DATETIME2,
    @CustomEventId INT,
    @SourceGuid UNIQUEIDENTIFIER,
    @Message NVARCHAR(MAX),
    @ExtraHiddenPayload NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.CustomEvents (EventTimestamp, CustomEventId, SourceGuid, Message, ExtraHiddenPayload)
    VALUES (@EventTimestamp, @CustomEventId, @SourceGuid, @Message, @ExtraHiddenPayload);
END;

GO
