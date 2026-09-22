-- ============================================================================
-- Upgrade GlobalShoping : état BDD au publish du 19/06/2026 → état actuel dev
-- ============================================================================
--
-- Cible : base restaurée depuis le backup du dernier publish (schéma + données
--         d'alors, SANS les évolutions post-publish).
--
-- Ce script enchaîne les migrations ajoutées APRÈS le publish du 19/06/2026.
-- Chaque étape est idempotente (relançable sans casse).
--
-- PRÉREQUIS (déjà présents sur la BDD du publish du 19/06/2026) :
--   - Stock_Refonte, GlobalShoping_Align/Patch, FactDetail_Currency,
--     Monetary_Standard_Global, GlobalShoping_FactStatus (remap statuts factures)
--   - T_Stock (Seuil), T_Mouvement_Stock, quatuor monétaire, clôtures…
--   - Scripts antérieurs au publish NON rejoués ici (voir liste ci-dessus)
--
-- HORS PÉRIMÈTRE DE CE SCRIPT (à traiter séparément) :
--   - APW_GRH : autre base, pas impactée
--   - Déploiement binaires : republier AppPlusPlus.Web (UI Rapports, hubs…)
--   - Deploy_03_IIS_SqlPermissions.sql : droits login IIS si nouvelle machine
--   - Migration_DemandesGeneriques_Job.sql : job SQL Agent 18h30 (étape 11, msdb)
--   - Permissions rôles « services » : appliquées au démarrage app ou via Administration
--
-- ── AVANT D'EXÉCUTER ──
--   1. BACKUP de la base cible :
--        BACKUP DATABASE [GlobalShoping] TO DISK = N'C:\Temp\GlobalShoping_pre_upgrade.bak'
--        WITH INIT, COMPRESSION;
--   2. Arrêter l'application AppPlusPlus (évite verrous).
--   3. Se placer dans le dossier Scripts (chemins :r relatifs) :
--        cd E:\APPLIWEB\APPLIWEB\AppPlusPlus.Web\Scripts
--   4. Lancer :
--        sqlcmd -S localhost -E -C -d GlobalShoping -i Upgrade_GlobalShoping_From_Publish_20260619.sql
--
-- ── APRÈS L'EXÉCUTION ──
--   - Marquer votre dépôt central (obligatoire pour les demandes génériques) :
--        UPDATE T_Localisations SET Is_Depot = 1 WHERE Id_Localisation = <ID_DEPOT>;
--   - (Optionnel si étape 11 échoue) Job 18h30 manuel :
--        sqlcmd -S localhost -E -C -i Migration_DemandesGeneriques_Job.sql
--
-- ============================================================================

USE GlobalShoping;
GO

SET NOCOUNT ON;
PRINT '=== Upgrade GlobalShoping — publish 2026-06-19 → état actuel ===';
PRINT CONCAT('Début : ', CONVERT(VARCHAR(30), GETDATE(), 120));
GO

-- ── Prérequis publish (schéma d'époque 19/06/2026) ─────────────────────────
PRINT '';
PRINT '--- Contrôle PRÉREQUIS (base = publish 19/06/2026 attendue) ---';

DECLARE @preOk BIT = 1;

IF OBJECT_ID('T_Mouvement_Stock') IS NULL BEGIN PRINT '*** ERREUR : T_Mouvement_Stock absente — exécuter d''abord Migration_Stock_Refonte / GlobalShoping_Patch'; SET @preOk = 0; END
IF COL_LENGTH('T_Facts', 'Id_Monais') IS NULL BEGIN PRINT '*** ERREUR : quatuor monétaire absent — exécuter Monetary_Standard_Global'; SET @preOk = 0; END
IF COL_LENGTH('T_Stock', 'Seuil') IS NULL BEGIN PRINT '*** ERREUR : T_Stock.Seuil absent — exécuter Migration_Stock_Refonte'; SET @preOk = 0; END

DECLARE @oldFactStatus INT = (SELECT COUNT(*) FROM T_Facts WHERE Status IN (3, 4, 5, 6, 7));
IF @oldFactStatus > 100
BEGIN
    PRINT CONCAT('*** ATTENTION : ', @oldFactStatus, ' facture(s) avec anciens codes Status 3-7.');
    PRINT '    → Exécuter Migration_GlobalShoping_FactStatus.sql AVANT cet upgrade (non inclus : non idempotent).';
    SET @preOk = 0;
END

IF @preOk = 1 PRINT 'Prérequis publish : OK';
ELSE BEGIN
    RAISERROR(N'Prérequis publish non satisfaits — corriger avant de continuer.', 16, 1);
    RETURN;
END
GO

-- ── État AVANT ──────────────────────────────────────────────────────────────
PRINT '';
PRINT '--- Contrôle AVANT upgrade (éléments post-publish) ---';

SELECT
    'Qte_max'                   AS Element,
    CASE WHEN COL_LENGTH('T_Stock', 'Qte_max') IS NOT NULL THEN 'OK' ELSE 'MANQUANT' END AS Etat
UNION ALL SELECT 'T_Cmds.Id_Localisation',
    CASE WHEN COL_LENGTH('T_Cmds', 'Id_Localisation') IS NOT NULL THEN 'OK' ELSE 'MANQUANT' END
UNION ALL SELECT 'Triggers monétaires (10 attendus)',
    CAST((SELECT COUNT(*) FROM sys.triggers WHERE name LIKE 'TR_MonetaryNormalize_%') AS VARCHAR(10))
UNION ALL SELECT 'T_Service_Projects',
    CASE WHEN OBJECT_ID('T_Service_Projects') IS NOT NULL THEN 'OK' ELSE 'MANQUANT' END
UNION ALL SELECT 'T_Service_Catalog',
    CASE WHEN OBJECT_ID('T_Service_Catalog') IS NOT NULL THEN 'OK' ELSE 'MANQUANT' END
UNION ALL SELECT 'T_Demandes',
    CASE WHEN OBJECT_ID('T_Demandes') IS NOT NULL THEN 'OK' ELSE 'MANQUANT' END
UNION ALL SELECT 'T_Localisations.Is_Depot',
    CASE WHEN COL_LENGTH('T_Localisations', 'Is_Depot') IS NOT NULL THEN 'OK' ELSE 'MANQUANT' END
UNION ALL SELECT 'T_Users.Id_Monais_Pref',
    CASE WHEN COL_LENGTH('T_Users', 'Id_Monais_Pref') IS NOT NULL THEN 'OK' ELSE 'MANQUANT' END
UNION ALL SELECT 'T_Arts.Image_Article',
    CASE WHEN COL_LENGTH('T_Arts', 'Image_Article') IS NOT NULL THEN 'OK' ELSE 'MANQUANT' END
UNION ALL SELECT 'sp_GenererDemandesGeneriques',
    CASE WHEN OBJECT_ID('dbo.sp_GenererDemandesGeneriques') IS NOT NULL THEN 'OK' ELSE 'MANQUANT' END;
GO

PRINT '';
PRINT '>>> Étape 1/10 — Arrondis monétaires (fonctions + 10 triggers)';
:r Migration_Monetary_Rounding_Enforcement.sql
GO

PRINT '';
PRINT '>>> Étape 2/10 — Réparation données quatuor factures (peut prendre quelques minutes)';
:r Migration_FactDetail_Quatuor_Repair.sql
GO

PRINT '';
PRINT '>>> Étape 3/10 — Module Services & Prestations';
:r Migration_Services_Module.sql
GO

PRINT '';
PRINT '>>> Étape 4/10 — Catalogue prestations';
:r Migration_Service_Catalog.sql
GO

PRINT '';
PRINT '>>> Étape 5/10 — Qte_max + localisation commandes internes';
:r Migration_Stock_QteMax_CommandeGenerique.sql
GO

PRINT '';
PRINT '>>> Étape 6/10 — Backfill Qte_max = 100 sur tous les stocks';
:r Migration_Stock_QteMax_Backfill_100.sql
GO

PRINT '';
PRINT '>>> Étape 7/10 — Demandes génériques (tables + procédure)';
:r Migration_Demandes_Generiques.sql
GO

PRINT '';
PRINT '>>> Étape 8/10 — Sécurité table T_Demande_Details (si étape 7 incomplète)';
:r Migration_Demandes_Generiques_FixDetails.sql
GO

PRINT '';
PRINT '>>> Étape 9/10 — Préférence devise utilisateur';
:r Migration_User_PreferredMoney.sql
GO

PRINT '';
PRINT '>>> Étape 10/11 — Image article (T_Arts.Image_Article)';
:r Migration_Article_Image.sql
GO

PRINT '';
PRINT '>>> Étape 11/11 — Job SQL Agent 18h30 (demandes auto)';
PRINT '    Nécessite : SQL Server Agent démarré + droits sysadmin';
:r Migration_DemandesGeneriques_Job.sql
GO

PRINT '';
PRINT '>>> Étape 12/12 — Types article catalogue commandes (A commander / Non a commander)';
:r Migration_Art_Types_Commande.sql
GO

-- ── État APRÈS ──────────────────────────────────────────────────────────────
PRINT '';
PRINT '--- Contrôle APRÈS upgrade (tous doivent être OK sauf triggers = 10) ---';

SELECT
    'Qte_max'                   AS Element,
    CASE WHEN COL_LENGTH('T_Stock', 'Qte_max') IS NOT NULL THEN 'OK' ELSE '*** ERREUR ***' END AS Etat
UNION ALL SELECT 'T_Cmds.Id_Localisation',
    CASE WHEN COL_LENGTH('T_Cmds', 'Id_Localisation') IS NOT NULL THEN 'OK' ELSE '*** ERREUR ***' END
UNION ALL SELECT 'Triggers monétaires (10 attendus)',
    CAST((SELECT COUNT(*) FROM sys.triggers WHERE name LIKE 'TR_MonetaryNormalize_%') AS VARCHAR(10))
UNION ALL SELECT 'T_Service_Projects',
    CASE WHEN OBJECT_ID('T_Service_Projects') IS NOT NULL THEN 'OK' ELSE '*** ERREUR ***' END
UNION ALL SELECT 'T_Service_Catalog',
    CASE WHEN OBJECT_ID('T_Service_Catalog') IS NOT NULL THEN 'OK' ELSE '*** ERREUR ***' END
UNION ALL SELECT 'T_Demandes',
    CASE WHEN OBJECT_ID('T_Demandes') IS NOT NULL THEN 'OK' ELSE '*** ERREUR ***' END
UNION ALL SELECT 'T_Localisations.Is_Depot',
    CASE WHEN COL_LENGTH('T_Localisations', 'Is_Depot') IS NOT NULL THEN 'OK' ELSE '*** ERREUR ***' END
UNION ALL SELECT 'T_Users.Id_Monais_Pref',
    CASE WHEN COL_LENGTH('T_Users', 'Id_Monais_Pref') IS NOT NULL THEN 'OK' ELSE '*** ERREUR ***' END
UNION ALL SELECT 'T_Arts.Image_Article',
    CASE WHEN COL_LENGTH('T_Arts', 'Image_Article') IS NOT NULL THEN 'OK' ELSE '*** ERREUR ***' END
UNION ALL SELECT 'sp_GenererDemandesGeneriques',
    CASE WHEN OBJECT_ID('dbo.sp_GenererDemandesGeneriques') IS NOT NULL THEN 'OK' ELSE '*** ERREUR ***' END
UNION ALL SELECT 'Fonction services',
    CASE WHEN EXISTS(SELECT 1 FROM T_Fonctions WHERE LOWER(LTRIM(RTRIM(Description_Fonction))) = 'services')
         THEN 'OK' ELSE '*** ERREUR ***' END;
GO

USE msdb;
GO
PRINT '';
PRINT '--- Contrôle job SQL Agent ---';
SELECT
    CASE WHEN EXISTS(SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'GlobalShoping_GenererDemandesGeneriques')
         THEN 'OK' ELSE '*** MANQUANT (SQL Agent arrêté ou droits insuffisants) ***' END AS Job_GenererDemandesGeneriques;
GO

USE GlobalShoping;
GO

PRINT '';
PRINT CONCAT('Fin upgrade : ', CONVERT(VARCHAR(30), GETDATE(), 120));
PRINT 'N''oubliez pas : UPDATE T_Localisations SET Is_Depot = 1 WHERE Id_Localisation = <votre_depot>;';
PRINT 'Job 18h30 : GlobalShoping_GenererDemandesGeneriques (étape 11 — vérifier SQL Agent)';
PRINT '=== Upgrade terminé ===';
GO
