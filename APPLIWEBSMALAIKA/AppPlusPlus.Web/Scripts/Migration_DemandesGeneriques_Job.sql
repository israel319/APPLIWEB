-- ============================================================================
-- SQL Agent Job : génération quotidienne des demandes génériques à 18:30
-- Prérequis : Migration_DemandesGeneriques.sql exécutée
-- Exécuter en tant que sysadmin ou avec droits SQL Agent
-- ============================================================================

USE msdb;
GO

DECLARE @JobName SYSNAME = N'GlobalShoping_GenererDemandesGeneriques';

IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = @JobName)
BEGIN
    EXEC msdb.dbo.sp_delete_job @job_name = @JobName, @delete_unused_schedule = 1;
    PRINT 'Job existant supprimé.';
END
GO

EXEC msdb.dbo.sp_add_job
    @job_name = N'GlobalShoping_GenererDemandesGeneriques',
    @enabled = 1,
    @description = N'Génère les demandes génériques (stock agent sous seuil) chaque jour à 18:30';
GO

EXEC msdb.dbo.sp_add_jobstep
    @job_name = N'GlobalShoping_GenererDemandesGeneriques',
    @step_name = N'Executer sp_GenererDemandesGeneriques',
    @subsystem = N'TSQL',
    @database_name = N'GlobalShoping',
    @command = N'EXEC dbo.sp_GenererDemandesGeneriques;',
    @retry_attempts = 2,
    @retry_interval = 5;
GO

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'Quotidien_1830',
    @freq_type = 4,
    @freq_interval = 1,
    @freq_subday_type = 1,
    @active_start_time = 183000;
GO

EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'GlobalShoping_GenererDemandesGeneriques',
    @schedule_name = N'Quotidien_1830';
GO

EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'GlobalShoping_GenererDemandesGeneriques',
    @server_name = N'(LOCAL)';
GO

PRINT 'Job SQL Agent créé : GlobalShoping_GenererDemandesGeneriques (18:30).';
GO
