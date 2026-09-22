-- ============================================================================
-- Image optionnelle sur les catégories (T_Art_Categorys)
-- Idempotent — relançable sans casse.
--
-- Local : sqlcmd -S localhost -d GlobalShoping -E -C -i Migration_Category_Image.sql
-- ============================================================================

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
PRINT '=== Migration Image_Category ===';
GO

IF COL_LENGTH('dbo.T_Art_Categorys', 'Image_Category') IS NULL
BEGIN
    ALTER TABLE dbo.T_Art_Categorys ADD Image_Category NVARCHAR(MAX) NULL;
    PRINT 'Colonne Image_Category ajoutée.';
END
ELSE
    PRINT 'Colonne Image_Category déjà présente.';
GO

IF COL_LENGTH('dbo.T_Art_Categorys', 'Image_Category') IS NOT NULL
BEGIN
    SELECT Id_Category, Description_Category,
           CASE WHEN Image_Category IS NULL OR Image_Category = '' THEN '(aucune)' ELSE '(image)' END AS Image_Category
    FROM dbo.T_Art_Categorys
    ORDER BY Id_Category;
END
GO

PRINT '=== Fin migration Image_Category ===';
GO
