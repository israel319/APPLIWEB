-- ============================================================================
-- MALAIKA KIDS — Purge GlobalShoping avant import inventaire Excel
--
-- Objectif : aucune donnée métier / catalogue existante ne doit rester.
--            Réinitialiser les colonnes IDENTITY pour repartir à zéro.
--
-- CONSERVÉ (configuration application) :
--   Rôles, permissions, fonctions, activités, modules, statuts, devises,
--   taux, mesures, monnaies, localisations (magasins), utilisateurs staff,
--   paramètres (T_AppSettings, T_Profile, T_InvoiceTemplate).
--
-- SUPPRIMÉ :
--   Toutes transactions, inventaire magasin, mouvements stock,
--   articles, stocks, catégories, marques, types article (re-seed minimal),
--   clients, fournisseurs, tables legacy d'import si présentes.
--
-- AVANT D'EXÉCUTER (obligatoire) :
--   1. BACKUP DATABASE [GlobalShoping] TO DISK = N'...\GlobalShoping_pre_malaika.bak' WITH INIT;
--   2. Arrêter le site / l'application IIS.
--
-- Usage local :
--   sqlcmd -S localhost -E -C -d GlobalShoping -i Migration_MalaikaKids_Purge_Avant_Import_Excel.sql
-- ============================================================================

USE GlobalShoping;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;

