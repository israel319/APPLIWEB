-- ============================================================================
-- Types article : visibilité catalogue commandes en ligne
--   A commander      → article visible et commandable sur l'accueil public
--   Non a commander  → article masqué du catalogue public
--
-- Base locale : GlobalShoping
-- Base hébergée : voir Apply_db_acb256_globalshoping.sql
-- ============================================================================

USE GlobalShoping;
GO

SET NOCOUNT ON;
PRINT '=== Migration types article — catalogue commandes ===';

DECLARE @typeCount INT = (SELECT COUNT(*) FROM dbo.T_Art_Types);

IF @typeCount = 0
BEGIN
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'A commander');
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'Non a commander');
    PRINT '2 types créés : A commander / Non a commander.';
END
ELSE IF @typeCount = 1
BEGIN
    UPDATE dbo.T_Art_Types
    SET Description_Type = N'A commander'
    WHERE Id_Type = (SELECT MIN(Id_Type) FROM dbo.T_Art_Types);

    INSERT INTO dbo.T_Art_Types (Description_Type)
    SELECT N'Non a commander'
    WHERE NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'Non a commander');

    PRINT 'Type existant renommé + type Non a commander ajouté si absent.';
END
ELSE
BEGIN
    ;WITH ranked AS (
        SELECT Id_Type,
               ROW_NUMBER() OVER (ORDER BY Id_Type) AS rn,
               COUNT(*) OVER () AS total
        FROM dbo.T_Art_Types
    )
    UPDATE t
    SET Description_Type = CASE
        WHEN r.rn = 1 THEN N'A commander'
        WHEN r.rn = 2 THEN N'Non a commander'
        ELSE t.Description_Type
    END
    FROM dbo.T_Art_Types t
    INNER JOIN ranked r ON r.Id_Type = t.Id_Type
    WHERE r.total >= 2 AND r.rn <= 2;

    IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'A commander')
        INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'A commander');

    IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'Non a commander')
        INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'Non a commander');

    PRINT CONCAT('Types mis à jour (', @typeCount, ' ligne(s) en base).');
END

SELECT Id_Type, Description_Type
FROM dbo.T_Art_Types
ORDER BY Id_Type;

PRINT '=== Fin migration types article ===';
GO
