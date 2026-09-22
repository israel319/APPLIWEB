-- ============================================================================
-- Script: Appliquer les Migrations GRH
-- Base de données: APW_GRH
-- Objectif: Enregistrer la migration InitialGRHModule dans EF Core
-- ============================================================================

USE [APW_GRH]
GO

-- Créer la table __EFMigrationsHistory si elle n'existe pas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    )
END
GO

-- Vérifier si la migration existe déjà
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20240610_InitialGRHModule')
BEGIN
    -- Insérer la migration
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20240610_InitialGRHModule', '7.0.0')
    
    PRINT 'Migration 20240610_InitialGRHModule enregistrée avec succès'
END
ELSE
BEGIN
    PRINT 'Migration 20240610_InitialGRHModule déjà existante'
END
GO

-- Vérifier que les tables GRH existent
DECLARE @TableCount INT
SELECT @TableCount = COUNT(*) 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'T_GRH_%'

PRINT CHAR(10) + '=== VÉRIFICATION DES TABLES GRH ===' + CHAR(10)
PRINT 'Nombre de tables GRH trouvées: ' + CAST(@TableCount AS VARCHAR(10))

-- Afficher les tables GRH
SELECT 
    TABLE_NAME,
    COLUMN_COUNT = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME)
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'T_GRH_%'
ORDER BY TABLE_NAME
GO

-- Afficher l'historique des migrations
PRINT CHAR(10) + '=== HISTORIQUE DES MIGRATIONS ===' + CHAR(10)
SELECT [MigrationId], [ProductVersion] 
FROM [__EFMigrationsHistory]
ORDER BY [MigrationId]
GO

PRINT CHAR(10) + '=== MIGRATION APPLIQUÉE AVEC SUCCÈS ===' + CHAR(10)
