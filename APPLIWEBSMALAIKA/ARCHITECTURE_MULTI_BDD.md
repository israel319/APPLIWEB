# ARCHITECTURE MULTI-BDD - AppPlusPlus

**Date:** Juin 2026  
**Convention de nommage:** 
- **APW** = Base de données principale (TestAPP → APW, changement futur)
- **APW_GRH** = Base de données module GRH

**Objectif:** Chaque module a sa propre BDD isolée pour meilleure performance, sécurité et scalabilité

---

## 🏗️ ARCHITECTURE ACTUELLE (AVANT)

```
AVANT:
└─ TestAPP (Monolithique)
   ├─ Ventes (Factures, Paiements)
   ├─ Stock (Articles, Mouvements)
   ├─ Commandes (Clients, Livraisons)
   ├─ Approvisionnement
   ├─ Finance
   └─ ... (tous les modules)
```

**Problèmes:**
- ❌ Base de données énorme et lente
- ❌ Difficile de scaler un module indépendamment
- ❌ Sécurité: Tous les données dans un seul container
- ❌ Difficile de faire des sauvegardes/restores par module

---

## 🎯 ARCHITECTURE CIBLE (APRÈS)

```
APRÈS:
├─ TestAPP (Existant - Garder INTACTE)
│  ├─ Ventes
│  ├─ Stock
│  ├─ Commandes
│  ├─ Approvisionnement
│  ├─ Finance
│  └─ Clients/Fournisseurs
│
├─ AppPlusPlus_GRH (NOUVEAU - Module GRH isolé)
│  ├─ Employees
│  ├─ Payroll
│  ├─ Leave Management
│  ├─ Recruitment
│  ├─ Evaluation
│  ├─ Training
│  └─ Documents
│
├─ AppPlusPlus_Modules (FUTUR - Si nécessaire)
│  └─ ...
│
└─ AppPlusPlus_Analytics (FUTUR - Data warehouse)
   └─ ...
```

**Avantages:**
- ✅ Chaque module a sa BDD dédiée
- ✅ Performance indépendante par module
- ✅ Sécurité granulaire (accès BDD par module)
- ✅ Backup/Restore par module
- ✅ Équipes peuvent développer indépendamment
- ✅ Scalabilité facile

---

## 📊 ÉTAT ACTUEL DE L'IMPLÉMENTATION

### **Bases de Données**

| BDD | Module | Contenu | Status |
|-----|--------|---------|--------|
| **TestAPP** | Tous (Ventes, Stock, etc.) | 48 tables existantes | ✅ INTACTE (aucun changement) |
| **APW_GRH** | GRH seulement | 30+ tables (Emp, Paie, Congés, etc.) | ✨ CRÉÉE (nouveau) |

### **DbContexts dans le Code**

| DbContext | BDD | Couche | Entités |
|-----------|-----|--------|---------|
| `AppDbContext` | TestAPP | Infrastructure | 48 entités (existantes) |
| `GrhDbContext` | APW_GRH | Infrastructure | 30+ entités (GRH) |

### **Injection Dépendances**

| Fichier | Rôle |
|---------|------|
| `DependencyInjection.cs` | Enregistre AppDbContext + services existants |
| `DependencyInjection_GRH.cs` (NOUVEAU) | Enregistre GrhDbContext + services GRH |

---

## 🔌 COMMENT ÇA MARCHE TECHNIQUEMENT

### **1. Configuration des Connection Strings**

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TestAPP;...",
    "GRH": "Server=localhost;Database=APW_GRH;..."
  }
}
```

### **2. Enregistrement des DbContexts (Program.cs)**

```csharp
// Services EXISTANTS (TestAPP)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Services GRH (APW_GRH)
builder.Services.AddGrhServices(builder.Configuration);

// Services métier communs
builder.Services.AddApplicationServices();
```

### **3. Utilisation dans une Service**

```csharp
// Service Ventes (utilise AppDbContext - TestAPP)
public class FacturationService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    
    public FacturationService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
}

