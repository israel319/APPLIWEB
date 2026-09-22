-- Backfill : Qte_max = 100 pour tous les stocks + défaut colonne
USE GlobalShoping;
GO
SET NOCOUNT ON;

UPDATE T_Stock SET Qte_max = 100;
PRINT CONCAT('T_Stock : ', @@ROWCOUNT, ' ligne(s) mises à Qte_max = 100.');
GO

IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_T_Stock_Qte_max')
    ALTER TABLE T_Stock DROP CONSTRAINT DF_T_Stock_Qte_max;
GO

ALTER TABLE T_Stock ADD CONSTRAINT DF_T_Stock_Qte_max DEFAULT 100 FOR Qte_max;
PRINT 'Défaut colonne Qte_max = 100 appliqué.';
GO
