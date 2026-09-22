/*
  Étape 2 — À exécuter sur la machine CIBLE (SSMS, compte sysadmin)

  IMPORTANT :
  - La machine source utilise SQL Server 2025 (17.x).
  - Vous NE POUVEZ PAS restaurer un .bak 2025 sur SQL 2019 / 2022 / Express ancien.
  - Installez SQL Server 2022 ou 2025 sur la cible, OU générez le script du schéma + données.

  Adapter :
  - @BackupFolder : où vous avez copié les .bak
  - @DataFolder    : dossier DATA de SQL sur la machine cible
    (ex. C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\)
*/
DECLARE @BackupFolder NVARCHAR(500) = N'C:\Temp\AppPlusPlusBackup';
DECLARE @DataFolder NVARCHAR(500) = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\';

-- ========== GlobalShoping ==========
RESTORE FILELISTONLY FROM DISK = N'C:\Temp\AppPlusPlusBackup\GlobalShoping.bak';
-- Vérifiez les noms logiques affichés (souvent GlobalShoping / GlobalShoping_log)

IF DB_ID(N'GlobalShoping') IS NOT NULL
BEGIN
    ALTER DATABASE [GlobalShoping] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [GlobalShoping];
END

RESTORE DATABASE [GlobalShoping]
FROM DISK = N'C:\Temp\AppPlusPlusBackup\GlobalShoping.bak'
WITH
    MOVE N'GlobalShoping'     TO @DataFolder + N'GlobalShoping.mdf',
    MOVE N'GlobalShoping_log' TO @DataFolder + N'GlobalShoping_log.ldf',
    REPLACE,
    STATS = 10;

-- ========== APW_GRH ==========
IF DB_ID(N'APW_GRH') IS NOT NULL
BEGIN
    ALTER DATABASE [APW_GRH] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [APW_GRH];
END

RESTORE DATABASE [APW_GRH]
FROM DISK = N'C:\Temp\AppPlusPlusBackup\APW_GRH.bak'
WITH
    MOVE N'APW_GRH'     TO @DataFolder + N'APW_GRH.mdf',
    MOVE N'APW_GRH_log' TO @DataFolder + N'APW_GRH_log.ldf',
    REPLACE,
    STATS = 10;

PRINT 'Restauration terminée. Vérifiez : SELECT name FROM sys.databases WHERE name IN (''GlobalShoping'',''APW_GRH'');';
