/*
  Catalogue hiérarchique PermissionCatalog → T_Activities
  Idempotent : n'insère que les codes absents.
  Prérequis : T_Fonctions avec au moins 1 ligne, T_Activities créée.
*/
SET NOCOUNT ON;

DECLARE @FonctionId INT = (
    SELECT TOP 1 Id_Fonction FROM dbo.T_Fonctions ORDER BY Id_Fonction
);

IF @FonctionId IS NULL
BEGIN
    RAISERROR('T_Fonctions est vide — créez au moins une fonction avant ce script.', 16, 1);
    RETURN;
END

;WITH Catalog(Code, Description_Activity) AS (
    SELECT * FROM (VALUES
        (N'dashboard', N'Tableau de bord'),
        (N'vente', N'Ventes'),
        (N'articles', N'Articles'),
        (N'commandes', N'Commandes'),
        (N'services', N'Prestations'),
        (N'grh', N'GRH'),
        (N'rapports', N'Rapports'),
        (N'administration', N'Administration'),
        (N'parametres', N'Paramètres'),
        (N'vente.facturation', N'Ventes · Facturation'),
        (N'vente.facturation.read', N'Ventes · Facturation · Consulter'),
        (N'vente.facturation.write', N'Ventes · Facturation · Créer / modifier'),
        (N'vente.facturation.delete', N'Ventes · Facturation · Supprimer / annuler'),
        (N'vente.livraisons', N'Ventes · Livraisons'),
        (N'vente.livraisons.read', N'Ventes · Livraisons · Consulter'),
        (N'vente.livraisons.write', N'Ventes · Livraisons · Enregistrer'),
        (N'articles.catalogue', N'Articles · Catalogue'),
        (N'articles.catalogue.read', N'Articles · Catalogue · Consulter'),
        (N'articles.catalogue.write', N'Articles · Catalogue · Créer / modifier'),
        (N'articles.catalogue.delete', N'Articles · Catalogue · Supprimer'),
        (N'articles.approvisionnement', N'Articles · Approvisionnement'),
        (N'articles.approvisionnement.read', N'Articles · Approvisionnement · Consulter'),
        (N'articles.approvisionnement.write', N'Articles · Approvisionnement · Créer / modifier'),
        (N'articles.approvisionnement.delete', N'Articles · Approvisionnement · Annuler'),
        (N'articles.transformation', N'Articles · Transformation'),
        (N'articles.transformation.read', N'Articles · Transformation · Consulter'),
        (N'articles.transformation.write', N'Articles · Transformation · Enregistrer'),
        (N'articles.localisations', N'Articles · Localisations'),
        (N'articles.localisations.read', N'Articles · Localisations · Consulter'),
        (N'articles.localisations.write', N'Articles · Localisations · Gérer'),
        (N'articles.inventaire', N'Articles · Inventaire magasin'),
        (N'articles.inventaire.read', N'Articles · Inventaire magasin · Consulter'),
        (N'articles.inventaire.write', N'Articles · Inventaire magasin · Saisir / clôturer'),
        (N'commandes.internes', N'Commandes · Commandes internes'),
        (N'commandes.internes.read', N'Commandes · Commandes internes · Consulter'),
        (N'commandes.internes.write', N'Commandes · Commandes internes · Créer / réceptionner'),
        (N'commandes.clients', N'Commandes · Commandes clients'),
        (N'commandes.clients.read', N'Commandes · Commandes clients · Consulter'),
        (N'commandes.clients.write', N'Commandes · Commandes clients · Gérer'),
        (N'commandes.demandes', N'Commandes · Demandes magasin'),
        (N'commandes.demandes.read', N'Commandes · Demandes magasin · Consulter'),
        (N'commandes.demandes.write', N'Commandes · Demandes magasin · Traiter'),
        (N'commandes.livraisons', N'Commandes · Livraisons commandes'),
        (N'commandes.livraisons.read', N'Commandes · Livraisons commandes · Consulter'),
        (N'commandes.livraisons.write', N'Commandes · Livraisons commandes · Enregistrer'),
        (N'administration.utilisateurs', N'Administration · Utilisateurs'),
        (N'administration.utilisateurs.read', N'Administration · Utilisateurs · Consulter'),
        (N'administration.utilisateurs.write', N'Administration · Utilisateurs · Créer / modifier'),
        (N'administration.utilisateurs.delete', N'Administration · Utilisateurs · Désactiver'),
        (N'administration.roles', N'Administration · Rôles & permissions'),
        (N'administration.roles.read', N'Administration · Rôles & permissions · Consulter'),
        (N'administration.roles.write', N'Administration · Rôles & permissions · Gérer'),
        (N'administration.fournisseurs', N'Administration · Fournisseurs'),
        (N'administration.fournisseurs.read', N'Administration · Fournisseurs · Consulter'),
        (N'administration.fournisseurs.write', N'Administration · Fournisseurs · Gérer'),
        (N'administration.localisations', N'Administration · Localisations'),
        (N'administration.localisations.read', N'Administration · Localisations · Consulter'),
        (N'administration.localisations.write', N'Administration · Localisations · Gérer'),
        (N'parametres.general', N'Paramètres · Configuration'),
        (N'parametres.general.read', N'Paramètres · Configuration · Consulter'),
        (N'parametres.general.write', N'Paramètres · Configuration · Modifier'),
        (N'grh.read', N'GRH · Consulter'),
        (N'grh.write', N'GRH · Gérer'),
        (N'rapports.read', N'Rapports · Consulter'),
        (N'rapports.write', N'Rapports · Gérer'),
        (N'services.read', N'Prestations · Consulter'),
        (N'services.write', N'Prestations · Gérer')
    ) AS v(Code, Description_Activity)
)
INSERT INTO dbo.T_Activities (FonctionId, Code, Description_Activity, IsActive)
SELECT @FonctionId, c.Code, c.Description_Activity, 1
FROM Catalog c
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.T_Activities a WHERE a.Code = c.Code
);

PRINT CONCAT('Catalogue hiérarchique : ', @@ROWCOUNT, ' activité(s) ajoutée(s).');

SELECT COUNT(*) AS TotalCatalogueHierarchique
FROM dbo.T_Activities
WHERE Code LIKE N'%.%' OR Code IN (N'dashboard', N'vente', N'articles', N'commandes', N'services', N'grh', N'rapports', N'administration', N'parametres');

GO
