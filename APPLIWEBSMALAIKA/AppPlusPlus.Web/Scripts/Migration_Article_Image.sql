-- ============================================================================
-- Champ image optionnel sur les articles (T_Arts)
-- Exécuter sur la base GlobalShoping
-- ============================================================================
USE GlobalShoping;
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF COL_LENGTH('dbo.T_Arts', 'Image_Article') IS NULL
BEGIN
    ALTER TABLE dbo.T_Arts ADD Image_Article NVARCHAR(MAX) NULL;
    PRINT 'Colonne Image_Article ajoutée à T_Arts.';
END
ELSE
BEGIN
    PRINT 'Colonne Image_Article déjà présente sur T_Arts.';
END
GO
