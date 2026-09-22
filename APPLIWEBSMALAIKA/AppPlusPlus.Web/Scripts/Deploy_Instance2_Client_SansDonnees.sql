-- ============================================================================
-- ÉTAPE B — INSTANCE 2 : CLIENT VIERGE (admin seul, pas de données métier)
-- ============================================================================
--
-- VOUS AVEZ :
--   • Un backup / script SQL « SANS DONNÉES » (schéma + référentiels minimaux)
--   • Vous restaurez puis RENOMMEZ la base :
--       MasterGlobalShopping
--
-- CE SCRIPT FAIT :
--   • Nettoie toute donnée métier résiduelle (au cas où)
--   • Supprime articles, stocks, clients, utilisateurs sauf admin
--   • Crée / réinitialise admin (login: admin, mot de passe: 1234)
--   • Assure les types A commander / Non a commander
--
-- AVANT :
--   1. BACKUP de la base cible
--   2. Arrêter l'application IIS / site web
--   3. Modifier la ligne USE ci-dessous
--
-- LANCER :
--   sqlcmd -S VOTRE_SERVEUR -E -C -d MasterGlobalShopping -i Deploy_Instance2_Client_SansDonnees.sql
--
-- APRÈS :
--   • appsettings de l'instance CLIENT → DefaultConnection pointe vers cette base
--   • Connexion staff : admin / 1234 (changer le mot de passe tout de suite)
--   • Accueil public : « Aucun article commandable » jusqu'à configuration
-- ============================================================================

USE MasterGlobalShopping;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '=== INSTANCE 2 — Client vierge (backup sans données → admin seul) ===';
PRINT CONCAT('Base : ', DB_NAME());
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- Sécurité : purge transactionnelle si des lignes ont été importées par erreur
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

    IF OBJECT_ID('dbo.T_Stock', 'U') IS NOT NULL DELETE FROM dbo.T_Stock;
    IF OBJECT_ID('dbo.T_Arts', 'U') IS NOT NULL DELETE FROM dbo.T_Arts;
    IF OBJECT_ID('dbo.T_Supplier', 'U') IS NOT NULL DELETE FROM dbo.T_Supplier;

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
    PRINT 'OK — Données métier supprimées, utilisateurs réduits à admin.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO

IF COL_LENGTH('dbo.T_Arts', 'Image_Article') IS NULL
    ALTER TABLE dbo.T_Arts ADD Image_Article NVARCHAR(MAX) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'A commander')
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'A commander');
IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'Non a commander')
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'Non a commander');
GO

-- ── Référentiels minimaux (backup « sans données » souvent vide) ─────────────
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
        PRINT 'Seed : T_Roles (5 rôles standards).';
    END
    ELSE IF NOT EXISTS (SELECT 1 FROM dbo.T_Roles WHERE Description_Role = N'Administrateur')
    BEGIN
        INSERT INTO dbo.T_Roles (Description_Role, IsActive) VALUES (N'Administrateur', 1);
        PRINT 'Seed : rôle Administrateur ajouté.';
    END
END
ELSE
BEGIN
    RAISERROR('ERREUR : table T_Roles absente — le backup doit contenir au moins le schéma.', 16, 1);
END
GO

IF OBJECT_ID('dbo.T_Customer_Type', 'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.T_Customer_Type)
BEGIN
    SET IDENTITY_INSERT dbo.T_Customer_Type ON;
    INSERT INTO dbo.T_Customer_Type (CustomerTypeId, Description) VALUES
        (0, N'Anonyme'), (1, N'Particulier'), (2, N'Entreprise');
    SET IDENTITY_INSERT dbo.T_Customer_Type OFF;
    PRINT 'Seed : T_Customer_Type.';
END
GO

DECLARE @RoleId INT = (
    SELECT TOP 1 RoleId FROM dbo.T_Roles
    WHERE Description_Role = N'Administrateur' OR RoleId = 1
    ORDER BY RoleId
);

IF @RoleId IS NULL
BEGIN
    RAISERROR('ERREUR : rôle Administrateur absent — le backup « sans données » doit contenir T_Roles.', 16, 1);
END
ELSE IF EXISTS (SELECT 1 FROM dbo.T_Users WHERE login = N'admin')
BEGIN
    UPDATE dbo.T_Users
    SET Password = N'1234', Name = N'Administrateur', Activated = 1, RoleId = @RoleId
    WHERE login = N'admin';
    PRINT 'Utilisateur admin mis à jour (admin / 1234).';
END
ELSE
BEGIN
    INSERT INTO dbo.T_Users (login, Password, Name, Email, Activated, RoleId)
    VALUES (N'admin', N'1234', N'Administrateur', NULL, 1, @RoleId);
    PRINT 'Utilisateur admin créé (admin / 1234).';
END
GO

PRINT '--- Contrôle instance 2 ---';
SELECT 'Utilisateurs' AS Element, COUNT(*) AS Valeur FROM dbo.T_Users
UNION ALL SELECT 'Articles', COUNT(*) FROM dbo.T_Arts
UNION ALL SELECT 'Commandes', COUNT(*) FROM dbo.T_Commande
UNION ALL SELECT 'Clients', COUNT(*) FROM dbo.T_Customer
UNION ALL SELECT 'Factures', COUNT(*) FROM dbo.T_Facts;

SELECT login, Name, RoleId FROM dbo.T_Users;
GO

PRINT '=== FIN INSTANCE 2 — Prêt pour configuration client ===';
GO
