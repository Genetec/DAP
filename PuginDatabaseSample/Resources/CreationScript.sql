CREATE TABLE Logs
(
    Timestamp DATETIME2 NOT NULL,
    LogLevel int NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Exception NVARCHAR(MAX) NULL,
    CorrelationID NVARCHAR(100) NULL,
);

GO

CREATE PROCEDURE InsertLog
    @Timestamp DATETIME2,
    @LogLevel int,
    @Message NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO Logs (Timestamp, LogLevel, Message)
    VALUES (@Timestamp, @LogLevel, @Message)
END

GO

CREATE PROCEDURE DeleteOldLogs
    @DaysOld INT
AS
BEGIN
    BEGIN
        DELETE FROM Logs
        WHERE Timestamp < DATEADD(DAY, -@DaysOld, GETDATE());
    END
END