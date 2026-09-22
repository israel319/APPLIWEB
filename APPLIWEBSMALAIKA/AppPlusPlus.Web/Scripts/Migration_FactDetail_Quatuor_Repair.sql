-- Réparation quatuor monétaire T_Fact_Details / T_Facts
-- Prérequis : Migration_Monetary_Rounding_Enforcement.sql (fonctions fn_MonetaryQuatuor*)
USE GlobalShoping;
GO

SET NOCOUNT ON;
GO

UPDATE fd
SET
    Montant = q.MontantNatif,
    Id_Monais = COALESCE(fd.Id_Monais, f.Id_Monais, 2),
    Taux = COALESCE(fd.Taux, f.Taux, 2800),
    Montant_Apres_Conversion = q.MontantApres
FROM dbo.T_Fact_Details fd
INNER JOIN dbo.T_Facts f ON f.Id = fd.Id_Fact
CROSS APPLY dbo.fn_MonetaryQuatuorFactDetail(
    fd.Pu, fd.Qte, fd.Montant,
    COALESCE(fd.Id_Monais, f.Id_Monais, 2),
    COALESCE(fd.Taux, f.Taux, 2800)) q
WHERE fd.Montant IS NULL
   OR fd.Id_Monais IS NULL
   OR fd.Taux IS NULL
   OR fd.Montant_Apres_Conversion IS NULL;
GO

UPDATE f
SET
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
    COALESCE(f.Taux, 2800)) q
WHERE f.Montant IS NULL OR f.Montant_Apres_Conversion IS NULL;
GO

PRINT 'Réparation quatuor factures terminée.';
GO
