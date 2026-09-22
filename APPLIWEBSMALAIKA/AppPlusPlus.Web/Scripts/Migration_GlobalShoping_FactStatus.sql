-- ============================================================================
-- Remap T_Facts.Status : ancien GlobalShoping → nouvelle application
-- ============================================================================
-- Ancien T_Status (GlobalShoping)     Nouveau code app    Libellé app
-- -------------------------------     ----------------    -----------
-- 1  Nouveau                          1                   Validée
-- 2  Livré                            1                   Validée
-- 3  Payé                             2                   Payée
-- 4  Livrée                           1                   Validée
-- 5  Reçu                             2                   Payée
-- 6  Annulé                           3                   Annulée
-- 7  Rejeté                           3                   Annulée
--
-- Sans ce remap, 66 001 factures « Payé » s'affichaient « Annulée ».
-- ============================================================================
USE GlobalShoping;
GO

SET NOCOUNT ON;

PRINT 'Distribution AVANT remap :';
SELECT Status, COUNT(*) AS Nb FROM dbo.T_Facts GROUP BY Status ORDER BY Status;
GO

UPDATE dbo.T_Facts
SET Status = CASE Status
    WHEN 1 THEN 1   -- Nouveau  → Validée
    WHEN 2 THEN 1   -- Livré    → Validée
    WHEN 3 THEN 2   -- Payé     → Payée
    WHEN 4 THEN 1   -- Livrée   → Validée
    WHEN 5 THEN 2   -- Reçu     → Payée
    WHEN 6 THEN 3   -- Annulé   → Annulée
    WHEN 7 THEN 3   -- Rejeté   → Annulée
    ELSE Status
END
WHERE Status BETWEEN 1 AND 7;
GO

PRINT CONCAT('Factures remappées : ', @@ROWCOUNT);
PRINT '';
PRINT 'Distribution APRÈS remap :';
SELECT Status, COUNT(*) AS Nb,
    CASE Status
        WHEN 0 THEN 'Brouillon'
        WHEN 1 THEN 'Validée'
        WHEN 2 THEN 'Payée'
        WHEN 3 THEN 'Annulée'
        ELSE 'Autre'
    END AS LibelleApp
FROM dbo.T_Facts
GROUP BY Status
ORDER BY Status;
GO
