-- ============================================================================
-- INSTANCE 2 — VIERGE (démarrage client)
-- Objectif : aucune donnée métier, uniquement la structure + référentiels
--            minimaux + un seul utilisateur admin.
--
-- CONSERVER : rôles, permissions, fonctions, activités, modules, devises,
--             taux, statuts, types article (A commander / Non a commander),
--             paramètres application.
--
-- SUPPRIMER : articles, stocks, clients, fournisseurs, utilisateurs (sauf admin),
--             toutes transactions.
--
-- AVANT D'EXÉCUTER :
--   BACKUP DATABASE [VOTRE_BASE] TO DISK = N'C:\Temp\backup_pre_empty.bak' WITH INIT;
--   Arrêter l'application.
--
-- Usage :
--   sqlcmd -S SERVEUR -d VOTRE_BASE -i Instance02_Empty_AdminOnly.sql
-- ============================================================================

-- ► Adapter le nom de la base ci-dessous
USE GlobalShoping;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '=== Instance 2 : reset vierge (admin seul) ===';
PRINT CONCAT('Base : ', DB_NAME(), ' — ', CONVERT(VARCHAR(30), GETDATE(), 120));
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- ── Même purge transactionnelle que l'instance démo ─────────────────────
    IF OBJECT_ID('dbo.T_Payments', 'U') IS NOT NULL DELETE FROM dbo.T_Payments;
    IF OBJECT_ID('dbo.T_Fact_Details', 'U') IS NOT NULL DELETE FROM dbo.T_Fact_Details;
    IF OBJECT_ID('dbo.T_Facts', 'U') IS NOT NULL DELETE FROM dbo.T_Facts;
    IF OBJECT_ID('dbo.T_Livraison_Detail', 'U') IS NOT NULL DELETE FROM dbo.T_Livraison_Detail;
    IF OBJECT_ID('dbo.T_Livraison', 'U') IS NOT NULL DELETE FROM dbo.T_Livraison;
    IF OBJECT_ID('dbo.T_Commande_Details', 'U') IS NOT NULL DELETE FROM dbo.T_Commande_Details;
    IF OBJECT_ID('dbo.T_Commande', 'U') IS NOT NULL DELETE FROM dbo.T_Commande;
    IF OBJECT_ID('dbo.T_Demande_Details', 'U') IS NOT NULL DELETE FROM dbo.T_Demande_Details;
    IF OBJECT_ID('dbo.T_Demandes', 'U') IS NOT NULL DELETE FROM dbo.T_Demandes;
    IF OBJECT_ID('dbo.T_Appro_Expense', 'U') IS NOT NULL DELETE FROM dbo.T_Appro_Expense;
    IF OBJECT_ID('dbo.T_Appro_Details', 'U') IS NOT NULL DELETE FROM dbo.T_Appro_Details;
    IF OBJECT_ID('dbo.T_Appros', 'U') IS NOT NULL DELETE FROM dbo.T_Appros;
    IF OBJECT_ID('dbo.T_Cmd_Details', 'U') IS NOT NULL DELETE FROM dbo.T_Cmd_Details;
    IF OBJECT_ID('dbo.T_Cmds', 'U') IS NOT NULL DELETE FROM dbo.T_Cmds;
    IF OBJECT_ID('dbo.T_Service_Timesheets', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Timesheets;
    IF OBJECT_ID('dbo.T_Service_Deliverables', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Deliverables;
    IF OBJECT_ID('dbo.T_Service_Tasks', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Tasks;
    IF OBJECT_ID('dbo.T_Service_Projects', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Projects;
    IF OBJECT_ID('dbo.T_Service_Catalog', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Catalog;
    IF OBJECT_ID('dbo.T_Mouvement_Stock', 'U') IS NOT NULL DELETE FROM dbo.T_Mouvement_Stock;
    IF OBJECT_ID('dbo.T_Transformation', 'U') IS NOT NULL DELETE FROM dbo.T_Transformation;
    IF OBJECT_ID('dbo.T_Audit', 'U') IS NOT NULL DELETE FROM dbo.T_Audit;
    IF OBJECT_ID('dbo.T_Versements', 'U') IS NOT NULL DELETE FROM dbo.T_Versements;
    IF OBJECT_ID('dbo.T_Customer', 'U') IS NOT NULL DELETE FROM dbo.T_Customer;

    -- ── Catalogue & stock (accueil public vide) ─────────────────────────────
    IF OBJECT_ID('dbo.T_Stock', 'U') IS NOT NULL DELETE FROM dbo.T_Stock;
    IF OBJECT_ID('dbo.T_Arts', 'U') IS NOT NULL DELETE FROM dbo.T_Arts;

    -- ── Fournisseurs (optionnel — pas nécessaires au démarrage) ─────────────
    IF OBJECT_ID('dbo.T_Supplier', 'U') IS NOT NULL DELETE FROM dbo.T_Supplier;

    -- ── Utilisateurs : ne garder que admin ──────────────────────────────────
    IF OBJECT_ID('dbo.T_User_Activities', 'U') IS NOT NULL
        DELETE FROM dbo.T_User_Activities WHERE UserLogin <> N'admin';

    IF OBJECT_ID('dbo.T_User_Localisations', 'U') IS NOT NULL
        DELETE FROM dbo.T_User_Localisations WHERE Id_User <> N'admin';

    IF OBJECT_ID('dbo.T_User_Fonctions', 'U') IS NOT NULL
        DELETE FROM dbo.T_User_Fonctions WHERE [User] <> N'admin';

    IF OBJECT_ID('dbo.T_Users_Caisse', 'U') IS NOT NULL
        DELETE FROM dbo.T_Users_Caisse WHERE [User] <> N'admin';

    IF OBJECT_ID('dbo.T_Users', 'U') IS NOT NULL
        DELETE FROM dbo.T_Users WHERE login <> N'admin';

    COMMIT TRANSACTION;
    PRINT 'Données métier supprimées.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO

-- ── Types article commandes en ligne ────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'A commander')
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'A commander');
IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'Non a commander')
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'Non a commander');
GO

-- ── Utilisateur admin (login: admin / mot de passe: 1234) ───────────────────
DECLARE @RoleId INT = (
    SELECT TOP 1 RoleId FROM dbo.T_Roles
    WHERE Description_Role = N'Administrateur' OR RoleId = 1
    ORDER BY RoleId
);

IF @RoleId IS NULL
    RAISERROR('Rôle Administrateur introuvable dans T_Roles.', 16, 1);
ELSE IF EXISTS (SELECT 1 FROM dbo.T_Users WHERE login = N'admin')
    UPDATE dbo.T_Users
    SET Password = N'1234', Name = N'Administrateur', Activated = 1, RoleId = @RoleId
    WHERE login = N'admin';
ELSE
    INSERT INTO dbo.T_Users (login, Password, Name, Email, Activated, RoleId)
    VALUES (N'admin', N'1234', N'Administrateur', NULL, 1, @RoleId);
GO

-- ── Contrôle ────────────────────────────────────────────────────────────────
SELECT 'T_Users' AS [Table], COUNT(*) AS Lignes FROM dbo.T_Users
UNION ALL SELECT 'T_Arts', COUNT(*) FROM dbo.T_Arts
UNION ALL SELECT 'T_Commande', COUNT(*) FROM dbo.T_Commande
UNION ALL SELECT 'T_Customer', COUNT(*) FROM dbo.T_Customer
UNION ALL SELECT 'T_Facts', COUNT(*) FROM dbo.T_Facts;

SELECT login, Name, RoleId FROM dbo.T_Users;

PRINT '=== Fin Instance 2 — admin seul (admin / 1234) ===';
GO