// Service Paie (utilise GrhDbContext - APW_GRH)
public class PayrollService
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;
    
    public PayrollService(IDbContextFactory<GrhDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
}
```

---

## 📋 CHECKLIST - IMPLÉMENTATION MULTI-BDD

### **Phase 1: Configuration ✅ FAIT**
- [x] Créer script SQL GRH_Schema_Creation.sql
- [x] Créer GrhDbContext.cs
- [x] Créer DependencyInjection_GRH.cs
- [x] Mettre à jour appsettings.json (ajout connection string GRH)
- [x] Mettre à jour Program.cs

### **Phase 2: Création BDD** ← À FAIRE
- [ ] Exécuter GRH_Schema_Creation.sql sur serveur SQL Server
- [ ] Vérifier que AppPlusPlus_GRH existe
- [ ] Vérifier que TestAPP est toujours intacte

### **Phase 3: Entity Models GRH** ← À FAIRE
- [ ] Créer 30+ entités C# (Employee.cs, Salary.cs, etc.)
- [ ] Créer 30+ Configurations EF Core
- [ ] Compiler et tester

### **Phase 4: Repositories GRH** ← À FAIRE
- [ ] Créer 13 Repository interfaces
- [ ] Créer 13 Repository implementations
- [ ] Tester les requêtes

### **Phase 5: Services métier GRH** ← À FAIRE
- [ ] Créer 12 Services (EmployeeService, PayrollService, etc.)
- [ ] Implémenter logique métier
- [ ] Tester validations

### **Phase 6: Pages Blazor GRH** ← À FAIRE
- [ ] Créer hubs (CompanyHub.razor, EmployeeHub.razor, etc.)
- [ ] Créer formulaires
- [ ] Intégrer MudBlazor

### **Phase 7: Tests & Déploiement** ← À FAIRE
- [ ] Tests unitaires
- [ ] Tests intégration
- [ ] Déploiement production

---

## 🔐 SÉCURITÉ MULTI-BDD

### **Avantages de sécurité:**

1. **Isolation des données**
   ```
   ✅ Données RH isolées dans APW_GRH
   ✅ Pas d'accès accidentel aux salaires depuis module Ventes
   ```

2. **Gestion des permissions granulaires**
   ```csharp
   // Seul RoleManager peut accéder à GRH
   [Authorize(Roles = "Admin, RhManager")]
   public class EmployeeHub : ComponentBase { }
   ```

3. **Backup/Restore indépendants**
   ```sql
   -- Peut sauvegarder APW_GRH sans toucher TestAPP
   BACKUP DATABASE [APW_GRH] TO DISK = 'backup_grh.bak';
   ```

---

## 🔄 AJOUTER UN AUTRE MODULE FUTUR

Quand vous voudrez ajouter un autre module (ex: Analytics), faire:

### **Étape 1: Créer script SQL**
```sql
-- NewModule_Schema_Creation.sql
IF EXISTS(SELECT * FROM sys.databases WHERE name = 'APW_Analytics')
BEGIN
    -- ... drop
