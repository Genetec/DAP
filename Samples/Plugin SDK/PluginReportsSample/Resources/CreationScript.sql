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

CREATE TABLE HealthEvents
(
    EventTimestamp DATETIME2 NOT NULL,
    HealthEventId INT NOT NULL,
    EventSourceTypeId INT NOT NULL,
    SourceEntityGuid UNIQUEIDENTIFIER NOT NULL,
    EventDescription NVARCHAR(MAX) NOT NULL,
    MachineName NVARCHAR(512) NOT NULL,
    SeverityId INT NOT NULL,
    ErrorNumber INT NOT NULL,
    Occurrence BIGINT NOT NULL,
    ObserverEntity UNIQUEIDENTIFIER NOT NULL,
    IsActive BIT NOT NULL
);

GO

CREATE INDEX IX_HealthEvents_EventTimestamp ON HealthEvents (EventTimestamp);

GO

-- Finds the history whose active state is replaced by an incoming event.
CREATE INDEX IX_HealthEvents_StateKey ON HealthEvents (HealthEventId, SourceEntityGuid, ObserverEntity);

GO

CREATE TABLE HealthStatistics
(
    SourceEntityGuid UNIQUEIDENTIFIER NOT NULL,
    EventSourceType INT NOT NULL,
    FailureCount INT NOT NULL,
    RtpPacketLoss INT NOT NULL,
    CalculationStatus INT NOT NULL,
    UnexpectedDowntimeTicks BIGINT NOT NULL,
    ExpectedDowntimeTicks BIGINT NOT NULL,
    UptimeTicks BIGINT NOT NULL,
    Mttr REAL NOT NULL,
    Mtbf REAL NOT NULL,
    Availability REAL NOT NULL,
    LastErrorTimestamp DATETIME2 NOT NULL,
    ObserverEntity UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_HealthStatistics PRIMARY KEY (SourceEntityGuid, EventSourceType, ObserverEntity)
);

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

CREATE PROCEDURE InsertHealthEvent
    @EventTimestamp DATETIME2,
    @HealthEventId INT,
    @EventSourceTypeId INT,
    @SourceEntityGuid UNIQUEIDENTIFIER,
    @EventDescription NVARCHAR(MAX),
    @MachineName NVARCHAR(512),
    @SeverityId INT,
    @ErrorNumber INT,
    @Occurrence BIGINT,
    @ObserverEntity UNIQUEIDENTIFIER,
    @IsActive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Serialize state changes, including when no row exists yet. The last
        -- successfully ingested record determines the active state, not its timestamp.
        DECLARE @LockResource NVARCHAR(255) = CONCAT('PluginReports.HealthEvent:',
            @HealthEventId, ':', @SourceEntityGuid, ':', @ObserverEntity);
        DECLARE @LockResult INT;
        EXEC @LockResult = sys.sp_getapplock
            @Resource = @LockResource,
            @LockMode = 'Exclusive',
            @LockOwner = 'Transaction',
            @LockTimeout = 15000;
        IF @LockResult < 0
            THROW 50001, 'Could not acquire the health-event state lock.', 1;

        UPDATE HealthEvents
        SET IsActive = 0
        WHERE HealthEventId = @HealthEventId
          AND SourceEntityGuid = @SourceEntityGuid
          AND ObserverEntity = @ObserverEntity;

        INSERT INTO HealthEvents (EventTimestamp, HealthEventId, EventSourceTypeId, SourceEntityGuid, EventDescription, MachineName, SeverityId, ErrorNumber, Occurrence, ObserverEntity, IsActive)
        VALUES (@EventTimestamp, @HealthEventId, @EventSourceTypeId, @SourceEntityGuid, @EventDescription, @MachineName, @SeverityId, @ErrorNumber, @Occurrence, @ObserverEntity, @IsActive);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;

GO

CREATE PROCEDURE InsertHealthStatistic
    @SourceEntityGuid UNIQUEIDENTIFIER,
    @EventSourceType INT,
    @FailureCount INT,
    @RtpPacketLoss INT,
    @CalculationStatus INT,
    @UnexpectedDowntimeTicks BIGINT,
    @ExpectedDowntimeTicks BIGINT,
    @UptimeTicks BIGINT,
    @Mttr REAL,
    @Mtbf REAL,
    @Availability REAL,
    @LastErrorTimestamp DATETIME2,
    @ObserverEntity UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Keep the latest snapshot for each source, source type, and observer.
        -- Range locks also protect the insert when the key is not present yet.
        UPDATE HealthStatistics WITH (UPDLOCK, HOLDLOCK)
        SET FailureCount = @FailureCount,
            RtpPacketLoss = @RtpPacketLoss,
            CalculationStatus = @CalculationStatus,
            UnexpectedDowntimeTicks = @UnexpectedDowntimeTicks,
            ExpectedDowntimeTicks = @ExpectedDowntimeTicks,
            UptimeTicks = @UptimeTicks,
            Mttr = @Mttr,
            Mtbf = @Mtbf,
            Availability = @Availability,
            LastErrorTimestamp = @LastErrorTimestamp
        WHERE SourceEntityGuid = @SourceEntityGuid
          AND EventSourceType = @EventSourceType
          AND ObserverEntity = @ObserverEntity;

        IF @@ROWCOUNT = 0
        BEGIN
            INSERT INTO HealthStatistics (SourceEntityGuid, EventSourceType, FailureCount, RtpPacketLoss, CalculationStatus, UnexpectedDowntimeTicks, ExpectedDowntimeTicks, UptimeTicks, Mttr, Mtbf, Availability, LastErrorTimestamp, ObserverEntity)
            VALUES (@SourceEntityGuid, @EventSourceType, @FailureCount, @RtpPacketLoss, @CalculationStatus, @UnexpectedDowntimeTicks, @ExpectedDowntimeTicks, @UptimeTicks, @Mttr, @Mtbf, @Availability, @LastErrorTimestamp, @ObserverEntity);
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
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
