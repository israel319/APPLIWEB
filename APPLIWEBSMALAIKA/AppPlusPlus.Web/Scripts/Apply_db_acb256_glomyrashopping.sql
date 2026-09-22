-- Migrations catalogue — base Glomyra hébergée
-- Idempotent — relançable sans casse.

USE db_acb256_glomyrashopping;
GO

IF COL_LENGTH('dbo.T_Arts', 'Image_Article') IS NULL
BEGIN
    ALTER TABLE dbo.T_Arts ADD Image_Article NVARCHAR(MAX) NULL;
    PRINT 'OK — Colonne Image_Article ajoutée.';
END
ELSE
    PRINT 'OK — Image_Article déjà présente.';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'A commander')
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'A commander');
IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'Non a commander')
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'Non a commander');
GO

PRINT '=== Fin Apply Glomyra ===';
GO
