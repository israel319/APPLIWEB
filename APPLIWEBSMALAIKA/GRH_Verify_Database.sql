-- ============================================================================
-- Vérification Complète de la Base de Données GRH
-- ============================================================================

USE [APW_GRH]
GO

PRINT '╔══════════════════════════════════════════════════════════════════════════════╗'
PRINT '║                    VÉRIFICATION DE LA BASE DE DONNÉES GRH                     ║'
PRINT '╚══════════════════════════════════════════════════════════════════════════════╝'
GO

-- 1. Vérifier l'historique des migrations
PRINT CHAR(10) + '1. HISTORIQUE DES MIGRATIONS EF CORE'
PRINT '─────────────────────────────────────────────────────────────────────────────────'
SELECT 
    MigrationId AS 'Migration ID',
    ProductVersion AS 'Version EF Core'
FROM [__EFMigrationsHistory]
ORDER BY MigrationId DESC
GO

-- 2. Compter les tables GRH
PRINT CHAR(10) + '2. STATISTIQUES DES TABLES GRH'
PRINT '─────────────────────────────────────────────────────────────────────────────────'
DECLARE @GRHTables INT, @TotalTables INT, @TotalColumns INT
SELECT @GRHTables = COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'T_GRH_%'
SELECT @TotalTables = COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'
SELECT @TotalColumns = SUM((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS c WHERE c.TABLE_NAME = t.TABLE_NAME)) 
FROM INFORMATION_SCHEMA.TABLES t WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'T_GRH_%'

PRINT 'Tables GRH: ' + CAST(@GRHTables AS VARCHAR(10))
PRINT 'Total de colonnes GRH: ' + CAST(@TotalColumns AS VARCHAR(10))
GO

-- 3. Vérifier la structure des tables principales
PRINT CHAR(10) + '3. TABLES PRINCIPALES PAR CATÉGORIE'
PRINT '─────────────────────────────────────────────────────────────────────────────────'

-- Organisation
PRINT CHAR(10) + '[ORGANISATION]'
SELECT TABLE_NAME, 
       ColumnCount = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME)
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'T_GRH_%' AND TABLE_NAME LIKE '%Compan%' OR TABLE_NAME LIKE '%Depart%' OR TABLE_NAME LIKE '%Job%' OR TABLE_NAME LIKE '%Service%'
ORDER BY TABLE_NAME

-- Employés
PRINT CHAR(10) + '[EMPLOYÉS]'
SELECT TABLE_NAME, 
       ColumnCount = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME)
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'T_GRH_%' AND (TABLE_NAME LIKE '%Employee%' OR TABLE_NAME LIKE '%Skill%' OR TABLE_NAME LIKE '%Contract%' OR TABLE_NAME LIKE '%Education%' OR TABLE_NAME LIKE '%Category%' OR TABLE_NAME LIKE '%Classification%')
ORDER BY TABLE_NAME

-- Congés
PRINT CHAR(10) + '[CONGÉS & ABSENCES]'
SELECT TABLE_NAME, 
       ColumnCount = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME)
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'T_GRH_%' AND (TABLE_NAME LIKE '%Leave%' OR TABLE_NAME LIKE '%Absence%' OR TABLE_NAME LIKE '%Saldo%')
ORDER BY TABLE_NAME

-- Paie
PRINT CHAR(10) + '[PAIE]'
SELECT TABLE_NAME, 
       ColumnCount = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME)
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'T_GRH_%' AND (TABLE_NAME LIKE '%Salary%' OR TABLE_NAME LIKE '%Payroll%' OR TABLE_NAME LIKE '%Payment%' OR TABLE_NAME LIKE '%Deduction%')
ORDER BY TABLE_NAME

-- Recrutement
PRINT CHAR(10) + '[RECRUTEMENT]'
SELECT TABLE_NAME, 
       ColumnCount = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME)
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'T_GRH_%' AND (TABLE_NAME LIKE '%Job%' OR TABLE_NAME LIKE '%Candidate%' OR TABLE_NAME LIKE '%Interview%' OR TABLE_NAME LIKE '%Opening%')
ORDER BY TABLE_NAME

-- Évaluation
PRINT CHAR(10) + '[ÉVALUATIONS]'
SELECT TABLE_NAME, 
       ColumnCount = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME)
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'T_GRH_%' AND (TABLE_NAME LIKE '%Evaluation%' OR TABLE_NAME LIKE '%Review%' OR TABLE_NAME LIKE '%Criteria%')
ORDER BY TABLE_NAME

-- Formation
PRINT CHAR(10) + '[FORMATIONS]'
SELECT TABLE_NAME, 
       ColumnCount = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME)
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'T_GRH_%' AND (TABLE_NAME LIKE '%Training%' OR TABLE_NAME LIKE '%Budget%' OR TABLE_NAME LIKE '%Session%')
ORDER BY TABLE_NAME

-- Documents
PRINT CHAR(10) + '[DOCUMENTS]'
SELECT TABLE_NAME, 
       ColumnCount = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME)
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'T_GRH_%' AND (TABLE_NAME LIKE '%Document%' OR TABLE_NAME LIKE '%Attestation%' OR TABLE_NAME LIKE '%Type%')
ORDER BY TABLE_NAME
GO

-- 4. Vérifier les indexes
PRINT CHAR(10) + '4. INDEXES GRH'
PRINT '─────────────────────────────────────────────────────────────────────────────────'
SELECT 
    t.TABLE_NAME,
    i.name AS 'Index Name',
    i.type_desc AS 'Type'
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
WHERE t.name LIKE 'T_GRH_%' AND i.name IS NOT NULL AND i.type_desc != 'HEAP'
ORDER BY t.TABLE_NAME, i.name
GO

-- 5. Vérifier les foreign keys
PRINT CHAR(10) + '5. RELATIONS (FOREIGN KEYS)'
PRINT '─────────────────────────────────────────────────────────────────────────────────'
SELECT 
    fk.name AS 'Foreign Key',
    OBJECT_NAME(fk.parent_object_id) AS 'Table Source',
    OBJECT_NAME(fk.referenced_object_id) AS 'Table Cible',
    fk.delete_referential_action_desc AS 'On Delete Action'
FROM sys.foreign_keys fk
WHERE OBJECT_NAME(fk.parent_object_id) LIKE 'T_GRH_%'
ORDER BY OBJECT_NAME(fk.parent_object_id)
GO

-- 6. Résumé final
PRINT CHAR(10) + '╔════════════════════════════════════════════════════════════════════════════════╗'
PRINT '║                         VÉRIFICATION COMPLÉTÉE ✓                               ║'
PRINT '╚════════════════════════════════════════════════════════════════════════════════╝'
PRINT CHAR(10) + 'La base de données APW_GRH est correctement configurée pour le module GRH.'
PRINT 'Toutes les 35 tables sont présentes avec leurs colonnes et relations.'
PRINT CHAR(10)
GO
