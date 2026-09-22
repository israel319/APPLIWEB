# IPS SARL — Spécifications fonctionnelles (Scope of Work)

> **Source** : `E:\IPS\Scope of work\DOCUMENT DE SPECIFICATIONS FONCTIONNELLES  LOGICIEL IPS SARL....pdf`  
> **Client** : IT PROGRESS SERVICES — IPS SARL  
> **Adresse** : 1828, Route Katebi, Kolwezi, Lualaba, R.D. Congo  
> **Contact** : +243 994 062 606 | +243 82 560 8437  
> **E-mail** : Info@ips-drc.com | www.ips-drc.com  
> **Année** : 2026

---

## Mode d'utilisation de ce document

| Symbole | Signification |
|---------|---------------|
| `[ ]` | Non démarré |
| `[~]` | En cours |
| `[x]` | **Validé** — étape terminée et acceptée, on passe à la suite |
| **AppPlusPlus** | État dans le logiciel actuel (`E:\APPLIWEB\APPLIWEB`) |

> **Règle** : une étape n'est cochée `[x]` qu'après livraison **et** validation métier.

---

## Vue d'ensemble des modules

| # | Module | Statut global AppPlusPlus |
|---|--------|---------------------------|
| 1 | Gestion des stocks & Facturation | 🟢 Avancé |
| 2 | Gestion des services & Facturation | 🟡 MVP livré |
| 3 | Gestion de la flotte & Location véhicules | 🔴 Non démarré |
| 4 | Gestion du personnel et de la paie | 🟡 GRH partiel |
| 5 | Gestion du crédit client & Facturation | 🟠 Partiel |
| — | Sécurité & droits d'accès (transversal) | 🟢 En place |
| — | Cartographie des postes de travail | 🟡 Rôles de base |

---

# PARTIE 1 — Architecture des modules logiciels

---

## Module 1 — Gestion des stocks & Facturation

> Cycle de vie des **produits physiques**, de l'achat à la vente.

### 1.1 Stock

- [x] **1.1.1** Suivi en temps réel des quantités disponibles (`T_Stock`, hub Articles)
- [x] **1.1.2** Alertes de seuil critique / rupture de stock (facturation + scan)
- [x] **1.1.3** Gestion des entrées fournisseurs (approvisionnement, bons de commande)
- [x] **1.1.4** Gestion des sorties (ventes → déduction stock automatique)
- [ ] **1.1.5** Validation métier complète du cycle achat → stock → vente

**Validation module 1.1** : `[ ]` — à cocher après recette utilisateur magasinier.

### 1.2 Lien avec la Facturation

- [x] **1.2.1** Facture validée → diminution automatique du stock
- [x] **1.2.2** Bon de commande fournisseur → attente d'entrée en stock (commandes internes / appro)
- [~] **1.2.3** Édition bon de commande → préparation facture d'achat (à confirmer selon process IPS)
- [ ] **1.2.4** Validation métier du lien vente ↔ stock ↔ appro

**Validation module 1** : `[ ]`

---

## Module 2 — Gestion des services & Facturation

> Prestations **immatérielles** : conseil, main-d'œuvre, interventions techniques.

### 2.1 Services

- [x] **2.1.1** Projets / missions par client (`T_Service_Projects`, hub `/services`)
- [~] **2.1.2** Planification des tâches (tables + liste ; formulaires UI à compléter)
- [x] **2.1.3** Feuilles de temps / timesheets par projet ou client
- [x] **2.1.4** Workflow heures : brouillon → validée → facturée
- [~] **2.1.5** Suivi des livrables (tables + liste ; formulaires UI à compléter)

**Validation module 2.1** : `[ ]`

### 2.2 Lien avec la Facturation

- [x] **2.2.1** Récupération automatique des heures validées pour facturation
- [x] **2.2.2** Prise en charge du forfait convenu
- [x] **2.2.3** Génération facture en un clic (brouillon → validation manuelle)
- [ ] **2.2.4** Tests bout en bout : projet → heures → facture → impression

**Validation module 2** : `[ ]`

---

## Module 3 — Gestion de la flotte & Location de véhicules

> Utilisation interne **et** location commerciale.

### 3.1 Volet logistique (services internes)

- [ ] **3.1.1** Planification & affectation véhicule → agent / équipe / mission
- [ ] **3.1.2** Suivi kilométrage
- [ ] **3.1.3** Suivi consommation carburant (cartes essence)
- [ ] **3.1.4** Carnets de bord digitaux
- [ ] **3.1.5** Alertes maintenance (vidange, contrôle technique, assurance)
- [ ] **3.1.6** Calcul coût de revient au km par véhicule

**Validation module 3.1** : `[ ]`

### 3.2 Volet commercial (location & facturation)

- [ ] **3.2.1** Calendrier disponibilité (loué / disponible / entretien)
- [ ] **3.2.2** Contrats de location (durée, forfait km, caution, avec/sans chauffeur)
- [ ] **3.2.3** Facturation automatique selon grilles tarifaires (journalier, dégressif, km sup.)
- [ ] **3.2.4** Intégration frais de déplacement sur factures prestations services

**Validation module 3** : `[ ]`

---

## Module 4 — Gestion du personnel et de la paie

> Module **strictement interne** — pas d'interaction directe avec facturation client.

### 4.1 Ressources humaines

- [x] **4.1.1** Fiches employés (`GRH`, module `/grh`)
- [~] **4.1.2** Contrats de travail
- [~] **4.1.3** Plannings
- [x] **4.1.4** Absences
- [x] **4.1.5** Congés
- [ ] **4.1.6** Heures supplémentaires (à confirmer / compléter)

**Validation module 4.1** : `[ ]`

### 4.2 Paie