PRINT '=== Malaika Kids — purge avant import Excel ===';
PRINT CONCAT('Base : ', DB_NAME(), ' — ', CONVERT(VARCHAR(30), GETDATE(), 120));
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- Triggers inventaire (colonnes calculées / session) — désactivation temporaire
    IF OBJECT_ID('dbo.TR_T_Inventaire_Vente_RefreshSession', 'TR') IS NOT NULL
        DISABLE TRIGGER dbo.TR_T_Inventaire_Vente_RefreshSession ON dbo.T_Inventaire_Vente;
    IF OBJECT_ID('dbo.TR_T_Inventaire_Reception_Detail_RefreshTotal', 'TR') IS NOT NULL
        DISABLE TRIGGER dbo.TR_T_Inventaire_Reception_Detail_RefreshTotal ON dbo.T_Inventaire_Reception_Detail;
    IF OBJECT_ID('dbo.TR_T_Inventaire_Reception_RefreshSession', 'TR') IS NOT NULL
        DISABLE TRIGGER dbo.TR_T_Inventaire_Reception_RefreshSession ON dbo.T_Inventaire_Reception;

    -- ── Inventaire magasin (enfants d'abord) ────────────────────────────────
    IF OBJECT_ID('dbo.T_Inventaire_Vente', 'U') IS NOT NULL
        DELETE FROM dbo.T_Inventaire_Vente;
    IF OBJECT_ID('dbo.T_Inventaire_Reception_Detail', 'U') IS NOT NULL
        DELETE FROM dbo.T_Inventaire_Reception_Detail;
    IF OBJECT_ID('dbo.T_Inventaire_Reception', 'U') IS NOT NULL
        DELETE FROM dbo.T_Inventaire_Reception;
    IF OBJECT_ID('dbo.T_Inventaire_Magasin', 'U') IS NOT NULL
        DELETE FROM dbo.T_Inventaire_Magasin;

    -- ── Paiements & factures ────────────────────────────────────────────────
    IF OBJECT_ID('dbo.T_Payments', 'U') IS NOT NULL DELETE FROM dbo.T_Payments;
    IF OBJECT_ID('dbo.T_Fact_Details', 'U') IS NOT NULL DELETE FROM dbo.T_Fact_Details;
    IF OBJECT_ID('dbo.T_Facts', 'U') IS NOT NULL DELETE FROM dbo.T_Facts;

    -- ── Livraisons & commandes clients ──────────────────────────────────────
    IF OBJECT_ID('dbo.T_Livraison_Detail', 'U') IS NOT NULL DELETE FROM dbo.T_Livraison_Detail;
    IF OBJECT_ID('dbo.T_Livraison', 'U') IS NOT NULL DELETE FROM dbo.T_Livraison;
    IF OBJECT_ID('dbo.T_Commande_Details', 'U') IS NOT NULL DELETE FROM dbo.T_Commande_Details;
    IF OBJECT_ID('dbo.T_Commande', 'U') IS NOT NULL DELETE FROM dbo.T_Commande;

    -- ── Demandes génériques ─────────────────────────────────────────────────
    IF OBJECT_ID('dbo.T_Demande_Details', 'U') IS NOT NULL DELETE FROM dbo.T_Demande_Details;
    IF OBJECT_ID('dbo.T_Demandes', 'U') IS NOT NULL DELETE FROM dbo.T_Demandes;

    -- ── Approvisionnements & commandes internes ─────────────────────────────
    IF OBJECT_ID('dbo.T_Appro_Expense', 'U') IS NOT NULL DELETE FROM dbo.T_Appro_Expense;
    IF OBJECT_ID('dbo.T_Appro_Details', 'U') IS NOT NULL DELETE FROM dbo.T_Appro_Details;
    IF OBJECT_ID('dbo.T_Appros', 'U') IS NOT NULL DELETE FROM dbo.T_Appros;
    IF OBJECT_ID('dbo.T_Cmd_Details_Externe', 'U') IS NOT NULL DELETE FROM dbo.T_Cmd_Details_Externe;
    IF OBJECT_ID('dbo.T_Cmd_Details', 'U') IS NOT NULL DELETE FROM dbo.T_Cmd_Details;
    IF OBJECT_ID('dbo.T_Cmds', 'U') IS NOT NULL DELETE FROM dbo.T_Cmds;

    -- ── Prestations / services ──────────────────────────────────────────────
    IF OBJECT_ID('dbo.T_Service_Timesheets', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Timesheets;
    IF OBJECT_ID('dbo.T_Service_Deliverables', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Deliverables;
    IF OBJECT_ID('dbo.T_Service_Tasks', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Tasks;
    IF OBJECT_ID('dbo.T_Service_Projects', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Projects;
    IF OBJECT_ID('dbo.T_Service_Catalog', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Catalog;

    -- ── Stock & finance opérationnelle ────────────────────────────────────────
    IF OBJECT_ID('dbo.T_Mouvement_Stock', 'U') IS NOT NULL DELETE FROM dbo.T_Mouvement_Stock;
    IF OBJECT_ID('dbo.T_Transformation', 'U') IS NOT NULL DELETE FROM dbo.T_Transformation;
    IF OBJECT_ID('dbo.T_Audit', 'U') IS NOT NULL DELETE FROM dbo.T_Audit;
    IF OBJECT_ID('dbo.T_Versements', 'U') IS NOT NULL DELETE FROM dbo.T_Versements;

    IF OBJECT_ID('dbo.T_Stock', 'U') IS NOT NULL DELETE FROM dbo.T_Stock;
    IF OBJECT_ID('dbo.T_Arts', 'U') IS NOT NULL DELETE FROM dbo.T_Arts;

    -- ── Référentiels catalogue (recréés à l'import Excel) ───────────────────
    IF OBJECT_ID('dbo.T_Art_Categorys', 'U') IS NOT NULL DELETE FROM dbo.T_Art_Categorys;
    IF OBJECT_ID('dbo.T_Art_Marques', 'U') IS NOT NULL DELETE FROM dbo.T_Art_Marques;
    IF OBJECT_ID('dbo.T_Art_Types', 'U') IS NOT NULL DELETE FROM dbo.T_Art_Types;

    -- ── Tiers ───────────────────────────────────────────────────────────────
    IF OBJECT_ID('dbo.T_Customer', 'U') IS NOT NULL DELETE FROM dbo.T_Customer;
    IF OBJECT_ID('dbo.T_Supplier', 'U') IS NOT NULL DELETE FROM dbo.T_Supplier;

    -- ── Tables legacy / import anciennes (si présentes) ─────────────────────
    IF OBJECT_ID('dbo.T_Importations', 'U') IS NOT NULL DELETE FROM dbo.T_Importations;
    IF OBJECT_ID('dbo.T_Importation2', 'U') IS NOT NULL DELETE FROM dbo.T_Importation2;
    IF OBJECT_ID('dbo.items_shop', 'U') IS NOT NULL DELETE FROM dbo.items_shop;
    IF OBJECT_ID('dbo.Articles_', 'U') IS NOT NULL DELETE FROM dbo.Articles_;
    IF OBJECT_ID('dbo.customers', 'U') IS NOT NULL DELETE FROM dbo.customers;

    IF OBJECT_ID('dbo.TR_T_Inventaire_Vente_RefreshSession', 'TR') IS NOT NULL
        ENABLE TRIGGER dbo.TR_T_Inventaire_Vente_RefreshSession ON dbo.T_Inventaire_Vente;
    IF OBJECT_ID('dbo.TR_T_Inventaire_Reception_Detail_RefreshTotal', 'TR') IS NOT NULL
        ENABLE TRIGGER dbo.TR_T_Inventaire_Reception_Detail_RefreshTotal ON dbo.T_Inventaire_Reception_Detail;
    IF OBJECT_ID('dbo.TR_T_Inventaire_Reception_RefreshSession', 'TR') IS NOT NULL
        ENABLE TRIGGER dbo.TR_T_Inventaire_Reception_RefreshSession ON dbo.T_Inventaire_Reception;

    COMMIT TRANSACTION;
    PRINT 'OK — Données métier et catalogue supprimées.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    IF OBJECT_ID('dbo.TR_T_Inventaire_Vente_RefreshSession', 'TR') IS NOT NULL
        ENABLE TRIGGER dbo.TR_T_Inventaire_Vente_RefreshSession ON dbo.T_Inventaire_Vente;
    IF OBJECT_ID('dbo.TR_T_Inventaire_Reception_Detail_RefreshTotal', 'TR') IS NOT NULL
        ENABLE TRIGGER dbo.TR_T_Inventaire_Reception_Detail_RefreshTotal ON dbo.T_Inventaire_Reception_Detail;
    IF OBJECT_ID('dbo.TR_T_Inventaire_Reception_RefreshSession', 'TR') IS NOT NULL
        ENABLE TRIGGER dbo.TR_T_Inventaire_Reception_RefreshSession ON dbo.T_Inventaire_Reception;
    THROW;
END CATCH
GO

-- ── Réinitialisation IDENTITY (index numériques à zéro) ─────────────────────
DECLARE @Reseed TABLE (TableName SYSNAME PRIMARY KEY);
INSERT INTO @Reseed (TableName) VALUES
    (N'dbo.T_Inventaire_Vente'),
    (N'dbo.T_Inventaire_Reception_Detail'),
    (N'dbo.T_Inventaire_Reception'),
    (N'dbo.T_Inventaire_Magasin'),
    (N'dbo.T_Payments'),
    (N'dbo.T_Fact_Details'),
    (N'dbo.T_Facts'),
    (N'dbo.T_Livraison_Detail'),
    (N'dbo.T_Livraison'),
    (N'dbo.T_Commande_Details'),
    (N'dbo.T_Commande'),
    (N'dbo.T_Demande_Details'),
    (N'dbo.T_Demandes'),
    (N'dbo.T_Appro_Expense'),
    (N'dbo.T_Appro_Details'),
    (N'dbo.T_Appros'),
    (N'dbo.T_Cmd_Details'),
    (N'dbo.T_Cmds'),
    (N'dbo.T_Mouvement_Stock'),
    (N'dbo.T_Transformation'),
    (N'dbo.T_Audit'),
    (N'dbo.T_Versements'),
    (N'dbo.T_Stock'),
    (N'dbo.T_Arts'),
    (N'dbo.T_Art_Categorys'),
    (N'dbo.T_Art_Marques'),
    (N'dbo.T_Art_Types'),
    (N'dbo.T_Customer'),
    (N'dbo.T_Supplier');

DECLARE @t SYSNAME, @sql NVARCHAR(400);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT TableName FROM @Reseed;
OPEN c;
FETCH NEXT FROM c INTO @t;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(@t, 'U') IS NOT NULL
       AND EXISTS (
            SELECT 1 FROM sys.identity_columns ic
            WHERE ic.object_id = OBJECT_ID(@t)
       )
    BEGIN
        SET @sql = N'DBCC CHECKIDENT (' + QUOTENAME(@t, '''') + N', RESEED, 0);';
        EXEC sp_executesql @sql;
        PRINT CONCAT('RESEED ', @t);
    END
    FETCH NEXT FROM c INTO @t;
END
CLOSE c;
DEALLOCATE c;
GO

-- ── Types article minimaux (commandes en ligne + segmentation Excel) ───────
IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'A commander')
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'A commander');
IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'Non a commander')
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'Non a commander');
IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'Babies')
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'Babies');
IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'Enfants')
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'Enfants');
GO

-- ── Marque générique pour l'import ──────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Marques)
    INSERT INTO dbo.T_Art_Marques (Description_Marque) VALUES (N'Malaika Kids');
GO

-- ── Contrôle post-purge ─────────────────────────────────────────────────────
SELECT 'T_Arts' AS [Table], COUNT(*) AS Lignes FROM dbo.T_Arts
UNION ALL SELECT 'T_Stock', COUNT(*) FROM dbo.T_Stock
UNION ALL SELECT 'T_Art_Categorys', COUNT(*) FROM dbo.T_Art_Categorys
UNION ALL SELECT 'T_Facts', COUNT(*) FROM dbo.T_Facts
UNION ALL SELECT 'T_Commande', COUNT(*) FROM dbo.T_Commande
UNION ALL SELECT 'T_Mouvement_Stock', COUNT(*) FROM dbo.T_Mouvement_Stock
UNION ALL SELECT 'T_Inventaire_Magasin', COUNT(*) FROM dbo.T_Inventaire_Magasin
UNION ALL SELECT 'T_Customer', COUNT(*) FROM dbo.T_Customer
UNION ALL SELECT 'T_Localisations (conservé)', COUNT(*) FROM dbo.T_Localisations
UNION ALL SELECT 'T_Users (conservé)', COUNT(*) FROM dbo.T_Users;

SELECT Id_Type, Description_Type FROM dbo.T_Art_Types ORDER BY Id_Type;

PRINT '=== Fin purge — prêt pour import Excel Malaika Kids ===';
GO
