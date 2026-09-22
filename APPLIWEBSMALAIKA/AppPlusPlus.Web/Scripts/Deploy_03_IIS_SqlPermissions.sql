/*
  Étape 3 — Droits SQL pour IIS (après restauration)

  Sous IIS, Trusted_Connection = identité du pool d'applications, pas votre compte Windows.
  Adapter le nom du pool si différent (voir IIS > Pools d'applications).
*/
USE [master];
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'IIS APPPOOL\AppPlusPlus')
    CREATE LOGIN [IIS APPPOOL\AppPlusPlus] FROM WINDOWS;
GO

USE [GlobalShoping];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'IIS APPPOOL\AppPlusPlus')
    CREATE USER [IIS APPPOOL\AppPlusPlus] FOR LOGIN [IIS APPPOOL\AppPlusPlus];
ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\AppPlusPlus];
GO

USE [APW_GRH];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'IIS APPPOOL\AppPlusPlus')
    CREATE USER [IIS APPPOOL\AppPlusPlus] FOR LOGIN [IIS APPPOOL\AppPlusPlus];
ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\AppPlusPlus];
GO

PRINT 'OK — droits accordés à IIS APPPOOL\AppPlusPlus';
