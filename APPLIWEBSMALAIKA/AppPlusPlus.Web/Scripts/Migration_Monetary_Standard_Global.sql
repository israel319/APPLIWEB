-- ============================================================================
-- Modèle monétaire standard GlobalShoping
-- Quatuor : Montant + Id_Monais (FK T_Moneys) + Taux + Montant_Apres_Conversion
-- Idempotent — respect conventions SQL Server (types, FK nommées, sp_rename)
-- ============================================================================
USE GlobalShoping;
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ── Helper : ajouter colonne si absente ──
-- (répété par table pour clarté et traçabilité)

-- (taux courant utilisé dans les backfills via sous-requête T_Taux)

-- ============================================================================
-- T_Facts — en-tête facture
-- ============================================================================
IF COL_LENGTH('dbo.T_Facts', 'Id_Monais') IS NULL AND COL_LENGTH('dbo.T_Facts', 'MoneyId') IS NOT NULL
    EXEC sp_rename 'dbo.T_Facts.MoneyId', 'Id_Monais', 'COLUMN';
GO

IF COL_LENGTH('dbo.T_Facts', 'Montant') IS NULL
    ALTER TABLE dbo.T_Facts ADD Montant DECIMAL(18, 2) NULL;
GO

IF COL_LENGTH('dbo.T_Facts', 'Montant_Apres_Conversion') IS NULL
    ALTER TABLE dbo.T_Facts ADD Montant_Apres_Conversion DECIMAL(18, 2) NULL;
GO

UPDATE f
SET
    Montant = v.MontantNatif,
    Montant_Apres_Conversion = v.MontantApres
FROM dbo.T_Facts f
LEFT JOIN dbo.T_Moneys m ON m.Id_Monais = f.Id_Monais
CROSS APPLY (
    SELECT
        IsUsd = CASE WHEN UPPER(LTRIM(RTRIM(ISNULL(m.Description_Monais, '')))) = 'USD' THEN 1 ELSE 0 END,
        TotalFc = ISNULL(f.Total_Apres_Reduction, f.Total),
        TauxVal = NULLIF(f.Taux, 0)
) x
CROSS APPLY (
    SELECT
        MontantNatif = CAST(CASE
            WHEN x.IsUsd = 1 AND x.TauxVal IS NOT NULL THEN ROUND(x.TotalFc / x.TauxVal, 2)
            ELSE ROUND(x.TotalFc, 0)
        END AS DECIMAL(18, 2)),
        MontantApres = CAST(CASE
            WHEN x.IsUsd = 1 THEN ROUND(x.TotalFc, 0)
            WHEN x.TauxVal IS NOT NULL THEN ROUND(x.TotalFc / x.TauxVal, 2)
            ELSE 0
        END AS DECIMAL(18, 2))
) v
WHERE f.Montant IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Facts_Moneys')
BEGIN
    ALTER TABLE dbo.T_Facts WITH NOCHECK
        ADD CONSTRAINT FK_T_Facts_Moneys FOREIGN KEY (Id_Monais) REFERENCES dbo.T_Moneys (Id_Monais);
    ALTER TABLE dbo.T_Facts CHECK CONSTRAINT FK_T_Facts_Moneys;
END
GO

-- ============================================================================
-- T_Payments — paiements facture
-- ============================================================================
IF COL_LENGTH('dbo.T_Payments', 'Montant') IS NULL AND COL_LENGTH('dbo.T_Payments', 'Amount') IS NOT NULL
    EXEC sp_rename 'dbo.T_Payments.Amount', 'Montant', 'COLUMN';
GO

IF COL_LENGTH('dbo.T_Payments', 'Montant') IS NULL
    ALTER TABLE dbo.T_Payments ADD Montant DECIMAL(18, 2) NOT NULL CONSTRAINT DF_T_Payments_Montant DEFAULT (0);
GO

IF COL_LENGTH('dbo.T_Payments', 'Id_Monais') IS NULL
    ALTER TABLE dbo.T_Payments ADD Id_Monais INT NULL;
GO

IF COL_LENGTH('dbo.T_Payments', 'Taux') IS NULL
    ALTER TABLE dbo.T_Payments ADD Taux DECIMAL(18, 0) NULL;
GO

IF COL_LENGTH('dbo.T_Payments', 'Montant_Apres_Conversion') IS NULL
    ALTER TABLE dbo.T_Payments ADD Montant_Apres_Conversion DECIMAL(18, 2) NULL;
GO

UPDATE p
SET
    Id_Monais = COALESCE(f.Id_Monais, 2),
    Taux = f.Taux,
    Montant = COALESCE(p.Montant, 0),
    Montant_Apres_Conversion = v.MontantApres
