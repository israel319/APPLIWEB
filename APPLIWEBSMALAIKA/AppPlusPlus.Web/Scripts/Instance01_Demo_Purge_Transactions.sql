-- ============================================================================
-- INSTANCE 1 — DÉMO / VITRINE
-- Objectif : garder le catalogue et la configuration, supprimer toutes les
--            opérations (ventes, commandes, stocks mouvementés, clients, etc.)
--
-- CONSERVER : articles, catégories, types, localisations, utilisateurs staff,
--             rôles, permissions, taux, paramètres, stocks à 0.
--
-- AVANT D'EXÉCUTER :
--   BACKUP DATABASE [VOTRE_BASE] TO DISK = N'C:\Temp\backup_pre_demo.bak' WITH INIT;
--   Arrêter l'application pendant le script.
--
-- Usage :
--   sqlcmd -S SERVEUR -d VOTRE_BASE -i Instance01_Demo_Purge_Transactions.sql
-- ============================================================================

-- ► Adapter le nom de la base ci-dessous
USE GlobalShoping;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '=== Instance 1 : purge des transactions (mode démo) ===';
PRINT CONCAT('Base : ', DB_NAME(), ' — ', CONVERT(VARCHAR(30), GETDATE(), 120));
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- ── Paiements & factures ────────────────────────────────────────────────
    IF OBJECT_ID('dbo.T_Payments', 'U') IS NOT NULL DELETE FROM dbo.T_Payments;
    IF OBJECT_ID('dbo.T_Fact_Details', 'U') IS NOT NULL DELETE FROM dbo.T_Fact_Details;
    IF OBJECT_ID('dbo.T_Facts', 'U') IS NOT NULL DELETE FROM dbo.T_Facts;

    -- ── Livraisons & commandes clients ────────────────────────────────────
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
    IF OBJECT_ID('dbo.T_Cmd_Details', 'U') IS NOT NULL DELETE FROM dbo.T_Cmd_Details;
    IF OBJECT_ID('dbo.T_Cmds', 'U') IS NOT NULL DELETE FROM dbo.T_Cmds;

    -- ── Prestations / services ──────────────────────────────────────────────
    IF OBJECT_ID('dbo.T_Service_Timesheets', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Timesheets;
    IF OBJECT_ID('dbo.T_Service_Deliverables', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Deliverables;
    IF OBJECT_ID('dbo.T_Service_Tasks', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Tasks;
    IF OBJECT_ID('dbo.T_Service_Projects', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Projects;
    IF OBJECT_ID('dbo.T_Service_Catalog', 'U') IS NOT NULL DELETE FROM dbo.T_Service_Catalog;

    -- ── Stock & finance opérationnelle ──────────────────────────────────────
    IF OBJECT_ID('dbo.T_Mouvement_Stock', 'U') IS NOT NULL DELETE FROM dbo.T_Mouvement_Stock;
    IF OBJECT_ID('dbo.T_Transformation', 'U') IS NOT NULL DELETE FROM dbo.T_Transformation;
    IF OBJECT_ID('dbo.T_Audit', 'U') IS NOT NULL DELETE FROM dbo.T_Audit;
    IF OBJECT_ID('dbo.T_Versements', 'U') IS NOT NULL DELETE FROM dbo.T_Versements;

    -- ── Clients (comptes en ligne, passagers) ───────────────────────────────
    IF OBJECT_ID('dbo.T_Customer', 'U') IS NOT NULL DELETE FROM dbo.T_Customer;

    -- ── Remise des stocks à zéro (catalogue conservé) ───────────────────────
    IF OBJECT_ID('dbo.T_Stock', 'U') IS NOT NULL
        UPDATE dbo.T_Stock SET Qte = 0;

    COMMIT TRANSACTION;
    PRINT 'Purge transactionnelle terminée.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO

-- ── Contrôle ────────────────────────────────────────────────────────────────
SELECT 'T_Facts' AS [Table], COUNT(*) AS Lignes FROM dbo.T_Facts
UNION ALL SELECT 'T_Commande', COUNT(*) FROM dbo.T_Commande
UNION ALL SELECT 'T_Customer', COUNT(*) FROM dbo.T_Customer
UNION ALL SELECT 'T_Mouvement_Stock', COUNT(*) FROM dbo.T_Mouvement_Stock
UNION ALL SELECT 'T_Arts', COUNT(*) FROM dbo.T_Arts
UNION ALL SELECT 'T_Stock (Qte>0)', COUNT(*) FROM dbo.T_Stock WHERE Qte > 0;

PRINT '=== Fin Instance 1 — démo sans transactions ===';
GO
