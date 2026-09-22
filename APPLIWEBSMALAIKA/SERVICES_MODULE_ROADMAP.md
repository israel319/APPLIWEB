# Module Services & Prestations — Feuille de route

> Document de suivi : une étape est **validée** quand elle est terminée et testée.  
> On ne passe à l’étape suivante qu’après validation.

**Base de données** : `GlobalShoping` (même BDD que Ventes — pas de BDD séparée)  
**Script SQL** : `AppPlusPlus.Web/Scripts/Migration_Services_Module.sql`  
**Hub applicatif** : `/services`

---

## Légende

| Symbole | Signification |
|---------|---------------|
| `[x]` | Étape validée |
| `[ ]` | À faire |
| `[~]` | En cours |

---

## Phase 0 — Fondations (schéma & permissions)

- [x] **0.1** Définir le modèle de données (`T_Service_Projects`, `T_Service_Tasks`, `T_Service_Timesheets`, `T_Service_Deliverables`)
- [x] **0.2** Étendre `T_Facts` (`Id_Service_Project`) et `T_Fact_Details` (`Description_Line`, `Id_Timesheet`)
- [x] **0.3** Créer entités Domain + enums (`AppPlusPlus.Domain/Entities/Prestations/`)
- [x] **0.4** Enregistrer dans `AppDbContext` + configurations EF
- [x] **0.5** Exécuter `Migration_Services_Module.sql` sur GlobalShoping
- [x] **0.6** Ajouter permission `services` + licence module `T_ModuleLicenses`
- [x] **0.7** Menu **Services** dans `MainLayout` + routes `/services`

**Validation phase 0** : build OK, script SQL exécuté, menu visible pour rôle autorisé.

---

## Phase 1 — Projets & feuilles de temps

- [x] **1.1** Service métier `IServiceManagementService` + implémentation
- [x] **1.2** Hub `/services` — onglet **Projets** (liste, recherche)
- [x] **1.3** Formulaire projet `/services/projet/nouveau` et `/modifier/{id}`
- [x] **1.4** Modes facturation : horaire / forfait / mixte
- [x] **1.5** Onglet **Feuilles de temps** — saisie rapide
- [x] **1.6** Workflow heures : brouillon → **validée** (approve) → facturée
- [ ] **1.7** Tests manuels : créer projet, saisir 2 h, valider, vérifier totaux

**Validation phase 1** : parcours complet projet + timesheet sans facture.

---

## Phase 2 — Facturation (1 clic)

- [x] **2.1** `GenerateInvoiceFromProjectAsync` — agrégation par tâche + forfait
- [x] **2.2** Facture **brouillon** (`Status = 0`) + redirection `/vente/facturation/{id}`
- [x] **2.3** Lignes prestation sans stock (`Description_Line`, pas de déduction `T_Stock`)
- [x] **2.4** Impression ticket : libellé prestation (`PrintFacture.razor`)
- [x] **2.5** Anti double facturation (timesheets → `Billed` + `FactId`)
- [ ] **2.6** Tests manuels : générer facture → valider → imprimer

**Validation phase 2** : facture correcte (qté décimale, libellés, montants arrondis CDF).

---

## Phase 3 — Tâches & livrables (compléter l’UI)

- [x] **3.1** Onglets **Tâches** et **Livrables** (lecture / listes)
- [ ] **3.2** Formulaire création / édition **tâche** (depuis hub ou fiche projet)
- [ ] **3.3** Formulaire création / édition **livrable**
- [ ] **3.4** Lier timesheet à une tâche (sélecteur dans saisie rapide)
- [ ] **3.5** Fiche projet enrichie (tâches + livrables + récap heures sur une page)
- [ ] **3.6** Tests : planifier tâche, enregistrer heures sur tâche, facturer

**Validation phase 3** : suivi complet projet → tâches → heures → livrables.

---

## Phase 4 — Confort & rapports (optionnel)

- [ ] **4.1** Catalogue prestations (`T_Service_Catalog`) — types réutilisables
- [ ] **4.2** Rapport heures par client / projet dans `/rapports`
- [ ] **4.3** Annulation facture brouillon → remettre timesheets en « validée »
- [ ] **4.4** Export Excel feuilles de temps
- [ ] **4.5** Notifications échéance livrables

**Validation phase 4** : rapports et catalogue utilisables en production.

---

## Parcours utilisateur cible (référence)

```
1. Services → Nouveau projet (client, tarif horaire ou forfait)
2. Feuilles de temps → saisir heures → Valider ✓
3. Projets → Générer facture
4. Ventes / Facturation → contrôler → Valider la facture → Imprimer
```

---

## Décisions actées (ne pas revisiter sans accord)

| Sujet | Décision |
|-------|----------|
| Base de données | **Même BDD** GlobalShoping |
| Facture générée | **Brouillon** puis validation manuelle |
| Lignes facture | Agrégation **par tâche** (+ forfait si mixte/forfait) |
| Validation heures | Gérant / admin (approve) |
| Forfait | **Une fois** par projet (`FlatInvoiced`) |
| Localisation | **Optionnelle** pour prestations pures |
| Catalogue articles | Module prestations **autonome** en phase 1 |

---

## Journal de validation

| Date | Étape validée | Validé par | Notes |
|------|---------------|------------|-------|
| 2026-06-20 | Phase 0 complète | — | Script SQL exécuté sur localhost |
| 2026-06-20 | Phase 1 code livré | — | Hub + formulaires |
| 2026-06-20 | Phase 2 code livré | — | Génération facture brouillon |
| | Phase 1.7 tests manuels | | |
| | Phase 2.6 tests manuels | | |

---

## Prochaine action

**→ Valider Phase 1.7** : test manuel projet + timesheet, puis cocher et passer à **Phase 2.6**.
