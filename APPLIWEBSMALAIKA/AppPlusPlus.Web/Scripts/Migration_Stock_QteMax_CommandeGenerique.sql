-- ============================================================================
-- Migration : Qte_max sur T_Stock + localisation sur commandes internes
-- Permet les commandes génériques de réappro par localisation
-- Database: GlobalShoping (ajuster USE si nécessaire)
-- ============================================================================

USE GlobalShoping;
GO
SET NOCOUNT ON;

-- ── Qte_max sur T_Stock ──
IF COL_LENGTH('T_Stock', 'Qte_max') IS NULL
BEGIN
    ALTER TABLE T_Stock ADD Qte_max INT NOT NULL CONSTRAINT DF_T_Stock_Qte_max DEFAULT 100;
    PRINT 'Colonne Qte_max ajoutée à T_Stock.';
END
ELSE
    PRINT 'Colonne Qte_max existe déjà sur T_Stock.';
GO

-- ── Localisation cible sur commandes internes génériques ──
IF COL_LENGTH('T_Cmds', 'Id_Localisation') IS NULL
BEGIN
    ALTER TABLE T_Cmds ADD Id_Localisation INT NULL;
    PRINT 'Colonne Id_Localisation ajoutée à T_Cmds.';
END
ELSE
    PRINT 'Colonne Id_Localisation existe déjà sur T_Cmds.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_T_Cmds_T_Localisations'
)
BEGIN
    ALTER TABLE T_Cmds
        ADD CONSTRAINT FK_T_Cmds_T_Localisations
        FOREIGN KEY (Id_Localisation) REFERENCES T_Localisations(Id_Localisation);
    PRINT 'FK FK_T_Cmds_T_Localisations créée.';
END
GO

PRINT 'Migration Qte_max / commande générique terminée.';
GO
