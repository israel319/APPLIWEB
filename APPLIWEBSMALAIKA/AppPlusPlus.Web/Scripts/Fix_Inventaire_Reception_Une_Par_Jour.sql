-- Consolidation réceptions dupliquées + index unique jour
USE GlobalShoping;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF COL_LENGTH('dbo.T_Inventaire_Reception', 'Date_Jour') IS NULL
BEGIN
    ALTER TABLE dbo.T_Inventaire_Reception ADD Date_Jour DATE NULL;
END
GO

UPDATE dbo.T_Inventaire_Reception
SET Date_Jour = CAST(Date_Creation AS DATE)
WHERE Date_Jour IS NULL;
GO

-- Déplacer les lignes vers la réception à conserver (plus ancienne par jour)
;WITH Keep AS (
    SELECT Id_Inventaire, Date_Jour,
           MIN(Id_Reception) AS Id_Keep
    FROM dbo.T_Inventaire_Reception
    GROUP BY Id_Inventaire, Date_Jour
    HAVING COUNT(*) > 1
),
Move AS (
    SELECT r.Id_Reception, k.Id_Keep
    FROM dbo.T_Inventaire_Reception r
    INNER JOIN Keep k ON k.Id_Inventaire = r.Id_Inventaire AND k.Date_Jour = r.Date_Jour
    WHERE r.Id_Reception <> k.Id_Keep
)
UPDATE d
SET Id_Reception = m.Id_Keep
FROM dbo.T_Inventaire_Reception_Detail d
INNER JOIN Move m ON m.Id_Reception = d.Id_Reception;

-- Recalculer totaux des réceptions conservées
UPDATE r
SET Montant_Total = ISNULL(x.Somme, 0)
FROM dbo.T_Inventaire_Reception r
OUTER APPLY (
    SELECT SUM(d.Montant_Ligne) AS Somme
    FROM dbo.T_Inventaire_Reception_Detail d
    WHERE d.Id_Reception = r.Id_Reception
) x;

-- Supprimer réceptions en double
;WITH Keep AS (
    SELECT Id_Inventaire, Date_Jour, MIN(Id_Reception) AS Id_Keep
    FROM dbo.T_Inventaire_Reception
    GROUP BY Id_Inventaire, Date_Jour
)
DELETE r
FROM dbo.T_Inventaire_Reception r
INNER JOIN Keep k ON k.Id_Inventaire = r.Id_Inventaire AND k.Date_Jour = r.Date_Jour
WHERE r.Id_Reception <> k.Id_Keep;
GO

ALTER TABLE dbo.T_Inventaire_Reception
    ALTER COLUMN Date_Jour DATE NOT NULL;
GO

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UQ_T_Inventaire_Reception_Detail_Reception_Article'
      AND object_id = OBJECT_ID('dbo.T_Inventaire_Reception_Detail'))
BEGIN
    ALTER TABLE dbo.T_Inventaire_Reception_Detail
        DROP CONSTRAINT UQ_T_Inventaire_Reception_Detail_Reception_Article;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UQ_T_Inventaire_Reception_Inventaire_Jour'
      AND object_id = OBJECT_ID('dbo.T_Inventaire_Reception'))
BEGIN
    CREATE UNIQUE INDEX UQ_T_Inventaire_Reception_Inventaire_Jour
        ON dbo.T_Inventaire_Reception (Id_Inventaire, Date_Jour);
END
GO

PRINT 'Consolidation terminée.';
GO
