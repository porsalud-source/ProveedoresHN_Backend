-- Apply to the help_prvEntities database before publishing the GPS tracking endpoints.
-- Additive only: legacy GPS logs are not altered or deleted.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.help_prv_TrackingPoints', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.help_prv_TrackingPoints (
        [Sequence] bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TrackingPointId uniqueidentifier NOT NULL UNIQUE,
        SessionId uniqueidentifier NOT NULL,
        AttentionId int NOT NULL,
        AgentId int NOT NULL,
        ProviderId int NOT NULL,
        CapturedAt datetime2(3) NOT NULL,
        ReceivedAt datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        Latitude float NOT NULL,
        Longitude float NOT NULL,
        Accuracy float NULL,
        CONSTRAINT CK_TrackingPoints_Coordinates CHECK (Latitude BETWEEN -90 AND 90 AND Longitude BETWEEN -180 AND 180),
        CONSTRAINT CK_TrackingPoints_Accuracy CHECK (Accuracy IS NULL OR Accuracy >= 0)
    );
    CREATE INDEX IX_TrackingPoints_History ON dbo.help_prv_TrackingPoints (ProviderId, AttentionId, [Sequence]);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.help_prv_TrackingPoints') AND name=N'IX_TrackingPoints_Capture')
    CREATE INDEX IX_TrackingPoints_Capture ON dbo.help_prv_TrackingPoints (ProviderId,AttentionId,AgentId,CapturedAt) INCLUDE (Latitude,Longitude);
-- Analysis-only local times. UTC columns remain the storage/API contract.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.help_prv_TrackingPoints') AND name=N'CapturedAtHonduras')
    ALTER TABLE dbo.help_prv_TrackingPoints ADD CapturedAtHonduras AS DATEADD(hour,-6,CapturedAt);
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.help_prv_TrackingPoints') AND name=N'ReceivedAtHonduras')
    ALTER TABLE dbo.help_prv_TrackingPoints ADD ReceivedAtHonduras AS DATEADD(hour,-6,ReceivedAt);
COMMIT TRANSACTION;
