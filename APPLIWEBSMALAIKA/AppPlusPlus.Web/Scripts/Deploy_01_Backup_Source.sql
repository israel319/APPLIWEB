/*
  Étape 1 — À exécuter sur la machine SOURCE (celle qui fonctionne)
  Crée les fichiers .bak à copier sur l'autre machine.

  Adapter @BackupFolder si besoin (dossier partagé ou clé USB).
*/
DECLARE @BackupFolder NVARCHAR(500) = N'C:\Temp\AppPlusPlusBackup';

-- Créer le dossier manuellement si nécessaire avant d'exécuter ce script.

BACKUP DATABASE [GlobalShoping]
TO DISK = N'C:\Temp\AppPlusPlusBackup\GlobalShoping.bak'
WITH INIT, COMPRESSION, STATS = 10;

BACKUP DATABASE [APW_GRH]
TO DISK = N'C:\Temp\AppPlusPlusBackup\APW_GRH.bak'
WITH INIT, COMPRESSION, STATS = 10;

PRINT 'Copiez le dossier C:\Temp\AppPlusPlusBackup vers la machine cible.';
