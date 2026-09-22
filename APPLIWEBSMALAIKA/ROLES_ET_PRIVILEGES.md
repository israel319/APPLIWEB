# Rôles et privilèges — AppPlusPlus

Ce document décrit comment les rôles sont connectés dans l'application et ce que chaque profil peut faire.

---

## Comment c'est connecté dans le système

| Mécanisme | Où | Effet |
|-----------|-----|--------|
| **T_Permissions** (L / É / S par module) | Chaque écran (Stock, Ventes, Commandes…) | Ouvre ou bloque un **module** |
| **Rôle « Administrateur »** | Partout | Bypass : accès à **tout** |
| **Localisation assignée** | Stock, ventes, rapports… | Limite les **données** au magasin de l'utilisateur |
| **SystemRolePolicy** | **Demandes génériques seulement** | Dépôt = admin **ou** gérant ; réception = magasinier **assigné** |
| **Administration** | `/administration` | **Réservé administrateur** (hardcodé, pas le gérant) |

Au démarrage, l'application initialise les droits par défaut dans `RoleService` si le rôle n'a encore aucune permission en base.

---

## Qui fait quoi — rôle par rôle

### Administrateur

- **Tout** : utilisateurs, rôles, localisations, dépôt, taux, licences…
- Tous les modules en lecture + écriture + suppression
- Toutes les localisations (pas besoin d'être assigné)
- **Approuve les demandes dépôt** (étape 1)

### Gérant

- **Pilotage magasin** : ventes, stock, appro, commandes clients **et** internes, livraisons, services
- **Rapports** : lecture seule
- **Paramètres** : lecture seule
- **Pas** Administration (users / rôles)
- Voit **toutes** les demandes génériques ; **approuve le dépôt** (comme l'admin)
- Ne confirme la réception magasin **que s'il est assigné** à cette localisation

### Magasinier *(agent stock)*

- **Stock** : tout (L + É + S)
- **Approvisionnement** : tout (appro, transferts, transformations)
- **Commandes internes** : tout (fournisseurs, réceptions)
- **Livraisons** : lecture + écriture
- **Ventes / commandes clients** : **lecture seule**
- **Rapports** : lecture seule
- **Uniquement ses localisations** assignées
- **Confirme la réception** des demandes pour **son** magasin (étape 2)

### Caissier / Caissière

- **Ventes + facturation** : lecture + écriture (caisse)
- **Stock, commandes clients, livraisons, services** : **lecture seule**
- **Pas** appro, **pas** commandes fournisseurs
- Données filtrées sur **ses** localisations

### Vendeur / Vendeuse

- **Pas** de tableau de bord (redirection vers `/vente`)
- **Catalogue stock** : lecture seule (pas appro ni paramètres articles)
- **Commandes clients + facturation** : peut créer / modifier
- **Pas** commandes internes, **pas** appro
- Données filtrées sur **ses** localisations

---

## Tableau synthétique des privilèges

| Action | Admin | Gérant | Magasinier | Caissier | Vendeur |
|--------|:-----:|:------:|:----------:|:--------:|:-------:|
| Gérer users / rôles | ● | — | — | — | — |
| Configurer dépôt / locs | ● | — | — | — | — |
| Vente / caisse | ● | ● | L | É | L + É cmd |
| Stock / catalogue | ● | ● | ● | L | L |
| Appro / transferts | ● | ● | ● | — | — |
| Cmd fournisseurs | ● | ● | ● | — | — |
| Approuver demande **dépôt** | ● | ● | — | — | — |
| Confirmer demande **magasin** | ●* | ●* | ●** | — | — |
| Rapports | ● | L | L | — | — |

**Légende :** ● = oui · L = lecture · É = écriture · — = non

- \* Admin / gérant : seulement s'ils sont assignés à la localisation concernée
- \** Magasinier : assigné au magasin demandeur

---

## Ce qui n'est pas « par rôle » partout

1. **Administration** : réservée à l'administrateur uniquement (le gérant n'y accède pas, même avec des droits custom).
2. **Demandes génériques** : seul endroit avec règles métier explicites (admin/gérant pour le dépôt, magasinier pour la réception).
3. **Rôle custom** (ex. « Comptable ») : pas de défaut automatique → à configurer manuellement dans **Administration → Rôles**.
4. Les permissions en base **ne se réécrivent pas** seules si le code change : il faut parfois réinitialiser les droits du rôle dans l'admin.

---

## Configuration pratique pour l'équipe

| Poste | Rôle à assigner | Localisation |
|-------|-----------------|--------------|
| Patron / IT | **Administrateur** | — |
| Responsable magasin | **Gérant** | optionnel |
| Agent entrepôt / magasin | **Magasinier** | Magasin, Encours, etc. |
| Caisse | **Caissier** | son magasin |
| Vendeur terrain | **Vendeur** | son magasin |

---

## Dépannage

Si un utilisateur voit trop ou pas assez, vérifier dans l'ordre :

1. Son **rôle** (Administrateur, Gérant, Magasinier…)
2. Ses **localisations** assignées (`T_User_Localisations`)
3. La matrice **Lecture / Écriture / Suppression** du rôle dans Administration

---

## Fichiers techniques de référence

| Fichier | Rôle |
|---------|------|
| `AppPlusPlus.Application/Services/Administration/RoleService.cs` | Matrice des permissions par défaut |
| `AppPlusPlus.Application/Services/Administration/PermissionResolver.cs` | Résolution des droits utilisateur |
| `AppPlusPlus.Application/Policies/SystemRolePolicy.cs` | Règles demandes génériques (dépôt / magasin) |
| `AppPlusPlus.Web/Components/Layout/MainLayout.razor` | Visibilité du menu et garde des routes |
