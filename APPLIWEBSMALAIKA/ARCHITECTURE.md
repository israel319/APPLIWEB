# Architecture App++ - Guide complet

## Vue d'ensemble

App++ suit une **Clean Architecture** stricte avec 5 projets organisés en couches concentriques.

```
AppPlusPlus.sln
  ├── AppPlusPlus.Domain            ← Coeur métier (entités, interfaces)
  ├── AppPlusPlus.Application       ← Logique métier (services, DTOs)
  ├── AppPlusPlus.Infrastructure    ← Accès données + services externes
  ├── AppPlusPlus.Web               ← Interface utilisateur (Blazor Server)
  └── AppPlusPlus.Client            ← Shell WASM (minimal)
```

**Règle de dépendance (toujours vers l'intérieur) :**
```
Web → Application + Infrastructure (DI uniquement)
Infrastructure → Domain + Application (interfaces)
Application → Domain
Domain → RIEN
```

---

## AppPlusPlus.Domain

**Rôle :** Contient les entités métier pures, les enums, les constantes et les interfaces repository. ZÉRO dépendance externe.

### Entities/ — Les entités de la base de données

| Dossier | Entités | Description |
|---|---|---|
| `Entities/Administration/` | User, Role, Permission, Localisation, Fonction, Activity, UserActivity, UserFonction, UserLocalisation | Gestion des utilisateurs, rôles, permissions et localisations |
| `Entities/Catalogue/` | Article, ArticleType, ArticleMarque, ArticleCategory, Mesure | Catalogue produits avec types, marques, catégories et unités de mesure |
| `Entities/Vente/` | Fact, FactDetail, Payment, Money, Reduction | Factures, détails lignes, paiements, devises et réductions |
| `Entities/Stock/` | Stock, MouvementStock | Stock par article/localisation et historique mouvements |
| `Entities/Approvisionnement/` | Appro, ApproDetail, ApproExpense, Transformation, ExpenseSource | Approvisionnements, frais et transformations d'articles |
| `Entities/Commandes/` | Commande, CommandeDetail, Livraison, LivraisonDetail | Commandes clients et livraisons |
| `Entities/CommandesInternes/` | Cmd, CmdDetail | Commandes internes (transferts entre localisations) |
| `Entities/Finance/` | Caisse, UserCaisse, Taux, Periode, Versement, Audit, Currency | Caisses, taux de change, périodes comptables, versements |
| `Entities/Clients/` | Customer, CustomerType | Clients et types de clients |
| `Entities/Fournisseurs/` | Supplier, SupplierService | Fournisseurs et services fournis |
| `Entities/Parametres/` | AppSetting, ShopProfile | Paramètres application et profil boutique |
| `Entities/Shared/` | Status, StatusCmd, StatusFactDetail | Statuts partagés entre modules |

### Enums/ — Énumérations métier

| Fichier | Description |
|---|---|
| `FactureStatus.cs` | Brouillon, Validee, Payee, Annulee |
| `TypeMouvement.cs` | Types de mouvements de stock (entrée, sortie, transfert...) |
| `TypeDocument.cs` | Types de documents (facture, bon de commande...) |

### Common/ — Constantes et types partagés

| Fichier | Description |
|---|---|
| `AppFunctions.cs` | Noms des fonctions/permissions (constantes string) |
| `PermissionSnapshot.cs` | Snapshot des permissions d'un utilisateur |
| `TypeConstants.cs` | Constantes de types partagées |

### Interfaces/Repositories/ — Contrats d'accès aux données

| Fichier | Description |
|---|---|
| `IRepository.cs` | CRUD générique (GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync) |
| `ICatalogueRepository.cs` | Articles + types/marques/catégories/mesures |
| `IFactureRepository.cs` | Factures et détails |
| `IStockRepository.cs` | Stocks et mouvements |
| `IApproRepository.cs` | Approvisionnements |
| `ICommandeRepository.cs` | Commandes clients |
| `ICmdRepository.cs` | Commandes internes |
| `IUserRepository.cs` | Utilisateurs |
| `IRoleRepository.cs` | Rôles et permissions |
| `ICustomerRepository.cs` | Clients |
| `ISupplierRepository.cs` | Fournisseurs |
| `IFinanceRepository.cs` | Caisses, taux, périodes |
| `IParametresRepository.cs` | Paramètres et profil boutique |
| `ILookupRepository.cs` | Données de référence (statuts, devises...) |
| `IUnitOfWork.cs` | Transaction multi-repository |

---

## AppPlusPlus.Application

**Rôle :** Contient la logique métier, les interfaces de services externes, les DTOs. Dépend uniquement de Domain.

### Services/ — Logique métier par module

Chaque module contient une **interface** (I*Service.cs) et son **implémentation** (*Service.cs).

| Dossier | Interface | Responsabilité |
|---|---|---|
| `Services/Administration/` | `IPermissionResolver` | Résolution permissions utilisateur (par login + rôle) |
| | `IRoleService` | CRUD rôles + initialisation permissions par défaut |
| `Services/Localisation/` | `ILocalisationService` | Résolution localisations utilisateur (multi-loc) |
| `Services/Parametres/` | `IAppSettingsService` | Cache paramètres application (singleton, chargé au démarrage) |
| | `IShopProfileService` | Profil boutique (logo, adresses) |
| `Services/Catalogue/` | `ICatalogueService` | CRUD articles, types, marques, catégories, mesures |
| `Services/Stock/` | `IStockService` | Consultation et mouvements de stock |
| `Services/Vente/` | `IFacturationService` | Création/modification factures, calculs totaux |
| | `IPaymentService` | Enregistrement paiements |
| `Services/Approvisionnement/` | `IApproService` | Création appros, calculs PA/PV/bénéfice |
| | `ITransformationService` | Transformations d'articles |
| `Services/Commandes/` | `ICommandeService` | Commandes clients |
| | `ICmdService` | Commandes internes |
| | `ILivraisonService` | Livraisons |
| `Services/Finance/` | `IClotureService` | Clôture journalière |
| | `IFinanceService` | Caisses, taux, périodes, versements |
| `Services/Dashboard/` | `IDashboardService` | KPIs du tableau de bord |
| `Services/Rapports/` | `IRapportService` | Génération rapports |

### Interfaces/ — Abstractions services externes

| Fichier | Description |
|---|---|
| `IEmailSender.cs` | Envoi d'emails (SMTP) |
| `INumberToWordsConverter.cs` | Conversion montant → lettres (pour factures) |
| `ICurrentUserService.cs` | Lecture utilisateur connecté (HttpContext.User.Claims) |

### DTOs/ — Objets de transfert

| Dossier | DTOs | Description |
|---|---|---|
| `DTOs/Dashboard/` | DashboardDataDto, MonthlyRevenueDto, StockAlertDto, TopArticleDto... | Données agrégées du dashboard |
| `DTOs/Commandes/` | CmdRowDto, CommandeListResult | Résumés pour listes |
| `DTOs/Vente/` | FactRowDto, FactureViewDto | Résumés factures |

### Common/

| Fichier | Description |
|---|---|
| `ServiceResult.cs` | Résultat Success/Failure avec message d'erreur |

### DependencyInjection.cs

Méthode d'extension `AddApplicationServices()` qui enregistre tous les services Application dans le conteneur DI.

---

## AppPlusPlus.Infrastructure

**Rôle :** Implémentation de l'accès aux données (EF Core), services externes (email, conversion). Dépend de Domain + Application.

### Persistence/AppDbContext.cs

Le DbContext Entity Framework Core. Contient tous les `DbSet<>` et applique les configurations via `ApplyConfigurationsFromAssembly()`.

### Persistence/Configurations/ — Configuration EF Core

21 fichiers `IEntityTypeConfiguration<T>`. Chaque fichier configure le mapping d'une entité vers la base de données (nom de table, clés, relations, index).

| Fichier | Entité configurée |
|---|---|
| `ArticleConfiguration.cs` | Article (table T_Articles) |
| `FactConfiguration.cs` | Fact + FactDetail |
| `CommandeConfiguration.cs` | Commande + CommandeDetail |
| `UserConfiguration.cs` | User (table T_Users) |
| `RoleConfiguration.cs` | Role + Permission |
| ... | (21 configurations au total) |

### Persistence/Repositories/ — Implémentations repository

Chaque repository utilise le pattern `IDbContextFactory<AppDbContext>` adapté à Blazor Server (pas de DbContext longue durée).

| Fichier | Interface Domain implémentée |
|---|---|
| `RepositoryBase.cs` | `IRepository<T>` — CRUD générique |
| `CatalogueRepository.cs` | `ICatalogueRepository` |
| `FactureRepository.cs` | `IFactureRepository` |
| `StockRepository.cs` | `IStockRepository` |
| `ApproRepository.cs` | `IApproRepository` |
| `CommandeRepository.cs` | `ICommandeRepository` |
| `CmdRepository.cs` | `ICmdRepository` |
| `UserRepository.cs` | `IUserRepository` |
| `RoleRepository.cs` | `IRoleRepository` |
| `CustomerRepository.cs` | `ICustomerRepository` |
| `SupplierRepository.cs` | `ISupplierRepository` |
| `FinanceRepository.cs` | `IFinanceRepository` |
| `ParametresRepository.cs` | `IParametresRepository` |
| `LookupRepository.cs` | `ILookupRepository` |
| `UnitOfWork.cs` | `IUnitOfWork` |

### QueryServices/ — Services qui nécessitent des requêtes complexes

Services Application dont l'implémentation vit dans Infrastructure car ils font des requêtes EF Core complexes (joins, agrégations) qui ne passent pas par un simple repository.

| Fichier | Interface Application implémentée |
|---|---|
| `DashboardService.cs` | `IDashboardService` — KPIs avec agrégations complexes |
| `Vente/FacturationQueryService.cs` | `IFacturationService` — requêtes factures multi-tables |
| `Commandes/LivraisonQueryService.cs` | `ILivraisonService` — requêtes livraisons |
| `Finance/ClotureQueryService.cs` | `IClotureService` — clôture avec calculs financiers |

### ExternalServices/ — Services externes

| Fichier | Interface Application implémentée | Description |
|---|---|---|
| `EmailSender.cs` | `IEmailSender` | Envoi email via SMTP (MailKit) |
| `NumberToWordsConverter.cs` | `INumberToWordsConverter` | Conversion montant → lettres françaises |

### Identity/

| Fichier | Description |
|---|---|
| `CurrentUserService.cs` | Lit les claims de `HttpContext.User` pour identifier l'utilisateur connecté |

### DependencyInjection.cs

Méthode d'extension `AddInfrastructureServices(IConfiguration)` qui enregistre :
- DbContextFactory
- Tous les repositories
- Services externes
- Query services

---

## AppPlusPlus.Web

**Rôle :** Interface utilisateur Blazor Server. Contient UNIQUEMENT les pages Razor et le rendu. La logique métier est déléguée aux services Application.

### Program.cs — Point d'entrée

- Configure les services (DI) : `AddInfrastructureServices()` + `AddApplicationServices()` + MudBlazor
- Configure l'authentification cookie
- Charge les paramètres au démarrage
- Initialise les permissions par défaut
- Définit les endpoints `/auth/login` et `/auth/logout`
- Lance le serveur : `app.Run()`

### GlobalUsings.cs

Imports globaux des namespaces Domain pour éviter de répéter les `@using` dans chaque fichier Razor.

### Components/App.razor

Layout HTML racine (head, body, scripts). Ne contient aucune logique.

### Components/Layout/

| Fichier | Description |
|---|---|
| `MainLayout.razor` | Sidebar + barre de navigation + zone contenu. Charge les permissions utilisateur. |
| `ReconnectModal.razor` | Modal de reconnexion quand le SignalR circuit se déconnecte |

### Components/Auth/

| Fichier | Description |
|---|---|
| `RedirectToLogin.razor` | Redirige vers /login si non authentifié |

### Components/Features/ — Pages et dialogs par module

Organisé par **feature/module métier**. Chaque dossier contient la page principale ET ses dialogs associés.

#### Features/Dashboard/
| Fichier | Route | Description |
|---|---|---|
| `Home.razor` | `/` | Tableau de bord : KPIs, graphiques ventes, alertes stock, top articles |

#### Features/Vente/
| Fichier | Route | Description |
|---|---|---|
| `VenteHub.razor` | `/vente` | Hub ventes : onglets factures, paiements, livraisons |
| `Facturation.razor` | `/vente/facturation` | Création/édition facture (scanner articles, calculs) |
| `PrintFacture.razor` | `/vente/facture/print/{id}` | Impression facture (format A4) |
| `FactureForm.razor` | — (dialog) | Sélection facture existante |
| `FactureDetail.razor` | — (dialog) | Détail complet d'une facture |
| `PaiementForm.razor` | — (dialog) | Enregistrement d'un paiement |
| `PaymentDialog.razor` | — (dialog) | Historique paiements d'une facture |

#### Features/Stock/
| Fichier | Route | Description |
|---|---|---|
| `StockHub.razor` | `/stock` | Hub stock : onglets articles, types, marques, catégories, mesures, localisations |
| `ArticleFormPage.razor` | `/stock/article/new`, `/stock/article/{id}` | Création/édition article (pleine page) |
| `ArticleForm.razor` | — (dialog) | Formulaire article rapide |
| `ArticleDetailDialog.razor` | — (dialog) | Détail article avec stock par localisation |
| `StockLocationInfo.cs` | — (modèle) | DTO local pour affichage stock/localisation |

#### Features/Approvisionnement/
| Fichier | Description |
|---|---|
| `ApproForm.razor` | Formulaire approvisionnement (articles, quantités, prix) |
| `ApproExpenseForm.razor` | Ajout de frais sur un approvisionnement |
| `ApproFromCmdForm.razor` | Approvisionnement depuis une commande interne |
| `ApproFromStockForm.razor` | Approvisionnement depuis le stock existant |
| `TransformationForm.razor` | Transformation d'articles (assemblage/désassemblage) |

#### Features/Commandes/
| Fichier | Route | Description |
|---|---|---|
| `CommandeHub.razor` | `/commandes` | Hub commandes : onglets commandes clients + commandes internes |
| `CommandeForm.razor` | — (dialog) | Création/édition commande client |
| `CommandeDetailView.razor` | — (dialog) | Détail commande client |
| `CommandeEdit.razor` | `/commandes/edit/{id}` | Édition commande (pleine page) |
| `CmdForm.razor` | — (dialog) | Création commande interne |
| `CmdDetailView.razor` | — (dialog) | Détail commande interne |
| `LivraisonForm.razor` | — (dialog) | Enregistrement livraison |
| `LivraisonChecklistForm.razor` | — (dialog) | Checklist réception livraison |

#### Features/Finance/
| Fichier | Description |
|---|---|
| `ClotureForm.razor` | Formulaire de clôture journalière |
| `PeriodeForm.razor` | Création/édition période comptable |
| `TauxForm.razor` | Gestion des taux de change |

#### Features/Clients/
| Fichier | Description |
|---|---|
| `CustomerForm.razor` | Formulaire création/édition client |

#### Features/Fournisseurs/
| Fichier | Description |
|---|---|
| `SupplierForm.razor` | Formulaire création/édition fournisseur |

#### Features/Administration/
| Fichier | Route | Description |
|---|---|---|
| `AdminHub.razor` | `/admin` | Hub admin : onglets utilisateurs, rôles, activités, fonctions, localisations |
| `UserForm.razor` | — (dialog) | Création/édition utilisateur (avec checkboxes localisations) |
| `UserAccessForm.razor` | — (dialog) | Gestion accès utilisateur |
| `LocalisationDialog.razor` | — (dialog) | Création/édition localisation |
| `LocalisationDetailDialog.razor` | — (dialog) | Détail localisation |

#### Features/Rapports/
| Fichier | Route | Description |
|---|---|---|
| `RapportHub.razor` | `/rapports` | Hub rapports : ventes, bénéfices, stock, export Excel, envoi email |

#### Features/Parametres/
| Fichier | Route | Description |
|---|---|---|
| `Settings.razor` | `/settings` | Paramètres application (nom, profil boutique, SMTP) |
| `Profile.razor` | `/profile` | Profil utilisateur connecté |
| `EmailDialog.razor` | — (dialog) | Envoi email avec pièce jointe |

#### Features/Localisation/
| Fichier | Route | Description |
|---|---|---|
| `LocalisationDetail.razor` | `/localisation/{id}` | Page détail localisation : KPIs, articles, stock, utilisateurs |

#### Features/Shared/
| Fichier | Route | Description |
|---|---|---|
| `Login.razor` | `/login` | Page de connexion |
| `NotFound.razor` | `/not-found` | Page 404 |
| `Error.razor` | `/error` | Page d'erreur |

---

## AppPlusPlus.Client

**Rôle :** Shell WebAssembly minimal. Contient uniquement `_Imports.cs` pour que le serveur puisse charger l'assembly WASM.

---

## Comment ajouter un nouveau module

Exemple : ajouter un module **"Comptabilité"**

### Étape 1 — Domain : Entités
```
AppPlusPlus.Domain/Entities/Comptabilite/
    ├── CompteComptable.cs
    └── EcritureComptable.cs
```

### Étape 2 — Domain : Interface repository
```
AppPlusPlus.Domain/Interfaces/Repositories/
    └── IComptabiliteRepository.cs
```

### Étape 3 — Infrastructure : EF Configuration
```
AppPlusPlus.Infrastructure/Persistence/Configurations/
    └── CompteComptableConfiguration.cs
```

### Étape 4 — Infrastructure : DbSet
Dans `AppDbContext.cs`, ajouter :
```csharp
public DbSet<CompteComptable> ComptesComptables { get; set; }
```

### Étape 5 — Infrastructure : Repository
```
AppPlusPlus.Infrastructure/Persistence/Repositories/
    └── ComptabiliteRepository.cs
```

### Étape 6 — Application : Service
```
AppPlusPlus.Application/Services/Comptabilite/
    ├── IComptabiliteService.cs
    └── ComptabiliteService.cs
```

### Étape 7 — Application : DTOs (si nécessaire)
```
AppPlusPlus.Application/DTOs/Comptabilite/
    └── CompteComptableDto.cs
```

### Étape 8 — Infrastructure : DI
Dans `Infrastructure/DependencyInjection.cs` :
```csharp
services.AddScoped<IComptabiliteRepository, ComptabiliteRepository>();
```

### Étape 9 — Application : DI
Dans `Application/DependencyInjection.cs` :
```csharp
services.AddScoped<IComptabiliteService, ComptabiliteService>();
```

### Étape 10 — Web : Pages UI
```
AppPlusPlus.Web/Components/Features/Comptabilite/
    ├── ComptabiliteHub.razor      ← page principale (@page "/comptabilite")
    ├── CompteForm.razor           ← dialog création/édition
    └── EcritureDetailView.razor   ← dialog détail
```

### Étape 11 — Web : Usings
Dans `GlobalUsings.cs` :
```csharp
global using AppPlusPlus.Domain.Entities.Comptabilite;
```
Dans `_Imports.razor` :
```razor
@using AppPlusPlus.Web.Components.Features.Comptabilite
```

### Étape 12 — Web : Navigation (MainLayout.razor)
Ajouter le lien dans la sidebar.
