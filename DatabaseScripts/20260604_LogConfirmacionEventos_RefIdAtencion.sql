USE [help_prv];
GO

IF COL_LENGTH('dbo.help_prv_LogConfirmacionEventos', 'refIdAtencion') IS NULL
BEGIN
    ALTER TABLE [dbo].[help_prv_LogConfirmacionEventos]
    ADD [refIdAtencion] INT NULL;
END
GO

ALTER PROCEDURE [dbo].[help_prv_P_InsertarLogConfirmacionEvento]
     @eventId INT
    ,@status INT
    ,@userId INT
    ,@refIdAtencion INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[help_prv_LogConfirmacionEventos]
           (
            [eventId]
           ,[status]
           ,[logDate]
           ,[userId]
           ,[refIdAtencion])
     VALUES
           (
            @eventId
           ,@status
           ,CURRENT_TIMESTAMP
           ,@userId
           ,@refIdAtencion);
END
GO
