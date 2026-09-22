-- ============================================================================
-- GUIDE — 2 instances, 2 bases de données (même application publiée 2 fois)
-- ============================================================================
-- Ce fichier ne modifie rien : il affiche la procédure à suivre.
-- Exécuter : sqlcmd -S localhost -E -C -i Deploy_2_Instances_Etapes.sql
-- ============================================================================

PRINT '';
PRINT '══════════════════════════════════════════════════════════════════';
PRINT '  DÉPLOIEMENT 2 INSTANCES — PROCÉDURE SIMPLE';
PRINT '══════════════════════════════════════════════════════════════════';
PRINT '';
PRINT 'PRINCIPE';
PRINT '  • 1 application publiée 2 fois (2 URLs / 2 sites IIS)';
PRINT '  • 2 bases SQL DISTINCTES (ne jamais partager la même BDD)';
PRINT '  • Chaque site a sa propre DefaultConnection dans appsettings';
PRINT '';
PRINT '──────────────────────────────────────────────────────────────────';
PRINT '  ÉTAPE 1 — Générer vos 2 scripts / backups SQL';
PRINT '──────────────────────────────────────────────────────────────────';
PRINT '  A) BACKUP « AVEC DONNÉES » (catalogue + config + historique)';
PRINT '  B) BACKUP « SANS DONNÉES » (schéma + référentiels, pas d''articles)';
PRINT '';
PRINT '──────────────────────────────────────────────────────────────────';
PRINT '  ÉTAPE 2 — Restaurer et RENOMMER les bases';
PRINT '──────────────────────────────────────────────────────────────────';
PRINT '  Instance DÉMO     → GlomyraShopping';
PRINT '  Instance CLIENT   → MasterGlobalShopping';
PRINT '';
PRINT '──────────────────────────────────────────────────────────────────';
PRINT '  ÉTAPE 3 — Lancer le script adapté sur CHAQUE base';
PRINT '──────────────────────────────────────────────────────────────────';
PRINT '  Sur la base DÉMO (avec données renommée) :';
PRINT '    Deploy_Instance1_Demo_AvecDonnees.sql';
PRINT '    → garde articles, supprime transactions, stock à 0';
PRINT '';
PRINT '  Sur la base CLIENT (sans données renommée) :';
PRINT '    Deploy_Instance2_Client_SansDonnees.sql';
PRINT '    → vide métier, admin seul (admin / 1234)';
PRINT '';
PRINT '  Avant chaque script : BACKUP + arrêter l''application';
PRINT '';
PRINT '──────────────────────────────────────────────────────────────────';
PRINT '  ÉTAPE 4 — Publier l''application 2 fois';
PRINT '──────────────────────────────────────────────────────────────────';
PRINT '  Site DÉMO   : DefaultConnection → GlomyraShopping';
PRINT '  Site CLIENT : DefaultConnection → MasterGlobalShopping';
PRINT '';
PRINT '──────────────────────────────────────────────────────────────────';
PRINT '  RÉSULTAT ATTENDU';
PRINT '──────────────────────────────────────────────────────────────────';
PRINT '  DÉMO   : accueil avec catalogue, pas d''historique ventes/commandes';
PRINT '  CLIENT : accueil vide, staff connecté en admin pour tout configurer';
PRINT '';
PRINT '══════════════════════════════════════════════════════════════════';
GO
