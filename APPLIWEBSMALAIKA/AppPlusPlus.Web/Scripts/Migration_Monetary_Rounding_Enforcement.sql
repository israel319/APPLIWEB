-- ============================================================================
-- Arrondis monétaires centralisés en BDD (GlobalShoping)
-- Règles alignées sur CurrencyFormat (C#) :
--   - Montant natif USD : 2 décimales | CDF : franc entier
--   - Montant_Apres_Conversion : FC si USD, USD (2 déc.) si CDF
-- Triggers sur toutes les tables du quatuor monétaire.
-- Idempotent.
-- ============================================================================
USE GlobalShoping;
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ── Fonctions ───────────────────────────────────────────────────────────────

CREATE OR ALTER FUNCTION dbo.fn_IsUsdMonais(@IdMonais INT)
RETURNS BIT
WITH SCHEMABINDING
AS
BEGIN
    IF @IdMonais = 1 RETURN 1;
    RETURN 0;
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_RoundMontantNatif(@Amount DECIMAL(18, 6), @IsUsd BIT)
RETURNS DECIMAL(18, 2)
AS
BEGIN
    RETURN CAST(ROUND(@Amount, CASE WHEN @IsUsd = 1 THEN 2 ELSE 0 END) AS DECIMAL(18, 2));
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_ConvertMontantApres(
    @Montant DECIMAL(18, 2),
    @IsUsd BIT,
    @Taux DECIMAL(18, 0))
RETURNS DECIMAL(18, 2)
AS
BEGIN
    IF @Taux IS NULL OR @Taux <= 0 RETURN 0;
    RETURN CAST(ROUND(
        CASE WHEN @IsUsd = 1 THEN @Montant * @Taux ELSE @Montant / @Taux END,
        CASE WHEN @IsUsd = 1 THEN 0 ELSE 2 END
    ) AS DECIMAL(18, 2));
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_MonetaryQuatuorFromNative(
    @Montant DECIMAL(18, 6),
    @IdMonais INT,
    @Taux DECIMAL(18, 0))
RETURNS TABLE
AS
RETURN
(
    SELECT
        MontantNatif = dbo.fn_RoundMontantNatif(@Montant, dbo.fn_IsUsdMonais(@IdMonais)),
        MontantApres = dbo.fn_ConvertMontantApres(
            dbo.fn_RoundMontantNatif(@Montant, dbo.fn_IsUsdMonais(@IdMonais)),
            dbo.fn_IsUsdMonais(@IdMonais),
            @Taux)
);
GO

CREATE OR ALTER FUNCTION dbo.fn_NormalizeLegacyFactCdf(@Amount DECIMAL(18, 6), @Taux DECIMAL(18, 0))
RETURNS DECIMAL(18, 2)
AS
BEGIN
    IF @Taux IS NULL OR @Taux <= 0 OR @Amount <= 0 RETURN CAST(@Amount AS DECIMAL(18, 2));
    IF @Amount < 1000 AND @Amount <> ROUND(@Amount, 0)
        RETURN CAST(ROUND(@Amount * @Taux, 0) AS DECIMAL(18, 2));
    RETURN CAST(ROUND(@Amount, 0) AS DECIMAL(18, 2));
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_MonetaryQuatuorFactDetail(
    @Pu FLOAT,
    @Qte FLOAT,
    @Montant DECIMAL(18, 6),
    @IdMonais INT,
    @Taux DECIMAL(18, 0))
RETURNS TABLE
AS
RETURN
(
    WITH x AS (
        SELECT
            IsUsd = dbo.fn_IsUsdMonais(@IdMonais),
            TauxVal = NULLIF(@Taux, 0),
            RawTotal = CAST(COALESCE(@Montant, ISNULL(@Pu, 0) * ISNULL(@Qte, 0)) AS DECIMAL(18, 6))
    ),
    fc AS (
        SELECT
            x.IsUsd,
            x.TauxVal,
            FcTotal = dbo.fn_NormalizeLegacyFactCdf(x.RawTotal, x.TauxVal)
        FROM x
    )
    SELECT
        MontantNatif = CAST(CASE
            WHEN fc.IsUsd = 1 AND fc.TauxVal IS NOT NULL
                THEN dbo.fn_RoundMontantNatif(fc.FcTotal / fc.TauxVal, 1)
            ELSE dbo.fn_RoundMontantNatif(fc.FcTotal, 0)
        END AS DECIMAL(18, 2)),
        MontantApres = CAST(CASE
            WHEN fc.IsUsd = 1
                THEN dbo.fn_RoundMontantNatif(fc.FcTotal, 0)
            WHEN fc.TauxVal IS NOT NULL
                THEN dbo.fn_ConvertMontantApres(dbo.fn_RoundMontantNatif(fc.FcTotal, 0), 0, fc.TauxVal)
            ELSE 0
        END AS DECIMAL(18, 2))
    FROM fc
);
GO

CREATE OR ALTER FUNCTION dbo.fn_MonetaryQuatuorFactHeader(
    @TotalApresReduction DECIMAL(18, 6),
    @IdMonais INT,
    @Taux DECIMAL(18, 0))
RETURNS TABLE
AS
RETURN
(
    WITH x AS (
        SELECT
            IsUsd = dbo.fn_IsUsdMonais(@IdMonais),
            TauxVal = NULLIF(@Taux, 0),
            Fc = dbo.fn_NormalizeLegacyFactCdf(@TotalApresReduction, NULLIF(@Taux, 0))
    )
    SELECT
        MontantNatif = CAST(CASE
            WHEN x.IsUsd = 1 AND x.TauxVal IS NOT NULL
                THEN dbo.fn_RoundMontantNatif(x.Fc / x.TauxVal, 1)
            ELSE dbo.fn_RoundMontantNatif(x.Fc, 0)
        END AS DECIMAL(18, 2)),
        MontantApres = CAST(CASE
            WHEN x.IsUsd = 1
                THEN dbo.fn_RoundMontantNatif(x.Fc, 0)
            WHEN x.TauxVal IS NOT NULL
                THEN dbo.fn_ConvertMontantApres(dbo.fn_RoundMontantNatif(x.Fc, 0), 0, x.TauxVal)
            ELSE 0
        END AS DECIMAL(18, 2))
    FROM x
);
GO

CREATE OR ALTER FUNCTION dbo.fn_RoundInvoicePayable(@Amount DECIMAL(18, 6), @IsUsd BIT)
RETURNS DECIMAL(18, 2)
AS
BEGIN
    RETURN dbo.fn_RoundMontantNatif(@Amount, @IsUsd);
END;
GO

-- ── Suppression triggers existants ──────────────────────────────────────────

DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql = @sql + N'DROP TRIGGER IF EXISTS ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_id)) + N'.' + QUOTENAME(name) + N';' + CHAR(13)
FROM sys.triggers
WHERE name LIKE 'TR_MonetaryNormalize_%';
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
GO

-- ── Trigger générique (tables opération standard) ───────────────────────────

CREATE OR ALTER TRIGGER dbo.TR_MonetaryNormalize_T_Payments
ON dbo.T_Payments
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE p SET
        Montant = q.MontantNatif,
        Montant_Apres_Conversion = q.MontantApres,
        Id_Monais = COALESCE(i.Id_Monais, p.Id_Monais, 2),
        Taux = COALESCE(i.Taux, p.Taux)
    FROM dbo.T_Payments p
    INNER JOIN inserted i ON p.Id = i.Id
    CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(
        COALESCE(i.Montant, p.Montant, 0),
        COALESCE(i.Id_Monais, p.Id_Monais, 2),
        COALESCE(i.Taux, p.Taux, 2800)) q;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_MonetaryNormalize_T_Commande
ON dbo.T_Commande
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE c SET
        Montant = q.MontantNatif,
        Montant_Apres_Conversion = q.MontantApres,
        Id_Monais = COALESCE(i.Id_Monais, c.Id_Monais, 1),
        Taux = COALESCE(i.Taux, c.Taux, 2800)
    FROM dbo.T_Commande c
    INNER JOIN inserted i ON c.CommandeId = i.CommandeId
    CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(
        COALESCE(i.Montant, c.Montant, i.MontantTotal, c.MontantTotal, 0),
        COALESCE(i.Id_Monais, c.Id_Monais, 1),
        COALESCE(i.Taux, c.Taux, 2800)) q;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_MonetaryNormalize_T_Commande_Details
ON dbo.T_Commande_Details
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE d SET
        Montant = q.MontantNatif,
        Montant_Apres_Conversion = q.MontantApres,
        Id_Monais = COALESCE(i.Id_Monais, d.Id_Monais, 1),
        Taux = COALESCE(CAST(i.Taux AS DECIMAL(18, 0)), d.Taux, 2800)
    FROM dbo.T_Commande_Details d
    INNER JOIN inserted i ON d.Id = i.Id
    CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(
        COALESCE(i.Montant, d.Montant, ISNULL(i.Pu, 0) * ISNULL(i.Qte, 0), 0),
        COALESCE(i.Id_Monais, d.Id_Monais, 1),
        COALESCE(CAST(i.Taux AS DECIMAL(18, 0)), d.Taux, 2800)) q;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_MonetaryNormalize_T_Livraison_Detail
ON dbo.T_Livraison_Detail
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE ld SET
        Montant = q.MontantNatif,
        Montant_Apres_Conversion = q.MontantApres,
        Id_Monais = COALESCE(i.Id_Monais, ld.Id_Monais, 2),
        Taux = COALESCE(i.Taux, ld.Taux, 2800)
    FROM dbo.T_Livraison_Detail ld
    INNER JOIN inserted i ON ld.Id = i.Id
    CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(
        COALESCE(i.Montant, ld.Montant, i.MontantPaye, ld.MontantPaye, 0),
        COALESCE(i.Id_Monais, ld.Id_Monais, 2),
        COALESCE(i.Taux, ld.Taux, 2800)) q;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_MonetaryNormalize_T_Appros
ON dbo.T_Appros
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE a SET
        Montant = q.MontantNatif,
        Montant_Apres_Conversion = q.MontantApres,
        Id_Monais = COALESCE(i.Id_Monais, a.Id_Monais, 1),
        Taux = COALESCE(i.Taux, a.Taux, 2800)
    FROM dbo.T_Appros a
    INNER JOIN inserted i ON a.Id = i.Id
    CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(
        COALESCE(i.Montant, a.Montant, i.BenTotal, a.BenTotal, i.PA * i.Qte, a.PA * a.Qte, 0),
        COALESCE(i.Id_Monais, a.Id_Monais, 1),
        COALESCE(i.Taux, a.Taux, 2800)) q;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_MonetaryNormalize_T_Appro_Details
ON dbo.T_Appro_Details
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE d SET
        Montant = q.MontantNatif,
        Montant_Apres_Conversion = q.MontantApres,
        Id_Monais = COALESCE(i.Id_Monais, d.Id_Monais, 1),
        Taux = COALESCE(i.Taux, d.Taux, 2800)
    FROM dbo.T_Appro_Details d
    INNER JOIN inserted i ON d.Id = i.Id
    CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(
        COALESCE(i.Montant, d.Montant, i.BenTotal, d.BenTotal, i.PA * i.Qte, d.PA * d.Qte, 0),
        COALESCE(i.Id_Monais, d.Id_Monais, 1),
        COALESCE(i.Taux, d.Taux, 2800)) q;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_MonetaryNormalize_T_Appro_Expense
ON dbo.T_Appro_Expense
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE e SET
        Montant = q.MontantNatif,
        Montant_Apres_Conversion = q.MontantApres,
        Id_Monais = COALESCE(i.Id_Monais, e.Id_Monais, 1),
        Taux = COALESCE(i.Taux, e.Taux, 2800)
    FROM dbo.T_Appro_Expense e
    INNER JOIN inserted i ON e.Id = i.Id
    CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(
        COALESCE(i.Montant, e.Montant, i.AmountUSD, e.AmountUSD, 0),
        COALESCE(i.Id_Monais, e.Id_Monais, i.CurrencyId, e.CurrencyId, 1),
        COALESCE(i.Taux, e.Taux, 2800)) q;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_MonetaryNormalize_T_Versements
ON dbo.T_Versements
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE v SET
        Montant_Apres_Conversion = q.MontantApres,
        Id_Monais = COALESCE(i.Id_Monais, v.Id_Monais, 2),
        Taux = COALESCE(i.Taux, v.Taux, 2800)
    FROM dbo.T_Versements v
    INNER JOIN inserted i ON v.Id = i.Id
    CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(
        COALESCE(i.Montant, v.Montant, 0),
        COALESCE(i.Id_Monais, v.Id_Monais, 2),
        COALESCE(i.Taux, v.Taux, 2800)) q;
END;
GO

-- ── Factures (logique Pu×Qté legacy) ────────────────────────────────────────

CREATE OR ALTER TRIGGER dbo.TR_MonetaryNormalize_T_Fact_Details
ON dbo.T_Fact_Details
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE fd SET
        Montant = q.MontantNatif,
        Montant_Apres_Conversion = q.MontantApres,
        Id_Monais = COALESCE(i.Id_Monais, fd.Id_Monais, f.Id_Monais, 2),
        Taux = COALESCE(i.Taux, fd.Taux, f.Taux, 2800)
    FROM dbo.T_Fact_Details fd
    INNER JOIN inserted i ON fd.Id = i.Id
    LEFT JOIN dbo.T_Facts f ON f.Id = fd.Id_Fact
    CROSS APPLY dbo.fn_MonetaryQuatuorFactDetail(
        COALESCE(i.Pu, fd.Pu),
        COALESCE(i.Qte, fd.Qte),
        COALESCE(i.Montant, fd.Montant),
        COALESCE(i.Id_Monais, fd.Id_Monais, f.Id_Monais, 2),
        COALESCE(i.Taux, fd.Taux, f.Taux, 2800)) q;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_MonetaryNormalize_T_Facts
ON dbo.T_Facts
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE f SET
        Total_Apres_Reduction = dbo.fn_RoundInvoicePayable(
            ISNULL(i.Total_Apres_Reduction, f.Total_Apres_Reduction),
            dbo.fn_IsUsdMonais(COALESCE(i.Id_Monais, f.Id_Monais, 2))),
        Montant = q.MontantNatif,
        Montant_Apres_Conversion = q.MontantApres,
        Id_Monais = COALESCE(i.Id_Monais, f.Id_Monais, 2),
        Taux = COALESCE(i.Taux, f.Taux, 2800)
    FROM dbo.T_Facts f
    INNER JOIN inserted i ON f.Id = i.Id
    CROSS APPLY dbo.fn_MonetaryQuatuorFactHeader(
        ISNULL(i.Total_Apres_Reduction, f.Total_Apres_Reduction),
        COALESCE(i.Id_Monais, f.Id_Monais, 2),
        COALESCE(i.Taux, f.Taux, 2800)) q;
END;
GO

-- ── Backfill global (ré-applique les arrondis sur données existantes) ───────

UPDATE fd SET
    Montant = q.MontantNatif,
    Id_Monais = COALESCE(fd.Id_Monais, f.Id_Monais, 2),
    Taux = COALESCE(fd.Taux, f.Taux, 2800),
    Montant_Apres_Conversion = q.MontantApres
FROM dbo.T_Fact_Details fd
LEFT JOIN dbo.T_Facts f ON f.Id = fd.Id_Fact
CROSS APPLY dbo.fn_MonetaryQuatuorFactDetail(
    fd.Pu, fd.Qte, fd.Montant,
    COALESCE(fd.Id_Monais, f.Id_Monais, 2),
    COALESCE(fd.Taux, f.Taux, 2800)) q;
GO

UPDATE f SET
    Total_Apres_Reduction = dbo.fn_RoundInvoicePayable(
        ISNULL(f.Total_Apres_Reduction, f.Total),
        dbo.fn_IsUsdMonais(COALESCE(f.Id_Monais, 2))),
    Montant = q.MontantNatif,
    Id_Monais = COALESCE(f.Id_Monais, 2),
    Montant_Apres_Conversion = q.MontantApres
FROM dbo.T_Facts f
CROSS APPLY dbo.fn_MonetaryQuatuorFactHeader(
    ISNULL(f.Total_Apres_Reduction, f.Total),
    COALESCE(f.Id_Monais, 2),
    COALESCE(f.Taux, 2800)) q;
GO

UPDATE p SET
    Montant = q.MontantNatif,
    Id_Monais = COALESCE(p.Id_Monais, 2),
    Taux = COALESCE(p.Taux, 2800),
    Montant_Apres_Conversion = q.MontantApres
FROM dbo.T_Payments p
CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(p.Montant, COALESCE(p.Id_Monais, 2), COALESCE(p.Taux, 2800)) q;
GO

UPDATE c SET
    Montant = q.MontantNatif,
    Id_Monais = COALESCE(c.Id_Monais, 1),
    Taux = COALESCE(c.Taux, 2800),
    Montant_Apres_Conversion = q.MontantApres
FROM dbo.T_Commande c
CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(COALESCE(c.Montant, c.MontantTotal, 0), COALESCE(c.Id_Monais, 1), COALESCE(c.Taux, 2800)) q;
GO

UPDATE d SET
    Montant = q.MontantNatif,
    Id_Monais = COALESCE(d.Id_Monais, 1),
    Taux = COALESCE(d.Taux, 2800),
    Montant_Apres_Conversion = q.MontantApres
FROM dbo.T_Commande_Details d
CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(COALESCE(d.Montant, ISNULL(d.Pu, 0) * ISNULL(d.Qte, 0), 0), COALESCE(d.Id_Monais, 1), COALESCE(d.Taux, 2800)) q;
GO

UPDATE ld SET
    Montant = q.MontantNatif,
    Id_Monais = COALESCE(ld.Id_Monais, 2),
    Taux = COALESCE(ld.Taux, 2800),
    Montant_Apres_Conversion = q.MontantApres
FROM dbo.T_Livraison_Detail ld
CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(COALESCE(ld.Montant, ld.MontantPaye, 0), COALESCE(ld.Id_Monais, 2), COALESCE(ld.Taux, 2800)) q;
GO

UPDATE a SET
    Montant = q.MontantNatif,
    Id_Monais = COALESCE(a.Id_Monais, 1),
    Taux = COALESCE(a.Taux, 2800),
    Montant_Apres_Conversion = q.MontantApres
FROM dbo.T_Appros a
CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(COALESCE(a.Montant, a.BenTotal, a.PA * a.Qte, 0), COALESCE(a.Id_Monais, 1), COALESCE(a.Taux, 2800)) q;
GO

UPDATE d SET
    Montant = q.MontantNatif,
    Id_Monais = COALESCE(d.Id_Monais, 1),
    Taux = COALESCE(d.Taux, 2800),
    Montant_Apres_Conversion = q.MontantApres
FROM dbo.T_Appro_Details d
CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(COALESCE(d.Montant, d.BenTotal, d.PA * d.Qte, 0), COALESCE(d.Id_Monais, 1), COALESCE(d.Taux, 2800)) q;
GO

UPDATE e SET
    Montant = q.MontantNatif,
    Id_Monais = COALESCE(e.Id_Monais, e.CurrencyId, 1),
    Taux = COALESCE(e.Taux, 2800),
    Montant_Apres_Conversion = q.MontantApres
FROM dbo.T_Appro_Expense e
CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(COALESCE(e.Montant, e.AmountUSD, 0), COALESCE(e.Id_Monais, e.CurrencyId, 1), COALESCE(e.Taux, 2800)) q;
GO

UPDATE v SET
    Id_Monais = COALESCE(v.Id_Monais, 2),
    Taux = COALESCE(v.Taux, 2800),
    Montant_Apres_Conversion = q.MontantApres
FROM dbo.T_Versements v
CROSS APPLY dbo.fn_MonetaryQuatuorFromNative(v.Montant, COALESCE(v.Id_Monais, 2), COALESCE(v.Taux, 2800)) q;
GO

PRINT 'Migration_Monetary_Rounding_Enforcement terminée.';
GO
