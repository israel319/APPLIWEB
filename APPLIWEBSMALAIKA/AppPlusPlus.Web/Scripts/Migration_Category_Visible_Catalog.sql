-- ============================================================================
-- Colonne Visible_Catalog sur T_Art_Categorys (affichage accueil catalogue)
-- Idempotent — relançable sans casse.
--
-- IMPORTANT : exécuter sur la base cible (ex. db_acb256_glomyrashopping).
-- Local : sqlcmd -S localhost -d GlobalShoping -E -C -i Migration_Category_Visible_Catalog.sql
-- ============================================================================

-- USE db_acb256_glomyrashopping;
-- GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
PRINT '=== Migration Visible_Catalog ===';
GO

-- Batch 1 : ajout colonne (séparé du SELECT — sinon Msg 207 au compile)
IF COL_LENGTH('dbo.T_Art_Categorys', 'Visible_Catalog') IS NULL
BEGIN
    ALTER TABLE dbo.T_Art_Categorys
        ADD Visible_Catalog BIT NOT NULL
            CONSTRAINT DF_T_Art_Categorys_Visible_Catalog DEFAULT (1)
            WITH VALUES;
    PRINT 'Colonne Visible_Catalog ajoutée (défaut : visible).';
END
ELSE
    PRINT 'Colonne Visible_Catalog déjà présente.';
GO

-- Batch 2 : vérification (après création de la colonne)
IF COL_LENGTH('dbo.T_Art_Categorys', 'Visible_Catalog') IS NOT NULL
BEGIN
    SELECT Id_Category, Description_Category, Visible_Catalog
    FROM dbo.T_Art_Categorys
    ORDER BY Id_Category;
END
ELSE
    PRINT 'ERREUR : Visible_Catalog introuvable — vérifiez le nom de la table T_Art_Categorys.';
GO

PRINT '=== Fin migration Visible_Catalog ===';
GO
