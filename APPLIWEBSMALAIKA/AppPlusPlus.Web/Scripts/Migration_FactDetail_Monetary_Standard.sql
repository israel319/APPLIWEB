-- ============================================================================
-- T_Fact_Details : modèle monétaire standard
-- Montant + Id_Monais + Taux + Montant_Apres_Conversion
-- Suppression de Montant_Fc / Montant_Usd
-- ============================================================================
USE GlobalShoping;
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF COL_LENGTH('dbo.T_Fact_Details', 'Montant') IS NULL
    ALTER TABLE dbo.T_Fact_Details ADD Montant DECIMAL(18, 2) NULL;
GO

IF COL_LENGTH('dbo.T_Fact_Details', 'Id_Monais') IS NULL
    ALTER TABLE dbo.T_Fact_Details ADD Id_Monais INT NULL;
GO

IF COL_LENGTH('dbo.T_Fact_Details', 'Taux') IS NULL
    ALTER TABLE dbo.T_Fact_Details ADD Taux DECIMAL(18, 0) NULL;
GO

IF COL_LENGTH('dbo.T_Fact_Details', 'Montant_Apres_Conversion') IS NULL
    ALTER TABLE dbo.T_Fact_Details ADD Montant_Apres_Conversion DECIMAL(18, 2) NULL;
GO

-- Backfill depuis Montant_Fc / Montant_Usd (migration précédente — dynamic SQL car colonnes peut-être déjà supprimées)
IF COL_LENGTH('dbo.T_Fact_Details', 'Montant_Fc') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
    UPDATE fd
    SET
        Montant = CASE WHEN x.IsUsd = 1 THEN fd.Montant_Usd ELSE fd.Montant_Fc END,
        Id_Monais = x.IdMonais,
        Taux = x.TauxVal,
        Montant_Apres_Conversion = CASE WHEN x.IsUsd = 1 THEN fd.Montant_Fc ELSE fd.Montant_Usd END
    FROM dbo.T_Fact_Details fd
    INNER JOIN dbo.T_Facts f ON f.Id = fd.Id_Fact
    LEFT JOIN dbo.T_Moneys m ON m.Id_Monais = f.Id_Monais
    CROSS APPLY (
        SELECT
            IsUsd = CASE WHEN UPPER(LTRIM(RTRIM(ISNULL(m.Description_Monais, '''')))) = ''USD'' THEN 1 ELSE 0 END,
            IdMonais = COALESCE(f.Id_Monais, 2),
            TauxVal = f.Taux
    ) x
    WHERE fd.Montant IS NULL';
END
GO

-- Backfill legacy (Pu × Qté) pour lignes restantes
UPDATE fd
SET
    Montant = v.MontantNatif,
    Id_Monais = x.IdMonais,
    Taux = x.TauxVal,
    Montant_Apres_Conversion = v.MontantApres
FROM dbo.T_Fact_Details fd
INNER JOIN dbo.T_Facts f ON f.Id = fd.Id_Fact
LEFT JOIN dbo.T_Moneys m ON m.Id_Monais = f.Id_Monais
CROSS APPLY (
    SELECT
        IsUsd = CASE WHEN UPPER(LTRIM(RTRIM(ISNULL(m.Description_Monais, '')))) = 'USD' THEN 1 ELSE 0 END,
        IdMonais = COALESCE(f.Id_Monais, 2),
        TauxVal = f.Taux,
        RawTotal = ISNULL(fd.Pu, 0) * ISNULL(fd.Qte, 0)
) x
CROSS APPLY dbo.fn_MonetaryQuatuorFactDetail(
    fd.Pu, fd.Qte, NULL,
    x.IdMonais,
    COALESCE(x.TauxVal, (SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800)
) v
WHERE fd.Montant IS NULL OR fd.Id_Monais IS NULL OR fd.Taux IS NULL;
GO

-- Note : exécuter aussi Migration_Monetary_Rounding_Enforcement.sql pour les triggers BDD.

IF COL_LENGTH('dbo.T_Fact_Details', 'Montant_Fc') IS NOT NULL
    ALTER TABLE dbo.T_Fact_Details DROP COLUMN Montant_Fc;
GO

IF COL_LENGTH('dbo.T_Fact_Details', 'Montant_Usd') IS NOT NULL
    ALTER TABLE dbo.T_Fact_Details DROP COLUMN Montant_Usd;
GO

PRINT 'Migration_FactDetail_Monetary_Standard terminée.';
GO
