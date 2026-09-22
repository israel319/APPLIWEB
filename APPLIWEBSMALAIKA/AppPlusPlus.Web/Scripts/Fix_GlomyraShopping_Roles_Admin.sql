-- ============================================================================
-- CORRECTIF — Glomyra (démo) : rôles + utilisateur admin
-- Base hébergée : db_acb256_glomyrashopping
--
-- Connexion application après exécution :
--   login    : admin
--   mot de passe : 1234
--
-- Idempotent — relançable sans risque.
-- ============================================================================

USE db_acb256_glomyrashopping;
GO

SET NOCOUNT ON;

PRINT '=== Glomyra — rôles + admin ===';
PRINT CONCAT('Base : ', DB_NAME());
GO

-- ── Rôles standards ─────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.T_Roles', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.T_Roles)
    BEGIN
        SET IDENTITY_INSERT dbo.T_Roles ON;
        INSERT INTO dbo.T_Roles (RoleId, Description_Role, IsActive) VALUES
            (1, N'Administrateur', 1),
            (2, N'Gérant', 1),
            (3, N'Vendeur', 1),
            (4, N'Caissier', 1),
            (5, N'Magasinier', 1);
        SET IDENTITY_INSERT dbo.T_Roles OFF;
        PRINT 'OK — 5 rôles standards créés.';
    END
    ELSE IF NOT EXISTS (SELECT 1 FROM dbo.T_Roles WHERE Description_Role = N'Administrateur')
    BEGIN
        INSERT INTO dbo.T_Roles (Description_Role, IsActive) VALUES (N'Administrateur', 1);
        PRINT 'OK — rôle Administrateur ajouté.';
    END
    ELSE
        PRINT 'OK — rôle Administrateur déjà présent.';
END
ELSE
BEGIN
    RAISERROR('Table T_Roles absente — le schéma de la base est incomplet.', 16, 1);
END
GO

-- ── Utilisateur admin ───────────────────────────────────────────────────────
DECLARE @RoleId INT = (
    SELECT TOP 1 RoleId FROM dbo.T_Roles
    WHERE Description_Role = N'Administrateur' OR RoleId = 1
    ORDER BY RoleId
);

IF @RoleId IS NULL
BEGIN
    RAISERROR('Impossible de résoudre le rôle Administrateur.', 16, 1);
END
ELSE IF EXISTS (SELECT 1 FROM dbo.T_Users WHERE login = N'admin')
BEGIN
    UPDATE dbo.T_Users
    SET Password = N'1234',
        Name = N'Administrateur',
        Activated = 1,
        RoleId = @RoleId
    WHERE login = N'admin';
    PRINT 'OK — admin mis à jour (admin / 1234).';
END
ELSE
BEGIN
    INSERT INTO dbo.T_Users (login, Password, Name, Email, Activated, RoleId)
    VALUES (N'admin', N'1234', N'Administrateur', NULL, 1, @RoleId);
    PRINT 'OK — admin créé (admin / 1234).';
END
GO

-- ── Contrôle ────────────────────────────────────────────────────────────────
SELECT RoleId, Description_Role, IsActive FROM dbo.T_Roles ORDER BY RoleId;
SELECT login, Name, RoleId, Activated FROM dbo.T_Users ORDER BY login;
GO

PRINT '=== Fin — connectez-vous sur /login avec admin / 1234 ===';
GO
