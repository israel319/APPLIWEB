/*
  Migration devise unique : Franc Congolais (FC) comme devise de stockage.
  Convertit les montants historiquement enregistrés en USD vers FC via le taux de la ligne.

  IMPORTANT : exécuter une sauvegarde avant application.
  Après migration, le code applicatif enregistre uniquement en FC.
*/
SET NOCOUNT ON;

DECLARE @DefaultTaux decimal(18,2) = 2800;
IF EXISTS (SELECT 1 FROM T_Taux)
    SELECT TOP 1 @DefaultTaux = Taux FROM T_Taux ORDER BY Id DESC;

PRINT 'Taux de référence migration : ' + CAST(@DefaultTaux AS varchar(20));

-- Approvisionnements : PA, PV, Ben, BenTotal (legacy USD → FC)
UPDATE d SET
    PA = CASE WHEN d.PA IS NOT NULL AND d.PA > 0 AND d.PA < 500 THEN CEILING(d.PA * COALESCE(NULLIF(d.Taux, 0), NULLIF(a.Taux, 0), @DefaultTaux)) ELSE d.PA END,
    PV = CASE WHEN d.PV IS NOT NULL AND d.PV > 0 AND d.PV < 500 THEN CEILING(d.PV * COALESCE(NULLIF(d.Taux, 0), NULLIF(a.Taux, 0), @DefaultTaux)) ELSE d.PV END,
    Ben = CASE WHEN d.Ben IS NOT NULL AND d.Ben <> 0 AND ABS(d.Ben) < 500 THEN CEILING(d.Ben * COALESCE(NULLIF(d.Taux, 0), NULLIF(a.Taux, 0), @DefaultTaux)) ELSE d.Ben END,
    BenTotal = CASE WHEN d.BenTotal IS NOT NULL AND d.BenTotal <> 0 AND ABS(d.BenTotal) < 50000 THEN CEILING(d.BenTotal * COALESCE(NULLIF(d.Taux, 0), NULLIF(a.Taux, 0), @DefaultTaux)) ELSE d.BenTotal END,
    Id_Monais = 2,
    Montant = CASE WHEN d.Montant IS NOT NULL AND d.Montant <> 0 AND d.Id_Monais = 1 THEN CEILING(d.Montant * COALESCE(NULLIF(d.Taux, 0), @DefaultTaux)) ELSE d.Montant END,
    Montant_Apres_Conversion = CASE WHEN d.Montant_Apres_Conversion IS NOT NULL AND d.Id_Monais = 2 THEN d.Montant_Apres_Conversion
        WHEN d.Montant IS NOT NULL AND d.Taux > 0 THEN ROUND(d.Montant / d.Taux, 2) ELSE d.Montant_Apres_Conversion END
FROM T_Appro_Details d
INNER JOIN T_Appros a ON a.Id = d.Id_Appro;

UPDATE a SET
    PA = CASE WHEN a.PA IS NOT NULL AND a.PA > 0 AND a.PA < 500 THEN CEILING(a.PA * COALESCE(NULLIF(a.Taux, 0), @DefaultTaux)) ELSE a.PA END,
    PV = CASE WHEN a.PV IS NOT NULL AND a.PV > 0 AND a.PV < 500 THEN CEILING(a.PV * COALESCE(NULLIF(a.Taux, 0), @DefaultTaux)) ELSE a.PV END,
    Ben = CASE WHEN a.Ben IS NOT NULL AND a.Ben <> 0 AND ABS(a.Ben) < 500 THEN CEILING(a.Ben * COALESCE(NULLIF(a.Taux, 0), @DefaultTaux)) ELSE a.Ben END,
    BenTotal = CASE WHEN a.BenTotal IS NOT NULL AND a.BenTotal <> 0 AND ABS(a.BenTotal) < 50000 THEN CEILING(a.BenTotal * COALESCE(NULLIF(a.Taux, 0), @DefaultTaux)) ELSE a.BenTotal END,
    Id_Monais = 2
FROM T_Appros a;

-- Catalogue articles : prix USD → FC
UPDATE art SET
    Price = CEILING(art.Price * @DefaultTaux),
    Id_Monais = 2
FROM T_Arts art
WHERE art.Id_Monais = 1 AND art.Price > 0 AND art.Price < 500;

PRINT 'Migration devise FC terminée.';
GO