END;
CREATE DATABASE APW_Analytics;
-- ... tables
```

### **Étape 2: Créer DbContext**
```csharp
// AppPlusPlus.Infrastructure/Persistence/AnalyticsDbContext.cs
public class AnalyticsDbContext : DbContext
{
    public DbSet<Report> Reports { get; set; }
    public DbSet<Dashboard> Dashboards { get; set; }
    // ...
}
```

### **Étape 3: Créer DependencyInjection**
```csharp
// AppPlusPlus.Infrastructure/DependencyInjection_Analytics.cs
public static IServiceCollection AddAnalyticsServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Enregistrer AnalyticsDbContext, repositories, services
}
```

### **Étape 4: Mettre à jour appsettings.json**
```json
"ConnectionStrings": {
    "DefaultConnection": "...",
    "GRH": "...",
    "Analytics": "Server=localhost;Database=AppPlusPlus_Analytics;..."
}
```

### **Étape 5: Mettre à jour Program.cs**
```csharp
builder.Services.AddGrhServices(builder.Configuration);
builder.Services.AddAnalyticsServices(builder.Configuration);
```

---

## 📈 SCALABILITÉ FUTURE

### **Scénario 1: Serveurs SQL différents**
```json
"ConnectionStrings": {
    "DefaultConnection": "Server=appserv1;Database=TestAPP;...",
    "GRH": "Server=appserv2;Database=APW_GRH;...",
    "Analytics": "Server=appserv3;Database=APW_Analytics;..."
}
```

### **Scénario 2: Sharding par département GRH**
```
APW_GRH_France
APW_GRH_Tunisie
APW_GRH_MarketingDept
APW_GRH_SalesDept
```

### **Scénario 3: Réplication Master-Slave**
```
APW_GRH_Primary (Write)
APW_GRH_Replica1 (Read)
APW_GRH_Replica2 (Read)
```

---

## 📝 FICHIERS MODIFIÉS/CRÉÉS

### **CRÉÉS (3 fichiers)**
| Fichier | Ligne | Description |
|---------|-------|-------------|
| `GrhDbContext.cs` | NEW | DbContext pour module GRH |
| `DependencyInjection_GRH.cs` | NEW | Enregistrement services GRH |
| `GRH_Schema_Creation.sql` | NEW | Script création BDD GRH |

### **MODIFIÉS (2 fichiers)**
| Fichier | Changement |
|---------|-----------|
| `appsettings.json` | +1 connection string (GRH) |
| `Program.cs` | +1 ligne (AddGrhServices) |

### **INTACTS (TestAPP - 0 changement)**
| Fichier | Status |
|---------|--------|
| `AppDbContext.cs` | ✅ INCHANGÉ |
| `DependencyInjection.cs` | ✅ INCHANGÉ |
| `appsettings.json (DefaultConnection)` | ✅ INCHANGÉ |
| Tous les services existants | ✅ INCHANGÉ |

---

## 🚀 PROCHAINES ÉTAPES IMMÉDIATES

### **1. Exécuter le script SQL**
```powershell
# Dans PowerShell
sqlcmd -S ".\SQLEXPRESS" -i "e:\APPLIWEB\APPLIWEB\GRH_Schema_Creation.sql"

# OU dans SSMS
# File → Open → GRH_Schema_Creation.sql → F5
```

### **2. Vérifier les BDD**
```sql
-- Dans SSMS
SELECT name FROM sys.databases WHERE name IN ('TestAPP', 'APW_GRH');
-- Doit retourner 2 lignes
```

### **3. Créer les Entity Models GRH** (prochaine phase)
```
AppPlusPlus.Domain/Entities/GRH/
├── Organization/
│   ├── Company.cs
│   ├── Department.cs
│   ├── Service.cs
│   ├── JobPosition.cs
│   ├── EmployeeCategory.cs
│   ├── EmployeeClassification.cs
│   └── Hierarchy.cs
├── Employee/
│   ├── Employee.cs
│   ├── EmployeeContract.cs
│   ├── EmployeeEducation.cs
│   └── EmployeeSkill.cs
└── ... (autres catégories)
```

---

## 📞 SUPPORT & QUESTIONS

**Q: Mon application va continuer à fonctionner avec TestAPP?**  
✅ **Oui.** Aucun changement dans TestAPP. Tous les modules existants (Ventes, Stock, etc.) continuent à fonctionner normalement.

**Q: Comment les deux BDD communiquent-elles?**  
➡️ **Elles ne communiquent pas directement.** Chaque module a son DbContext. Si besoin de données croisées (ex: Vente ↔ GRH), utiliser des APIs/Services.

**Q: Puis-je ajouter un autre module plus tard?**  
✅ **Oui.** Suivre le même pattern (DependencyInjection_ModuleName.cs, etc.). Voir section "Ajouter un autre module futur".

**Q: Comment faire des migrations EF Core?**  
```powershell
# Pour TestAPP (existant)
dotnet ef migrations add MigrationName -c AppDbContext

# Pour GRH (nouveau)
dotnet ef migrations add MigrationName -c GrhDbContext
```

**Q: Connection string OK?**  
```json
// ✅ CORRECT
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TestAPP;...",
    "GRH": "Server=localhost;Database=AppPlusPlus_GRH;..."
}
```

---

**Document créé:** Juin 2026  
**Statut:** 🔵 Architecture multi-BDD implémentée (Phase 1-2)