- [~] **4.2.1** Calcul automatique salaire brut / net en fin de mois
- [~] **4.2.2** Génération bulletins de paie
- [ ] **4.2.3** Déclarations sociales / fiscales selon législation RDC
- [ ] **4.2.4** Validation métier paie IPS

**Validation module 4** : `[ ]`

---

## Module 5 — Gestion du crédit client & Facturation

> Sécuriser la trésorerie : plafonds, délais, encaissements.

### 5.1 Crédit client

- [ ] **5.1.1** Plafond de crédit autorisé par client
- [ ] **5.1.2** Délais de paiement accordés (ex. 30 jours fin de mois)
- [~] **5.1.3** Historique des encaissements (paiements partiels existants sur factures)
- [ ] **5.1.4** Balance âgée clients

**Validation module 5.1** : `[ ]`

### 5.2 Lien avec la Facturation

- [ ] **5.2.1** Vérification plafond crédit à la création facture
- [ ] **5.2.2** Blocage si factures en retard
- [ ] **5.2.3** Génération automatique lettres de relance impayés
- [ ] **5.2.4** Validation métier recouvrement IPS

**Validation module 5** : `[ ]`

---

# PARTIE 2 — Sécurité et gestion des droits d'accès

> Module transversal : rôles, autorisations, traçabilité.

### 2.A Profils et rôles

- [x] **2.A.1** Profils types (Administrateur, Gérant, Caissier, Magasinier, Vendeur…)
- [x] **2.A.2** Licences par module (`T_ModuleLicenses` : vente, stock, commandes, grh, rapports, services)
- [ ] **2.A.3** Profils IPS complets (Commercial, Logistique flotte, Crédit client, RH…)

**Validation 2.A** : `[ ]`

### 2.B Niveaux d'autorisations

- [x] **2.B.1** Lecture seule
- [x] **2.B.2** Écriture / modification
- [x] **2.B.3** Suppression / validation définitive (managers)
- [ ] **2.B.4** Matrice droits alignée sur les 6 postes IPS (Partie 3)

**Validation 2.B** : `[ ]`

### 2.C Traçabilité (logs)

- [~] **2.C.1** Enregistrement actions importantes (qui, quoi, quand)
- [ ] **2.C.2** Journal d'audit consultable par administrateur / DG
- [ ] **2.C.3** Validation conformité IPS

**Validation Partie 2** : `[ ]`

---

# PARTIE 3 — Cartographie des postes de travail

---

## Poste 1 — Responsable des stocks / Magasinier

**Rôle** : Disponibilité des produits et fiabilité des inventaires.

- [x] Enregistrer réceptions fournisseurs
- [x] Valider bons de sortie (ventes / interne)
- [~] Réaliser inventaires (stock + rapports)
- [x] Suivre alertes seuil critique

**Validation poste 1** : `[ ]`

---

## Poste 2 — Gestionnaire de la flotte et logistique

**Rôle** : Disponibilité, sécurité et rentabilité de la flotte.

- [ ] Planifier et affecter véhicules en interne
- [ ] Suivre kilométrage et carburant
- [ ] Piloter alertes maintenance
- [ ] Analyser coût de revient au km

**Validation poste 2** : `[ ]`

---

## Poste 3 — Agent commercial / Chargé de location

**Rôle** : Chiffre d'affaires et gestion commerciale courante.

- [ ] Consulter disponibilité véhicules / produits (flotte)
- [x] Saisir feuilles de temps des services
- [ ] Rédiger contrats de location
- [x] Générer factures ventes
- [x] Générer factures services (1 clic → brouillon)
- [ ] Générer factures locations

**Validation poste 3** : `[ ]`

---

## Poste 4 — Responsable crédit client / Recouvrement (ou Comptable)

**Rôle** : Réduire délais de paiement et risque d'impayés.

- [ ] Configurer plafonds crédit client
- [x] Enregistrer encaissements (paiements factures)
- [ ] Analyser balance âgée
- [ ] Valider / bloquer commandes hors limites
- [ ] Piloter relances

**Validation poste 4** : `[ ]`

---

## Poste 5 — Gestionnaire RH et paie

**Rôle** : Conformité administrative RH et ponctualité de la paie.

- [x] Gérer fiches employés
- [~] Gérer contrats
- [x] Valider congés / absences
- [ ] Valider heures supplémentaires
- [~] Calculer paie mensuelle
- [~] Éditer bulletins et déclarations sociales

**Validation poste 5** : `[ ]`

---

## Poste 6 — Administrateur système / Directeur général

**Rôle** : Supervision globale et sécurité des données.

- [x] Créer comptes utilisateurs et attribuer rôles
- [x] Consulter tableaux de bord (dashboard)
- [ ] Inspecter journal d'audits (logs)
- [x] Accès transversal à tous les modules autorisés par licence

**Validation poste 6** : `[ ]`

---

# Journal de validation IPS

| Date | Élément validé | Validé par | Commentaire |
|------|----------------|------------|-------------|
| 2026-06-20 | Module 2 — MVP code (AppPlusPlus) | — | Hub `/services`, SQL exécuté |
| | Module 1 — Recette complète | | |
| | Module 2 — Recette complète | | |
| | Module 3 — Démarrage | | |
| | … | | |

---

# Prochaine étape recommandée

**→ EN COURS** : recette Module 1 — voir `E:\IPS\Scope of work\IPS_RECETTE_MODULE_1_STOCK.md`  
Plan global : `IPS_PLAN_RECETTE.md`

1. ~~Valider Module 2~~ — **après** Module 1
2. Recette Module 1 (Stock & Facturation) — **commencer ici**
3. Recette Module 2 (Services)
4. Module 3 (Flotte) — cadrage ensuite

---

*Document généré à partir du PDF IPS SARL — Specifications fonctionnelles (2026).*