FROM dbo.T_Payments p
INNER JOIN dbo.T_Facts f ON f.Id = p.Id_Fact
LEFT JOIN dbo.T_Moneys m ON m.Id_Monais = f.Id_Monais
CROSS APPLY (
    SELECT IsUsd = CASE WHEN UPPER(LTRIM(RTRIM(ISNULL(m.Description_Monais, '')))) = 'USD' THEN 1 ELSE 0 END
) x
CROSS APPLY (
    SELECT MontantApres = CAST(CASE
        WHEN x.IsUsd = 1 AND f.Taux > 0 THEN ROUND(p.Montant * f.Taux, 0)
        WHEN f.Taux > 0 THEN ROUND(p.Montant / f.Taux, 2)
        ELSE 0
    END AS DECIMAL(18, 2))
) v
WHERE p.Id_Monais IS NULL OR p.Taux IS NULL OR p.Montant_Apres_Conversion IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Payments_Moneys')
BEGIN
    ALTER TABLE dbo.T_Payments WITH NOCHECK
        ADD CONSTRAINT FK_T_Payments_Moneys FOREIGN KEY (Id_Monais) REFERENCES dbo.T_Moneys (Id_Monais);
    ALTER TABLE dbo.T_Payments CHECK CONSTRAINT FK_T_Payments_Moneys;
END
GO

-- ============================================================================
-- T_Commande — en-tête commande client
-- ============================================================================
IF COL_LENGTH('dbo.T_Commande', 'Montant') IS NULL
    ALTER TABLE dbo.T_Commande ADD Montant DECIMAL(18, 2) NULL;
GO

IF COL_LENGTH('dbo.T_Commande', 'Id_Monais') IS NULL
    ALTER TABLE dbo.T_Commande ADD Id_Monais INT NULL;
GO

IF COL_LENGTH('dbo.T_Commande', 'Taux') IS NULL
    ALTER TABLE dbo.T_Commande ADD Taux DECIMAL(18, 0) NULL;
GO

IF COL_LENGTH('dbo.T_Commande', 'Montant_Apres_Conversion') IS NULL
    ALTER TABLE dbo.T_Commande ADD Montant_Apres_Conversion DECIMAL(18, 2) NULL;
GO

