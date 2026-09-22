-- ============================================================================
-- ÉTAPE A — INSTANCE 1 : DÉMO (catalogue visible, ZÉRO opération)
-- ============================================================================
--
-- VOUS AVEZ :
--   • Un backup / script SQL « AVEC DONNÉES » (articles, config, historique…)
--   • Vous restaurez puis RENOMMEZ la base :
--       GlomyraShopping
--
-- CE SCRIPT FAIT :
--   • Supprime factures, commandes, clients, stocks mouvementés, etc.
--   • GARDE articles, catégories, utilisateurs staff, paramètres
--   • Remet les quantités stock à 0
--
-- AVANT :
--   1. BACKUP de la base cible
--   2. Arrêter l'application IIS / site web
--   3. Modifier la ligne USE ci-dessous
--
-- LANCER :
--   sqlcmd -S VOTRE_SERVEUR -E -C -d GlomyraShopping -i Deploy_Instance1_Demo_AvecDonnees.sql
--
-- APRÈS :
--   • appsettings de l'instance DÉMO → DefaultConnection pointe vers cette base
--   • Accueil public : catalogue OK, pas d'historique métier
-- ============================================================================

USE GlomyraShopping;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '=== INSTANCE 1 — Démo (backup avec données → sans transactions) ===';
PRINT CONCAT('Base : ', DB_NAME());
GO

BEGIN TRY
    BEGIN TRANSACTION;

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

    IF OBJECT_ID('dbo.T_Stock', 'U') IS NOT NULL
        UPDATE dbo.T_Stock SET Qte = 0;

    COMMIT TRANSACTION;
    PRINT 'OK — Transactions supprimées, catalogue conservé, stocks à 0.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO

-- Types commandes en ligne (idempotent)
IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'A commander')
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'A commander');
IF NOT EXISTS (SELECT 1 FROM dbo.T_Art_Types WHERE Description_Type = N'Non a commander')
    INSERT INTO dbo.T_Art_Types (Description_Type) VALUES (N'Non a commander');
GO

IF COL_LENGTH('dbo.T_Arts', 'Image_Article') IS NULL
    ALTER TABLE dbo.T_Arts ADD Image_Article NVARCHAR(MAX) NULL;
GO

PRINT '--- Contrôle instance 1 ---';
SELECT 'Articles' AS Element, COUNT(*) AS Valeur FROM dbo.T_Arts
UNION ALL SELECT 'Commandes', COUNT(*) FROM dbo.T_Commande
UNION ALL SELECT 'Factures', COUNT(*) FROM dbo.T_Facts
UNION ALL SELECT 'Clients', COUNT(*) FROM dbo.T_Customer
UNION ALL SELECT 'Stock Qte>0', COUNT(*) FROM dbo.T_Stock WHERE Qte > 0;
GO