UPDATE c
SET
    Montant = c.MontantTotal,
    Id_Monais = 1,
    Taux = COALESCE((SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800),
    Montant_Apres_Conversion = CAST(ROUND(
        c.MontantTotal * COALESCE((SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800), 0
    ) AS DECIMAL(18, 2))
FROM dbo.T_Commande c
WHERE c.Montant IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Commande_Moneys')
BEGIN
    ALTER TABLE dbo.T_Commande WITH NOCHECK
        ADD CONSTRAINT FK_T_Commande_Moneys FOREIGN KEY (Id_Monais) REFERENCES dbo.T_Moneys (Id_Monais);
    ALTER TABLE dbo.T_Commande CHECK CONSTRAINT FK_T_Commande_Moneys;
END
GO

-- ============================================================================
-- T_Commande_Details
-- ============================================================================
IF COL_LENGTH('dbo.T_Commande_Details', 'Montant') IS NULL
    ALTER TABLE dbo.T_Commande_Details ADD Montant DECIMAL(18, 2) NULL;
GO

IF COL_LENGTH('dbo.T_Commande_Details', 'Id_Monais') IS NULL
    ALTER TABLE dbo.T_Commande_Details ADD Id_Monais INT NULL;
GO

IF COL_LENGTH('dbo.T_Commande_Details', 'Montant_Apres_Conversion') IS NULL
    ALTER TABLE dbo.T_Commande_Details ADD Montant_Apres_Conversion DECIMAL(18, 2) NULL;
GO

UPDATE d
SET
    Montant = ISNULL(d.Pu, 0) * ISNULL(d.Qte, 0),
    Id_Monais = 1,
    Taux = COALESCE(CAST(d.Taux AS DECIMAL(18, 0)), (SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800),
    Montant_Apres_Conversion = CAST(ROUND(
        ISNULL(d.Pu, 0) * ISNULL(d.Qte, 0)
        * COALESCE(CAST(d.Taux AS DECIMAL(18, 0)), (SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800), 0
    ) AS DECIMAL(18, 2))
FROM dbo.T_Commande_Details d
WHERE d.Montant IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Commande_Details_Moneys')
BEGIN
    ALTER TABLE dbo.T_Commande_Details WITH NOCHECK
        ADD CONSTRAINT FK_T_Commande_Details_Moneys FOREIGN KEY (Id_Monais) REFERENCES dbo.T_Moneys (Id_Monais);
    ALTER TABLE dbo.T_Commande_Details CHECK CONSTRAINT FK_T_Commande_Details_Moneys;
END
GO

-- ============================================================================
-- T_Livraison_Detail
-- ============================================================================
IF COL_LENGTH('dbo.T_Livraison_Detail', 'Montant') IS NULL
    ALTER TABLE dbo.T_Livraison_Detail ADD Montant DECIMAL(18, 2) NULL;
GO

IF COL_LENGTH('dbo.T_Livraison_Detail', 'Id_Monais') IS NULL
    ALTER TABLE dbo.T_Livraison_Detail ADD Id_Monais INT NULL;
GO

IF COL_LENGTH('dbo.T_Livraison_Detail', 'Montant_Apres_Conversion') IS NULL
    ALTER TABLE dbo.T_Livraison_Detail ADD Montant_Apres_Conversion DECIMAL(18, 2) NULL;
GO

UPDATE ld
SET
    Montant = ld.MontantPaye,
    Id_Monais = 2,
    Taux = COALESCE(ld.Taux, (SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800),
    Montant_Apres_Conversion = CAST(CASE
        WHEN COALESCE(ld.Taux, (SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800) > 0
        THEN ROUND(ld.MontantPaye / COALESCE(ld.Taux, (SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800), 2)
        ELSE 0
    END AS DECIMAL(18, 2))
FROM dbo.T_Livraison_Detail ld
WHERE ld.Montant IS NULL AND ld.MontantPaye IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Livraison_Detail_Moneys')
BEGIN
    ALTER TABLE dbo.T_Livraison_Detail WITH NOCHECK
        ADD CONSTRAINT FK_T_Livraison_Detail_Moneys FOREIGN KEY (Id_Monais) REFERENCES dbo.T_Moneys (Id_Monais);
    ALTER TABLE dbo.T_Livraison_Detail CHECK CONSTRAINT FK_T_Livraison_Detail_Moneys;
END
GO

-- ============================================================================
-- T_Appros (en-tête — montant = BenTotal)
-- ============================================================================
IF COL_LENGTH('dbo.T_Appros', 'Montant') IS NULL
    ALTER TABLE dbo.T_Appros ADD Montant DECIMAL(18, 2) NULL;
GO

IF COL_LENGTH('dbo.T_Appros', 'Id_Monais') IS NULL
    ALTER TABLE dbo.T_Appros ADD Id_Monais INT NULL;
GO

IF COL_LENGTH('dbo.T_Appros', 'Montant_Apres_Conversion') IS NULL
    ALTER TABLE dbo.T_Appros ADD Montant_Apres_Conversion DECIMAL(18, 2) NULL;
GO

UPDATE a
SET
    Montant = COALESCE(a.BenTotal, a.PA * a.Qte, 0),
    Id_Monais = 1,
    Taux = COALESCE(a.Taux, (SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800),
    Montant_Apres_Conversion = CAST(ROUND(
        COALESCE(a.BenTotal, a.PA * a.Qte, 0)
        * COALESCE(a.Taux, (SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800), 0
    ) AS DECIMAL(18, 2))
FROM dbo.T_Appros a
WHERE a.Montant IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Appros_Moneys')
BEGIN
    ALTER TABLE dbo.T_Appros WITH NOCHECK
        ADD CONSTRAINT FK_T_Appros_Moneys FOREIGN KEY (Id_Monais) REFERENCES dbo.T_Moneys (Id_Monais);
    ALTER TABLE dbo.T_Appros CHECK CONSTRAINT FK_T_Appros_Moneys;
END
GO

-- ============================================================================
-- T_Appro_Details
-- ============================================================================
IF COL_LENGTH('dbo.T_Appro_Details', 'Montant') IS NULL
    ALTER TABLE dbo.T_Appro_Details ADD Montant DECIMAL(18, 2) NULL;
GO

IF COL_LENGTH('dbo.T_Appro_Details', 'Id_Monais') IS NULL
    ALTER TABLE dbo.T_Appro_Details ADD Id_Monais INT NULL;
GO

IF COL_LENGTH('dbo.T_Appro_Details', 'Montant_Apres_Conversion') IS NULL
    ALTER TABLE dbo.T_Appro_Details ADD Montant_Apres_Conversion DECIMAL(18, 2) NULL;
GO

UPDATE d
SET
    Montant = COALESCE(d.BenTotal, d.PA * d.Qte, 0),
    Id_Monais = COALESCE(a.Id_Monais, art.Id_Monais, 1),
    Taux = COALESCE(d.Taux, a.Taux, (SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800),
    Montant_Apres_Conversion = CAST(ROUND(
        COALESCE(d.BenTotal, d.PA * d.Qte, 0)
        * COALESCE(d.Taux, a.Taux, (SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800), 0
    ) AS DECIMAL(18, 2))
FROM dbo.T_Appro_Details d
LEFT JOIN dbo.T_Appros a ON a.Id = d.Id_Appro
LEFT JOIN dbo.T_Arts art ON art.Id_Article = d.Id_Article
WHERE d.Montant IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Appro_Details_Moneys')
BEGIN
    ALTER TABLE dbo.T_Appro_Details WITH NOCHECK
        ADD CONSTRAINT FK_T_Appro_Details_Moneys FOREIGN KEY (Id_Monais) REFERENCES dbo.T_Moneys (Id_Monais);
    ALTER TABLE dbo.T_Appro_Details CHECK CONSTRAINT FK_T_Appro_Details_Moneys;
END
GO

-- ============================================================================
-- T_Appro_Expense — versements caisse
-- ============================================================================
IF COL_LENGTH('dbo.T_Appro_Expense', 'Montant') IS NULL
    ALTER TABLE dbo.T_Appro_Expense ADD Montant DECIMAL(18, 2) NULL;
GO

IF COL_LENGTH('dbo.T_Appro_Expense', 'Id_Monais') IS NULL
    ALTER TABLE dbo.T_Appro_Expense ADD Id_Monais INT NULL;
GO

IF COL_LENGTH('dbo.T_Appro_Expense', 'Taux') IS NULL
    ALTER TABLE dbo.T_Appro_Expense ADD Taux DECIMAL(18, 0) NULL;
GO

IF COL_LENGTH('dbo.T_Appro_Expense', 'Montant_Apres_Conversion') IS NULL
    ALTER TABLE dbo.T_Appro_Expense ADD Montant_Apres_Conversion DECIMAL(18, 2) NULL;
GO

UPDATE e
SET
    Montant = COALESCE(
        e.AmountUSD,
        CASE WHEN e.AmountCDF IS NOT NULL
            THEN e.AmountCDF / NULLIF(COALESCE((SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800), 0)
            ELSE NULL END,
        0),
    Id_Monais = COALESCE(e.CurrencyId, 1),
    Taux = COALESCE((SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800),
    Montant_Apres_Conversion = CAST(COALESCE(
        e.AmountCDF,
        e.AmountUSD * COALESCE((SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800),
        0
    ) AS DECIMAL(18, 2))
FROM dbo.T_Appro_Expense e
WHERE e.Montant IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Appro_Expense_Moneys')
BEGIN
    ALTER TABLE dbo.T_Appro_Expense WITH NOCHECK
        ADD CONSTRAINT FK_T_Appro_Expense_Moneys FOREIGN KEY (Id_Monais) REFERENCES dbo.T_Moneys (Id_Monais);
    ALTER TABLE dbo.T_Appro_Expense CHECK CONSTRAINT FK_T_Appro_Expense_Moneys;
END
GO

-- ============================================================================
-- T_Versements — clôture journalière
-- ============================================================================
IF COL_LENGTH('dbo.T_Versements', 'Id_Monais') IS NULL
    ALTER TABLE dbo.T_Versements ADD Id_Monais INT NULL;
GO

IF COL_LENGTH('dbo.T_Versements', 'Taux') IS NULL
    ALTER TABLE dbo.T_Versements ADD Taux DECIMAL(18, 0) NULL;
GO

IF COL_LENGTH('dbo.T_Versements', 'Montant_Apres_Conversion') IS NULL
    ALTER TABLE dbo.T_Versements ADD Montant_Apres_Conversion DECIMAL(18, 2) NULL;
GO

UPDATE v
SET
    Id_Monais = 2,
    Taux = COALESCE((SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800),
    Montant_Apres_Conversion = CAST(CASE
        WHEN COALESCE((SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800) > 0
        THEN ROUND(v.Montant / COALESCE((SELECT TOP 1 Taux FROM dbo.T_Taux ORDER BY Id DESC), 2800), 2)
        ELSE 0
    END AS DECIMAL(18, 2))
FROM dbo.T_Versements v
WHERE v.Id_Monais IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Versements_Moneys')
BEGIN
    ALTER TABLE dbo.T_Versements WITH NOCHECK
        ADD CONSTRAINT FK_T_Versements_Moneys FOREIGN KEY (Id_Monais) REFERENCES dbo.T_Moneys (Id_Monais);
    ALTER TABLE dbo.T_Versements CHECK CONSTRAINT FK_T_Versements_Moneys;
END
GO

PRINT 'Migration_Monetary_Standard_Global terminée.';
GO
