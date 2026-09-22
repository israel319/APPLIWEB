# Analyse Complète - Module GRH (Gestion des Ressources Humaines)

**Date d'analyse :** Juin 2026  
**Dernière mise à jour :** Juin 2026 (Ajout: Company, Service, EmployeeCategory, EmployeeClassification)  
**Statut :** 🔵 En attente de validation client

---

## 📋 Résumé des Entités Ajoutées

### ✅ NOUVELLES ENTITÉS INTÉGRÉES

| Entité | Table | Description |
|--------|-------|-------------|
| **Company** | T_GRH_Companies | Gestion multi-entreprise - permet de gérer plusieurs entreprises |
| **Service** | T_GRH_Services | Division/Section au sein d'un département |
| **EmployeeCategory** | T_GRH_EmployeeCategories | **Catégories avec avantages & inconvénients spécifiques** |
| **EmployeeClassification** | T_GRH_EmployeeClassifications | **Classification de contrats (CDI/CDD/Stage/Consultant) avec avantages/inconvénients** |

---

## 📊 Structure Hiérarchique (Nouveau)

```
Company
  ├─ Department
  │    ├─ Service
  │    │    ├─ Employee (avec CategoryId + ClassificationId)
  │    │    └─ JobPosition
  │    └─ JobPosition
  └─ Employee

EmployeeCategory (avantages/allocations spécifiques)
  └─ Employee (relation 1:N)

EmployeeClassification (type contrat)
  └─ Employee (relation 1:N)
```

---

## 🎯 Avantages & Inconvénients par Catégorie d'Employé

### **Matrice Comparative - EmployeeCategory**

| Catégorie | Avantages ✅ | Inconvénients ⚠️ | Salaire | Niveau |
|-----------|---|---|---|---|
| **Cadre Supérieur** | Voiture, Téléphone, Logement, Assurance famille, Budget formation, Prime performance | Astreinte, Responsabilités étendues, Imputabilité forte | 3000-6000 | 9-10 |
| **Cadre Moyen** | Téléphone, Transport, Formation annuelle, Prime | Charges managériales, Pression objectifs | 1500-3000 | 7-8 |
| **Technicien/Spécialiste** | Téléphone, Prime performance, Formation | Projets critiques, Astreinte technique | 1000-2000 | 6-7 |
| **Employé Standard** | Assurance base, Transport, Panier repas | Pas de voiture, Avantages limités | 600-1200 | 3-5 |
| **Stagiaire/Apprenti** | Formation, Encadrement, Apprentissage | Contrat limité, Pas de CNSS, Responsabilités réduites | 300-500 | 1-2 |

### **Matrice Comparative - EmployeeClassification**

| Classification | Avantages ✅ | Inconvénients ⚠️ | Avantages Sociaux | Durée |
|---|---|---|---|---|
| **CDI** | Stabilité, Tous avantages, Retraite, Indemnités | Coûts fixes, Difficulté licenciement | ✅ Complets | Indéfinie |
| **CDD** | Flexibilité, Coûts variables, Facilité séparation | Pas de stabilité, Avantages réduits | ⚠️ Réduits | 6-24 mois |
| **Prestataire** | Expertise, Pas d'engagement, Flexibilité max | Coûts élevés, Pas de loyauté | ❌ Aucuns | Variable |
| **Stagiaire** | Coût faible, Formation, Pas CNSS | Durée limitée, Productivité réduite | ❌ Aucuns | 3-6 mois |
| **Consultant** | Expertise spécialisée, Flexibilité 100% | Coûts très élevés, Pas loyauté | ❌ Aucuns | Projet |

---

## 📍 Détails des Avantages & Inconvénients

### **EmployeeCategory (T_GRH_EmployeeCategories) - Avantages gérés:**
```
Colonnes spécifiques:
├─ HealthInsuranceType (Full/Partial/None)
├─ PensionContributionRate (%)
├─ VehicleAllowance (bool)
├─ TelephoneAllowance (bool)
├─ FoodAllowance + Amount
├─ TransportAllowance + Amount
├─ HousingAllowance + Amount
├─ ChildAllowance + Amount
├─ PerformanceBonusEligible (bool)
├─ LeaveEntitlementDays (int)
├─ AnnualHealthCheckup (bool)
├─ FamilyMemberInsurance (bool)
└─ TrainingBudgetPerYear (decimal)
```

### **EmployeeClassification (T_GRH_EmployeeClassifications) - Avantages/Droits:**
```
Colonnes:
├─ SocialSecurityEligible (bool) → CNSS
├─ HealthInsuranceRequired (bool) → Assurance maladie
├─ BenefitsEligible (bool) → Accès avantages
└─ ContractType → Stabilité (CDI/CDD/Contract/Intern/Consultant)
```

---

## 📊 TABLE DE SYNTHÈSE - OÙ TROUVER CHAQUE ENTITÉ

| Entité | Catégorie | Type | Status |
|--------|-----------|------|--------|
| **Company** | Structure Org. | Entreprises | ✨ Nouvelle |
| **Department** | Structure Org. | Département | ✏️ Mise à jour |
| **Service** | Structure Org. | Division service | ✨ Nouvelle |
| **JobPosition** | Structure Org. | Postes/Métiers | ✏️ Mise à jour |
| **EmployeeCategory** | Structure Org. | Catégories + avantages | ✨ Nouvelle |
| **EmployeeClassification** | Structure Org. | Classification contrats | ✨ Nouvelle |
| **Employee** | Données Employé | Données principales | ✏️ Mise à jour FK |
| **LeaveType** | Congés | Types de congés | - |
| **LeaveRequest** | Congés | Demandes congés | - |
| **Salary** | Paie | Salaires | - |
| **PayrollRun** | Paie | Paie mensuelle | - |

---

## 🔑 RELATIONS CLÉS (Nouveau Schema)

```
Employee:
  ├─ N:1 → Company (NEW)
  ├─ N:1 → Department
  ├─ N:1 → Service (NEW, nullable)
  ├─ N:1 → JobPosition
  ├─ N:1 → EmployeeCategory (NEW) ← AVANTAGES/ALLOCATIONS
  └─ N:1 → EmployeeClassification (NEW) ← TYPE CONTRAT
```

---

## ✅ POINTS À CONFIRMER AVEC LE CLIENT

**Entités Organisationnelles:**
- [ ] Company: Gérer plusieurs entreprises/filiales?
- [ ] Service: Nécessaire ou Department suffit?
- [ ] EmployeeCategory: Avantages listés sont corrects?
- [ ] EmployeeClassification: Tous types contrats couverts?

**Avantages & Inconvénients:**
- [ ] Avantages par catégorie corrects?
- [ ] Override avantages par département?
- [ ] Gérer allocations négociables individuellement?

---

Le module GRH gérera **toutes les aspects de la gestion des ressources humaines** avec une structure complète intégrant :
- ✅ Gestion des employés et leurs données personnelles
- ✅ Structure organisationnelle (départements, postes, hiérarchie)
- ✅ Gestion des congés et absences
- ✅ Paie et salaires
- ✅ Recrutement et gestion des candidatures
- ✅ Évaluations et performance
- ✅ Formations et développement professionnel
- ✅ Contrats de travail
- ✅ Documents et attestations RH
- ✅ Gestion des compétences

---

## 2. Architecture proposée (suivant le pattern Clean Architecture)

```
GRH Module Structure
├── Domain/Entities/GRH/
│   ├── Employee/
│   │   ├── Employee.cs               (entité principale)
│   │   ├── EmployeeContract.cs       (contrats de travail)
│   │   ├── EmployeeEducation.cs      (formations/études)
│   │   └── EmployeeSkill.cs          (compétences)
│   ├── Organization/
│   │   ├── Department.cs
│   │   ├── JobPosition.cs
│   │   └── Hierarchy.cs
│   ├── Leave/
│   │   ├── LeaveType.cs              (types de congés)
│   │   ├── LeaveRequest.cs           (demandes de congés)
│   │   └── LeaveSaldo.cs             (soldes de congés)
│   ├── Absence/
│   │   ├── Absence.cs
│   │   └── AbsenceType.cs
│   ├── Payroll/
│   │   ├── Salary.cs
│   │   ├── PayrollRun.cs             (paie mensuelle)
│   │   ├── PayrollDetail.cs          (détails lignes paie)
│   │   ├── SalaryComponent.cs        (éléments de paie: brut, net, etc.)
│   │   ├── Deduction.cs              (déductions: impôts, CNSS, etc.)
│   │   └── PaymentHistory.cs         (historique versements)
│   ├── Recruitment/
│   │   ├── Candidate.cs
│   │   ├── JobOpening.cs
│   │   ├── Recruitment.cs            (processus de recrutement)
│   │   ├── Interview.cs
│   │   └── CandidateApplication.cs
│   ├── Evaluation/
│   │   ├── PerformanceEvaluation.cs
│   │   ├── EvaluationCriteria.cs
│   │   └── EmployeeReview.cs
│   ├── Training/
│   │   ├── TrainingProgram.cs
│   │   ├── TrainingSession.cs
│   │   ├── EmployeeTraining.cs
│   │   └── TrainingBudget.cs
│   ├── Documents/
│   │   ├── EmployeeDocument.cs       (documents RH)
│   │   ├── DocumentType.cs
│   │   └── EmployeeAttestation.cs    (certificats, attestations)
│   └── Shared/
│       ├── EmployeeStatus.cs         (actif, inactif, suspendu, etc.)
│       ├── ContractType.cs
│       ├── LeaveStatus.cs
│       └── RecruitmentStatus.cs
├── Application/Services/GRH/
│   ├── ICompanyService.cs            + CompanyService.cs
│   ├── IDepartmentService.cs         + DepartmentService.cs
│   ├── IServiceService.cs            + ServiceService.cs
│   ├── IEmployeeService.cs           + EmployeeService.cs
│   ├── IEmployeeCategoryService.cs   + EmployeeCategoryService.cs
│   ├── IEmployeeClassificationService.cs + EmployeeClassificationService.cs
│   ├── ILeaveService.cs              + LeaveService.cs
│   ├── IAbsenceService.cs            + AbsenceService.cs
│   ├── IPayrollService.cs            + PayrollService.cs
│   ├── IRecruitmentService.cs        + RecruitmentService.cs
│   ├── IEvaluationService.cs         + EvaluationService.cs
│   ├── ITrainingService.cs           + TrainingService.cs
│   └── IDocumentService.cs           + DocumentService.cs
├── Infrastructure/Persistence/GRH/
│   ├── Configurations/               (IEntityTypeConfiguration pour chaque entité)
│   ├── Repositories/
│   │   ├── ICompanyRepository.cs     + CompanyRepository.cs
│   │   ├── IDepartmentRepository.cs  + DepartmentRepository.cs
│   │   ├── IServiceRepository.cs     + ServiceRepository.cs
│   │   ├── IEmployeeRepository.cs    + EmployeeRepository.cs
│   │   ├── IEmployeeCategoryRepository.cs + EmployeeCategoryRepository.cs
│   │   ├── IEmployeeClassificationRepository.cs + EmployeeClassificationRepository.cs
│   │   ├── ILeaveRepository.cs       + LeaveRepository.cs
│   │   ├── IPayrollRepository.cs     + PayrollRepository.cs
│   │   ├── IRecruitmentRepository.cs + RecruitmentRepository.cs
│   │   └── ... (autres repositories spécialisés)
│   └── QueryServices/
│       ├── PayrollQueryService.cs    (requêtes complexes paie)
│       ├── RecruitmentQueryService.cs
│       └── EmployeeQueryService.cs
└── Web/Components/Features/GRH/
    ├── CompanyHub.razor              (/grh/companies)
    ├── DepartmentHub.razor           (/grh/departments)
    ├── ServiceHub.razor              (/grh/services)
    ├── EmployeeHub.razor             (/grh/employees)
    ├── EmployeeCategoryHub.razor     (/grh/categories)
    ├── EmployeeClassificationHub.razor (/grh/classifications)
    ├── LeaveManagement.razor         (/grh/leaves)
    ├── PayrollHub.razor              (/grh/payroll)
    ├── RecruitmentHub.razor          (/grh/recruitment)
    ├── EvaluationHub.razor           (/grh/evaluation)
    ├── TrainingHub.razor             (/grh/training)
    └── DocumentsHub.razor            (/grh/documents)
```

---

## 3. Entités détaillées et structure de données

### 3.1 Entités Employé et Données Personnelles

#### **Employee (T_GRH_Employees)**
Entité principale représentant un employé

```
Champs principaux:
├─ Id (PK, Auto-increment)
├─ EmployeeCode (string, 50, UNIQUE) -- Code unique (ex: EMP001)
├─ FirstName (string, 50, REQUIRED)
├─ LastName (string, 50, REQUIRED)
├─ Email (string, 100, UNIQUE)
├─ Phone (string, 20)
├─ BirthDate (DateTime)
├─ BirthPlace (string, 100)
├─ Gender (enum: M/F)
├─ MaritalStatus (enum: Single/Married/Divorced/Widowed)
├─ Nationality (string, 50)
├─ IdNumber (string, 50) -- Numéro CIN/Passeport
├─ IdType (enum: CIN/Passport/Other)
├─ JoiningDate (DateTime, REQUIRED)
├─ CompanyId (FK -> Company, REQUIRED)
├─ DepartmentId (FK -> Department, REQUIRED)
├─ ServiceId (FK -> Service, nullable)
├─ JobPositionId (FK -> JobPosition, REQUIRED)
├─ CategoryId (FK -> EmployeeCategory)
├─ ClassificationId (FK -> EmployeeClassification)
├─ ReportingTo (FK -> Employee) -- Supérieur hiérarchique
├─ EmployeeStatus (enum: Active/Inactive/OnLeave/Suspended/Terminated)
├─ StatusChangedDate (DateTime)
├─ EmploymentType (enum: FullTime/PartTime/Contract/Intern)
├─ Address (string, 200)
├─ City (string, 50)
├─ ZipCode (string, 10)
├─ EmergencyContact (string, 100)
├─ EmergencyPhone (string, 20)
├─ BankAccountNumber (string, 50)
├─ BankName (string, 100)
├─ CreatedAt (DateTime)
├─ CreatedBy (FK -> User)
├─ UpdatedAt (DateTime)
└─ UpdatedBy (FK -> User)

Relations:
├─ Company (N:1)
├─ Department (N:1)
├─ Service (N:1)
├─ JobPosition (N:1)
├─ EmployeeCategory (N:1)
├─ EmployeeClassification (N:1)
├─ Manager/Subordinates (N:1, 1:N)
├─ EmployeeContracts (1:N)
├─ Salaries (1:N)
├─ LeaveRequests (1:N)
├─ Absences (1:N)
├─ PerformanceEvaluations (1:N)
├─ TrainingParticipations (1:N)
├─ EmployeeEducations (1:N)
├─ EmployeeSkills (1:N)
├─ EmployeeDocuments (1:N)
└─ PayrollHistories (1:N)
```

#### **EmployeeContract (T_GRH_EmployeeContracts)**
Gestion des contrats de travail

```
Champs:
├─ Id (PK)
├─ EmployeeId (FK -> Employee, REQUIRED)
├─ ContractType (enum: CDI/CDD/Stage/Apprenticeship)
├─ ContractNumber (string, 50, UNIQUE)
├─ StartDate (DateTime, REQUIRED)
├─ EndDate (DateTime) -- NULL pour CDI
├─ SalaryAmount (decimal, REQUIRED)
├─ Position (string, 100)
├─ Department (string, 100)
├─ WorkingHoursPerWeek (decimal, default: 40)
├─ ContractStatus (enum: Active/Expired/Terminated)
├─ ContractPdf (string) -- Chemin/URL du fichier PDF
├─ TerminationReason (string, 200)
├─ TerminationDate (DateTime)
├─ Notes (text)
├─ CreatedAt (DateTime)
└─ CreatedBy (FK -> User)

Relations:
└─ Employee (N:1)
```

#### **EmployeeEducation (T_GRH_EmployeeEducations)**
Formation/Études des employés

```
Champs:
├─ Id (PK)
├─ EmployeeId (FK -> Employee, REQUIRED)
├─ EducationType (enum: HighSchool/Bachelor/Master/Doctorate/Certificate)
├─ InstitutionName (string, 100)
├─ FieldOfStudy (string, 100)
├─ StartDate (DateTime)
├─ EndDate (DateTime)
├─ Grade (string, 10) -- Note/GPA
├─ Certificate (string) -- Chemin du diplôme PDF
├─ Description (text)
└─ CreatedAt (DateTime)
```

#### **EmployeeSkill (T_GRH_EmployeeSkills)**
Compétences des employés

```
Champs:
├─ Id (PK)
├─ EmployeeId (FK -> Employee, REQUIRED)
├─ SkillName (string, 50)
├─ ProficiencyLevel (enum: Beginner/Intermediate/Advanced/Expert)
├─ YearsOfExperience (int)
├─ Verified (bool) -- Compétence vérifiée par manager
├─ VerifiedBy (FK -> User)
├─ VerifiedDate (DateTime)
└─ Description (text)
```

### 3.2 Entités Structure Organisationnelle

#### **Company (T_GRH_Companies)**
Entreprises (gestion multi-entreprise)

```
Champs:
├─ Id (PK)
├─ CompanyCode (string, 50, UNIQUE)
├─ CompanyName (string, 100, REQUIRED)
├─ LegalName (string, 150)
├─ Description (text)
├─ RegistrationNumber (string, 50) -- Numéro d'immatriculation
├─ TaxId (string, 50) -- Numéro d'identification fiscale
├─ Address (string, 200)
├─ City (string, 50)
├─ ZipCode (string, 10)
├─ Country (string, 50)
├─ Phone (string, 20)
├─ Email (string, 100)
├─ Website (string, 200)
├─ Logo (string) -- Chemin logo
├─ HeadquartersLocation (string, 100)
├─ NumberOfEmployees (int)
├─ Currency (string, 3, default: "TND")
├─ IsActive (bool, default: true)
├─ CreatedAt (DateTime)
└─ UpdatedAt (DateTime)

Relations:
├─ Employees (1:N)
├─ Departments (1:N)
├─ Services (1:N)
└─ JobPositions (1:N)
```

#### **Service (T_GRH_Services)**
Services/Divisions au sein d'un département

```
Champs:
├─ Id (PK)
├─ ServiceCode (string, 50, UNIQUE)
├─ ServiceName (string, 100, REQUIRED)
├─ DepartmentId (FK -> Department, REQUIRED)
├─ CompanyId (FK -> Company, REQUIRED)
├─ Description (text)
├─ HeadId (FK -> Employee) -- Chef du service
├─ Budget (decimal)
├─ Location (string, 100)
├─ IsActive (bool, default: true)
├─ CreatedAt (DateTime)
└─ UpdatedAt (DateTime)

Relations:
├─ Department (N:1)
├─ Company (N:1)
├─ Employees (1:N)
└─ Head (N:1 to Employee)

Exemples:
├─ Department Ventes → Service Ventes Directes
├─ Department Ventes → Service Ventes B2B
├─ Department IT → Service Infrastructure
├─ Department IT → Service Development
```

#### **Department (T_GRH_Departments)**
Départements/Services principaux

```
Champs:
├─ Id (PK)
├─ DepartmentCode (string, 50, UNIQUE)
├─ DepartmentName (string, 100, REQUIRED)
├─ Description (text)
├─ HeadId (FK -> Employee) -- Chef de département
├─ Location (string, 100)
├─ Budget (decimal)
├─ CreatedAt (DateTime)
└─ UpdatedAt (DateTime)

Relations:
├─ Employees (1:N)
├─ JobPositions (1:N)
└─ Head (N:1 to Employee)
```

#### **EmployeeCategory (T_GRH_EmployeeCategories)**
Catégories d'employés (avec avantages et inconvénients spécifiques)

```
Champs:
├─ Id (PK)
├─ CategoryCode (string, 50, UNIQUE)
├─ CategoryName (string, 100, REQUIRED) 
├─ Description (text)
├─ CategoryLevel (int, 1-10) -- Niveau hiérarchique
├─ BaseSalaryBand (string, 50) -- Grille salariale de base
├─ HealthInsuranceType (enum: Full/Partial/None)
├─ PensionContributionRate (decimal) -- %
├─ VehicleAllowance (bool)
├─ TelephoneAllowance (bool)
├─ FoodAllowance (bool)
├─ FoodAllowanceAmount (decimal, nullable)
├─ TransportAllowance (bool)
├─ TransportAllowanceAmount (decimal, nullable)
├─ HousingAllowance (bool)
├─ HousingAllowanceAmount (decimal, nullable)
├─ ChildAllowance (bool)
├─ ChildAllowanceAmount (decimal, nullable)
├─ PerformanceBonusEligible (bool)
├─ LeaveEntitlementDays (int)
├─ AnnualHealthCheckup (bool)
├─ FamilyMemberInsurance (bool)
├─ TrainingBudgetPerYear (decimal)
├─ IsActive (bool, default: true)
├─ CreatedAt (DateTime)
└─ UpdatedAt (DateTime)

Relations:
└─ Employees (1:N)

Exemples de catégories:
├─ Cadre Supérieur (Director/VP)
│  ├─ Avantages: Voiture ✅, Téléphone ✅, Logement ✅, Assurance famille ✅, Budget formation élevé ✅
│  ├─ Salaire: 3000-6000 TND
│  └─ Inconvénients: Astreinte ⚠️, Responsabilités étendues ⚠️
│
├─ Cadre Moyen (Manager/Lead)
│  ├─ Avantages: Téléphone ✅, Allocation transport ✅, Formation annuelle ✅
│  ├─ Salaire: 1500-3000 TND
│  └─ Inconvénients: Charges managériales ⚠️
│
├─ Technicien/Spécialiste (Senior Developer)
│  ├─ Avantages: Allocation téléphone ✅, Prime performance ✅, Formation ✅
│  ├─ Salaire: 1000-2000 TND
│  └─ Inconvénients: Projets critiques ⚠️
│
├─ Employé Standard (Junior/Middle Staff)
│  ├─ Avantages: Assurance base ✅, Allocation transport ✅
│  ├─ Salaire: 600-1200 TND
│  └─ Inconvénients: Pas de voiture de fonction ⚠️
│
└─ Stagiaire/Apprenti
   ├─ Avantages: Formation complète ✅
   ├─ Salaire: 300-500 TND
   └─ Inconvénients: Contrat limité ⚠️, Responsabilités réduites ⚠️, Pas d'avantages sociaux ⚠️
```

#### **EmployeeClassification (T_GRH_EmployeeClassifications)**
Classification des employés (statut du contrat)

```
Champs:
├─ Id (PK)
├─ ClassificationCode (string, 50, UNIQUE)
├─ ClassificationName (string, 100, REQUIRED)
├─ Description (text)
├─ ContractType (enum: Permanent/Temporary/Contract/Intern/Consultant)
├─ SocialSecurityEligible (bool) -- Couverture CNSS
├─ HealthInsuranceRequired (bool)
├─ BenefitsEligible (bool)
├─ IsActive (bool, default: true)
├─ CreatedAt (DateTime)
└─ UpdatedAt (DateTime)

Relations:
└─ Employees (1:N)

Exemples de classifications:
├─ Permanent/CDI
│  ├─ Avantages: Stabilité ✅, Tous les avantages ✅, Retraite ✅
│  └─ Inconvénients: Coûts élevés pour l'entreprise ⚠️
│
├─ Temporaire/CDD
│  ├─ Avantages: Flexibilité ✅, Coûts réduits ✅
│  └─ Inconvénients: Pas de stabilité ⚠️, Avantages limités ⚠️
│
├─ Contractuel/Contract
│  ├─ Avantages: Flexibilité ✅, Services spécialisés ✅
│  └─ Inconvénients: Dépendance aux prestataires ⚠️, Coûts variables ⚠️
│
├─ Stagiaire/Intern
│  ├─ Avantages: Coût très faible ✅, Formation ✅, Pas de CNSS ✅
│  └─ Inconvénients: Durée limitée ⚠️, Productivité réduite ⚠️, Besoin de supervision ⚠️
│
└─ Consultant
   ├─ Avantages: Expertise spécialisée ✅, Pas d'engagement long terme ✅
   └─ Inconvénients: Coûts élevés ⚠️, Pas de loyauté ⚠️
```

#### **JobPosition (T_GRH_JobPositions)**
Postes/Métiers

```
Champs:
├─ Id (PK)
├─ PositionCode (string, 50, UNIQUE)
├─ PositionName (string, 100, REQUIRED)
├─ DepartmentId (FK -> Department, REQUIRED)
├─ CompanyId (FK -> Company, REQUIRED)
├─ Description (text)
├─ SalaryRange_Min (decimal)
├─ SalaryRange_Max (decimal)
├─ Level (enum: Junior/Middle/Senior/Lead/Manager/Director)
├─ RequiredEducation (string, 100)
├─ RequiredExperience (int) -- En années
├─ IsActive (bool, default: true)
├─ CreatedAt (DateTime)
└─ UpdatedAt (DateTime)

Relations:
├─ Department (N:1)
├─ Company (N:1)
└─ Employees (1:N)
```

#### **Hierarchy (T_GRH_Hierarchies)**
Structure hiérarchique

```
Champs:
├─ Id (PK)
├─ EmployeeId (FK -> Employee, REQUIRED)
├─ SupervisedBy (FK -> Employee, REQUIRED)
├─ EffectiveDate (DateTime)
└─ EndDate (DateTime, nullable)

Relations:
├─ Employee (N:1)
└─ Supervisor (N:1 to Employee)

⚠️ Alternative: Utiliser Employee.ReportingTo (FK) pour simplifier
```

### 3.3 Entités Congés et Absences

#### **LeaveType (T_GRH_LeaveTypes)**
Types de congés (Congés payés, Maladie, Maternité, etc.)

```
Champs:
├─ Id (PK)
├─ LeaveCode (string, 50, UNIQUE)
├─ LeaveTypeName (string, 100, REQUIRED)
├─ Description (text)
├─ DaysAllowedPerYear (int, REQUIRED)
├─ IsPaid (bool, default: true)
├─ RequiresApproval (bool, default: true)
├─ MaxConsecutiveDays (int)
├─ CarryoverDays (int) -- Jours reportables d'une année à l'autre
├─ IsActive (bool, default: true)
├─ CreatedAt (DateTime)
└─ UpdatedAt (DateTime)

Exemple de données:
├─ Congés payés (20 jours/an, payé, approbation)
├─ Congés maladie (illimité, payé, justificatif)
├─ Congé maternité (90 jours, payé)
├─ Congé paternité (7 jours, payé)
├─ Congé sans solde (illimité, non-payé, approbation)
└─ Congé formation (5 jours/an, payé)
```

#### **LeaveRequest (T_GRH_LeaveRequests)**
Demandes de congés

```
Champs:
├─ Id (PK)
├─ EmployeeId (FK -> Employee, REQUIRED)
├─ LeaveTypeId (FK -> LeaveType, REQUIRED)
├─ StartDate (DateTime, REQUIRED)
├─ EndDate (DateTime, REQUIRED)
├─ NumberOfDays (decimal) -- Peut être 0.5 (demi-journée)
├─ Reason (text)
├─ LeaveStatus (enum: Draft/Pending/Approved/Rejected/Cancelled)
├─ SubmittedAt (DateTime)
├─ ApprovedAt (DateTime)
├─ ApprovedBy (FK -> User)
├─ RejectionReason (text)
├─ Notes (text)
├─ Attachment (string) -- Pièce jointe (justificatif)
├─ CreatedAt (DateTime)
└─ CreatedBy (FK -> User)

Relations:
├─ Employee (N:1)
└─ LeaveType (N:1)
```

#### **LeaveSaldo (T_GRH_LeaveSaldos)**
Soldes de congés par employé et type

```
Champs:
├─ Id (PK)
├─ EmployeeId (FK -> Employee, REQUIRED)
├─ LeaveTypeId (FK -> LeaveType, REQUIRED)
├─ Year (int, REQUIRED)
├─ AllowedDays (decimal)
├─ UsedDays (decimal)
├─ ApprovedDays (decimal)
├─ PendingDays (decimal)
├─ AvailableDays (decimal) -- Computed: AllowedDays - UsedDays - ApprovedDays
├─ CarriedOverDays (decimal) -- Jours reportés de l'année précédente
├─ LastUpdated (DateTime)
└─ UpdatedBy (FK -> User)

Relations:
├─ Employee (N:1)
└─ LeaveType (N:1)
```

#### **Absence (T_GRH_Absences)**
Absences non planifiées

```
Champs:
├─ Id (PK)
├─ EmployeeId (FK -> Employee, REQUIRED)
├─ AbsenceTypeId (FK -> AbsenceType, REQUIRED)
├─ AbsenceDate (DateTime, REQUIRED)
├─ Duration (enum: FullDay/HalfDay/Hours)
├─ Hours (int) -- Si Duration = Hours
├─ Reason (text)
├─ AbsenceStatus (enum: Reported/Justified/Pending/Unjustified)
├─ Justification (text)
├─ Attachment (string) -- Justificatif médical, etc.
├─ ReportedAt (DateTime)
├─ ReportedBy (FK -> User)
├─ ReproachState (enum: None/Warned/UnderInvestigation)
├─ Notes (text)
├─ CreatedAt (DateTime)
└─ CreatedBy (FK -> User)

Relations:
├─ Employee (N:1)
└─ AbsenceType (N:1)
```

#### **AbsenceType (T_GRH_AbsenceTypes)**
Types d'absences

```
Champs:
├─ Id (PK)
├─ AbsenceCode (string, 50, UNIQUE)
├─ AbsenceTypeName (string, 100)
├─ Description (text)
├─ IsPaid (bool)
└─ IsActive (bool)

Exemple:
├─ Retard (Unpaid)
├─ Absence non justifiée (Unpaid)
├─ Absence justifiée (Paid)
└─ Absence médicale (Paid)
```

### 3.4 Entités Paie et Salaires

#### **Salary (T_GRH_Salaries)**
Salaires courants des employés

```
Champs:
├─ Id (PK)
├─ EmployeeId (FK -> Employee, REQUIRED, UNIQUE)
├─ ContractId (FK -> EmployeeContract)
├─ BaseSalary (decimal, REQUIRED)
├─ Currency (string, 3, default: "TND")
├─ EffectiveDate (DateTime)
├─ EndDate (DateTime, nullable)
├─ SalaryBand (string, 50) -- Grille salariale
├─ PaymentFrequency (enum: Monthly/Bi-weekly/Weekly)
├─ PaymentMethod (enum: BankTransfer/Check/Cash)
├─ BankAccountNumber (string, 50)
├─ CreatedAt (DateTime)
└─ UpdatedAt (DateTime)

Relations:
└─ Employee (1:1)
```

#### **SalaryComponent (T_GRH_SalaryComponents)**
Éléments composant la paie

```
Champs:
├─ Id (PK)
├─ ComponentCode (string, 50, UNIQUE)
├─ ComponentName (string, 100, REQUIRED)
├─ ComponentType (enum: Earning/Deduction/Tax/Insurance)
├─ Description (text)
├─ Percentage (decimal) -- Si applicable (ex: 5% bonus)
├─ FixedAmount (decimal) -- Si applicable
├─ IsDefault (bool) -- Inclus automatiquement dans paie
├─ CalculationMethod (enum: Fixed/Percentage/Manual)
├─ IsActive (bool)
├─ CreatedAt (DateTime)
└─ UpdatedAt (DateTime)

Exemples:
├─ Salaire de base (Earning, Fixed)
├─ Heures supplémentaires (Earning, Hourly)
├─ Prime d'ancienneté (Earning, Percentage)
├─ Impôt sur le revenu (Tax, Percentage)
├─ Cotisation CNSS (Deduction, Percentage)
├─ Cotisation mutuelle (Insurance, Fixed)
└─ Avance sur salaire (Deduction, Fixed)
```

#### **Deduction (T_GRH_Deductions)**
Déductions applicables aux employés

```
Champs:
├─ Id (PK)
├─ EmployeeId (FK -> Employee, REQUIRED)
├─ DeductionTypeId (FK -> SalaryComponent)
├─ Amount (decimal, REQUIRED)
├─ StartDate (DateTime)
├─ EndDate (DateTime)
├─ Reason (string, 200)
├─ ApprovedBy (FK -> User)
├─ ApprovedAt (DateTime)
├─ Notes (text)
├─ CreatedAt (DateTime)
└─ CreatedBy (FK -> User)

Exemples:
├─ Retenue pour avance
├─ Retenue pour absence injustifiée
├─ Sanction disciplinaire
└─ Saisie-arrêt
```

#### **PayrollRun (T_GRH_PayrollRuns)**
Paie mensuelle/périodique

```
Champs:
├─ Id (PK)
├─ PayrollCode (string, 50, UNIQUE)
├─ PeriodStartDate (DateTime, REQUIRED)
├─ PeriodEndDate (DateTime, REQUIRED)
├─ PaymentDate (DateTime)
├─ PayrollStatus (enum: Draft/Prepared/Approved/Paid/Cancelled)
├─ NumberOfEmployees (int)
├─ TotalGrossSalary (decimal)
├─ TotalDeductions (decimal)
├─ TotalNetSalary (decimal)
├─ TotalTaxes (decimal)
├─ TotalInsurance (decimal)
├─ PreparedAt (DateTime)
├─ PreparedBy (FK -> User)
├─ ApprovedAt (DateTime)
├─ ApprovedBy (FK -> User)
├─ PaidAt (DateTime)
├─ Notes (text)
├─ CreatedAt (DateTime)
└─ CreatedBy (FK -> User)

Relations:
└─ PayrollDetails (1:N)
```

#### **PayrollDetail (T_GRH_PayrollDetails)**
Détails de paie par employé

```
Champs:
├─ Id (PK)
├─ PayrollRunId (FK -> PayrollRun, REQUIRED)
├─ EmployeeId (FK -> Employee, REQUIRED)
├─ BaseSalary (decimal)
├─ Gross (decimal) -- Total gains
├─ NetSalary (decimal) -- Salaire net à verser
├─ TaxAmount (decimal)
├─ InsuranceAmount (decimal)
├─ OtherDeductions (decimal)
├─ DetailJson (text) -- JSON avec breakdown détaillé des composantes
├─ PaymentStatus (enum: Pending/Paid/Cancelled)
├─ PaymentDate (DateTime)
├─ PaymentMethod (string)
├─ Reference (string) -- Référence de virement
├─ Notes (text)
├─ CreatedAt (DateTime)
└─ CreatedBy (FK -> User)

DetailJson structure (example):
{
  "earnings": [
    { "componentId": 1, "componentName": "Salaire de base", "amount": 1000 },
    { "componentId": 2, "componentName": "Prime", "amount": 100 }
  ],
  "deductions": [
    { "componentId": 3, "componentName": "CNSS", "amount": 50 },
    { "componentId": 4, "componentName": "Impôt", "amount": 80 }
  ]
}

Relations:
├─ PayrollRun (N:1)
└─ Employee (N:1)
```

#### **PaymentHistory (T_GRH_PaymentHistories)**
Historique des versements

```
Champs:
├─ Id (PK)
├─ PayrollDetailId (FK -> PayrollDetail, REQUIRED)
├─ EmployeeId (FK -> Employee, REQUIRED)
├─ PaymentDate (DateTime, REQUIRED)
├─ Amount (decimal, REQUIRED)
├─ PaymentMethod (enum: BankTransfer/Check/Cash)
├─ Reference (string) -- Numéro de chèque, numéro de virement
├─ BankName (string)
├─ Status (enum: Pending/Confirmed/Failed/Reversed)
├─ Notes (text)
├─ CreatedAt (DateTime)
└─ CreatedBy (FK -> User)

Relations:
├─ PayrollDetail (N:1)
└─ Employee (N:1)
```

### 3.8 Entités Documents et Attestations

#### **EmployeeDocument (T_GRH_EmployeeDocuments)**
Documents RH

```
Champs:
├─ Id (PK)
├─ EmployeeId (FK -> Employee, REQUIRED)
├─ DocumentTypeId (FK -> DocumentType, REQUIRED)
├─ DocumentName (string, 100, REQUIRED)
├─ DocumentFile (string) -- Chemin du fichier
├─ FileSize (long)
├─ MimeType (string, 50)
├─ UploadDate (DateTime)
├─ ExpiryDate (DateTime) -- Si doc. avec validité
├─ IsRequired (bool)
├─ Status (enum: Valid/Expired/Pending/Rejected)
├─ Notes (text)
├─ UploadedBy (FK -> User)
├─ CreatedAt (DateTime)
└─ UpdatedAt (DateTime)

Relations:
├─ Employee (N:1)
└─ DocumentType (N:1)
```

#### **DocumentType (T_GRH_DocumentTypes)**
Types de documents

```
Champs:
├─ Id (PK)
├─ DocumentCode (string, 50, UNIQUE)
├─ DocumentName (string, 100, REQUIRED)
├─ Description (text)
├─ IsRequired (bool) -- Obligatoire pour tous les employés
├─ Periodicity (enum: OneTime/Annual/Biennial/OnDemand)
├─ ReminderDays (int) -- Rappel X jours avant expiration
├─ MaxFileSize (long) -- En MB
├─ AllowedFormats (string) -- Ex: "pdf,doc,docx"
├─ IsActive (bool)
├─ CreatedAt (DateTime)
└─ UpdatedAt (DateTime)

Exemple:
├─ Contrat de travail (OneTime)
├─ Certificat médical (Annual)
├─ Certificat de sécurité (Biennial)
├─ Justificatif de domicile (Annual)
└─ Attestation CIN (Biennial)
```

#### **EmployeeAttestation (T_GRH_EmployeeAttestations)**
Attestations et certificats

```
Champs:
├─ Id (PK)
├─ AttestationCode (string, 50, UNIQUE)
├─ EmployeeId (FK -> Employee, REQUIRED)
├─ AttestationType (enum: Salary/Service/Unemployment/Resignation/Other)
├─ IssuingDate (DateTime)
├─ Purpose (text)
├─ ValidityPeriod (int) -- En jours
├─ ExpiryDate (DateTime)
├─ Template (string) -- Chemin template PDF
├─ GeneratedFile (string) -- Fichier généré
├─ SignedBy (FK -> User)
├─ SigningDate (DateTime)
├─ Notes (text)
├─ CreatedAt (DateTime)
└─ CreatedBy (FK -> User)

Relations:
└─ Employee (N:1)
```

### 3.9 Entités Statuts Partagés

#### **EmployeeStatus (T_GRH_EmployeeStatuses)**
Statuts d'employés

```
Valeurs:
├─ Active (Actif)
├─ Inactive (Inactif)
├─ OnLeave (En congé)
├─ Suspended (Suspendu)
├─ Terminated (Résilié)
└─ OnMaternity (En congé maternité)
```

#### **ContractType (T_GRH_ContractTypes)**
Types de contrats

```
Valeurs:
├─ CDI (Contrat à durée indéterminée)
├─ CDD (Contrat à durée déterminée)
├─ Stage (Stage)
└─ Apprenticeship (Apprentissage)
```

#### **LeaveStatus (T_GRH_LeaveStatuses)**
Statuts des demandes de congés

```
Valeurs:
├─ Draft (Brouillon)
├─ Pending (En attente)
├─ Approved (Approuvé)
├─ Rejected (Rejeté)
└─ Cancelled (Annulé)
```

#### **RecruitmentStatus (T_GRH_RecruitmentStatuses)**
Statuts de recrutement

```
Valeurs:
├─ Applied (Candidaturé)
├─ UnderReview (Examen)
├─ Shortlisted (Présélectionné)
├─ Interviewed (Entretenu)
├─ OfferExtended (Offre envoyée)
├─ Hired (Embauché)
└─ Rejected (Rejeté)
```

---

## 4. Procédures Stockées (SQL Server)

### 4.1 Procédures de Paie

#### **sp_CalculatePayroll**
Calcule la paie pour une période donnée

```sql
CREATE PROCEDURE sp_CalculatePayroll
    @PayrollRunId INT,
    @PeriodStartDate DATETIME,
    @PeriodEndDate DATETIME,
    @CalculatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Récupérer tous les employés actifs
        DECLARE @EmployeeId INT;
        DECLARE @BaseSalary DECIMAL(18,2);
        DECLARE @Gross DECIMAL(18,2);
        DECLARE @TotalDeductions DECIMAL(18,2);
        DECLARE @TaxAmount DECIMAL(18,2);
        DECLARE @NetSalary DECIMAL(18,2);
        
        -- Cursor pour chaque employé
        DECLARE emp_cursor CURSOR FOR
            SELECT e.Id, s.BaseSalary
            FROM T_GRH_Employees e
            INNER JOIN T_GRH_Salaries s ON e.Id = s.EmployeeId
            WHERE e.EmployeeStatus = 'Active';
        
        OPEN emp_cursor;
        FETCH NEXT FROM emp_cursor INTO @EmployeeId, @BaseSalary;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Calculer les gains
            SET @Gross = dbo.CalculateEarnings(@EmployeeId, @BaseSalary);
            
            -- Calculer les impôts et déductions
            SET @TaxAmount = dbo.CalculateTax(@Gross);
            SET @TotalDeductions = dbo.CalculateDeductions(@EmployeeId, @TaxAmount);
            
            -- Salaire net
            SET @NetSalary = @Gross - @TotalDeductions;
            
            -- Insérer dans PayrollDetail
            INSERT INTO T_GRH_PayrollDetails 
                (PayrollRunId, EmployeeId, BaseSalary, Gross, TaxAmount, 
                 OtherDeductions, NetSalary, PaymentStatus, CreatedBy, CreatedAt)
            VALUES 
                (@PayrollRunId, @EmployeeId, @BaseSalary, @Gross, @TaxAmount,
                 @TotalDeductions - @TaxAmount, @NetSalary, 'Pending', @CalculatedBy, GETDATE());
            
            -- Mise à jour de la paie globale du payroll run
            UPDATE T_GRH_PayrollRuns 
            SET TotalGrossSalary = TotalGrossSalary + @Gross,
                TotalDeductions = TotalDeductions + @TotalDeductions,
                TotalNetSalary = TotalNetSalary + @NetSalary,
                TotalTaxes = TotalTaxes + @TaxAmount,
                NumberOfEmployees = NumberOfEmployees + 1
            WHERE Id = @PayrollRunId;
            
            FETCH NEXT FROM emp_cursor INTO @EmployeeId, @BaseSalary;
        END;
        
        CLOSE emp_cursor;
        DEALLOCATE emp_cursor;
        
        -- Mise à jour du statut
        UPDATE T_GRH_PayrollRuns 
        SET PayrollStatus = 'Prepared'
        WHERE Id = @PayrollRunId;
        
        COMMIT TRANSACTION;
        
        SELECT 'SUCCESS' AS Result, 'Payroll calculated successfully' AS Message;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
```

#### **sp_UpdateLeaveSaldo**
Mise à jour des soldes de congés

```sql
CREATE PROCEDURE sp_UpdateLeaveSaldo
    @EmployeeId INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @LeaveTypeId INT;
    DECLARE @AllowedDays INT;
    DECLARE @UsedDays DECIMAL(5,1);
    DECLARE @ApprovedDays DECIMAL(5,1);
    
    -- Cursor pour chaque type de congé
    DECLARE leave_cursor CURSOR FOR
        SELECT Id, DaysAllowedPerYear FROM T_GRH_LeaveTypes WHERE IsActive = 1;
    
    OPEN leave_cursor;
    FETCH NEXT FROM leave_cursor INTO @LeaveTypeId, @AllowedDays;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Calculer les jours utilisés
        SET @UsedDays = (
            SELECT ISNULL(SUM(NumberOfDays), 0)
            FROM T_GRH_LeaveRequests
            WHERE EmployeeId = @EmployeeId 
                AND LeaveTypeId = @LeaveTypeId
                AND YEAR(StartDate) = @Year
                AND LeaveStatus IN ('Approved', 'Cancelled') -- Seulement approuvés
        );
        
        -- Calculer les jours en attente
        SET @ApprovedDays = (
            SELECT ISNULL(SUM(NumberOfDays), 0)
            FROM T_GRH_LeaveRequests
            WHERE EmployeeId = @EmployeeId 
                AND LeaveTypeId = @LeaveTypeId
                AND YEAR(StartDate) = @Year
                AND LeaveStatus = 'Pending'
        );
        
        -- Upsert LeaveSaldo
        IF EXISTS (SELECT 1 FROM T_GRH_LeaveSaldos 
                  WHERE EmployeeId = @EmployeeId AND LeaveTypeId = @LeaveTypeId AND Year = @Year)
        BEGIN
            UPDATE T_GRH_LeaveSaldos
            SET UsedDays = @UsedDays,
                ApprovedDays = @ApprovedDays,
                LastUpdated = GETDATE()
            WHERE EmployeeId = @EmployeeId AND LeaveTypeId = @LeaveTypeId AND Year = @Year;
        END
        ELSE
        BEGIN
            INSERT INTO T_GRH_LeaveSaldos 
                (EmployeeId, LeaveTypeId, Year, AllowedDays, UsedDays, ApprovedDays, PendingDays, LastUpdated)
            VALUES 
                (@EmployeeId, @LeaveTypeId, @Year, @AllowedDays, @UsedDays, @ApprovedDays, 0, GETDATE());
        END;
        
        FETCH NEXT FROM leave_cursor INTO @LeaveTypeId, @AllowedDays;
    END;
    
    CLOSE leave_cursor;
    DEALLOCATE leave_cursor;
END;
```

#### **sp_ProcessMonthlyAbsences**
Traitement des absences mensuelles (calcul déductions)

```sql
CREATE PROCEDURE sp_ProcessMonthlyAbsences
    @Month INT,
    @Year INT,
    @DeductionTypeId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Récupérer les absences non justifiées
        INSERT INTO T_GRH_Deductions (EmployeeId, DeductionTypeId, Amount, StartDate, EndDate, Reason, CreatedAt, CreatedBy)
        SELECT 
            a.EmployeeId,
            @DeductionTypeId,
            (CAST(CASE WHEN a.Duration = 'FullDay' THEN s.BaseSalary / 22
                      WHEN a.Duration = 'HalfDay' THEN (s.BaseSalary / 22) / 2
                      ELSE (s.BaseSalary / 22 / 8) * a.Hours
                 END AS DECIMAL(18,2))) AS Amount,
            DATEFROMPARTS(@Year, @Month, 1),
            EOMONTH(DATEFROMPARTS(@Year, @Month, 1)),
            'Absence non justifiée',
            GETDATE(),
            1
        FROM T_GRH_Absences a
        INNER JOIN T_GRH_Employees e ON a.EmployeeId = e.Id
        INNER JOIN T_GRH_Salaries s ON e.Id = s.EmployeeId
        WHERE MONTH(a.AbsenceDate) = @Month
            AND YEAR(a.AbsenceDate) = @Year
            AND a.AbsenceStatus = 'Unjustified'
            AND a.Duration IN ('FullDay', 'HalfDay', 'Hours');
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
```

### 4.2 Procédures de Congés

#### **sp_AutoApproveAnnualLeave**
Approbation automatique des congés payés après délai (3 jours)

```sql
CREATE PROCEDURE sp_AutoApproveAnnualLeave
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        UPDATE T_GRH_LeaveRequests
        SET LeaveStatus = 'Approved',
            ApprovedAt = GETDATE(),
            ApprovedBy = 1 -- Admin user
        WHERE LeaveStatus = 'Pending'
            AND LeaveTypeId IN (SELECT Id FROM T_GRH_LeaveTypes WHERE LeaveCode = 'ANNUAL')
            AND DATEDIFF(DAY, SubmittedAt, GETDATE()) >= 3
            AND StartDate > GETDATE();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
```

#### **sp_ValidateLeaveAvailability**
Valide qu'un employé a suffisamment de congés

```sql
CREATE PROCEDURE sp_ValidateLeaveAvailability
    @EmployeeId INT,
    @LeaveTypeId INT,
    @RequestedDays DECIMAL(5,1),
    @Year INT,
    @IsValid BIT OUTPUT,
    @AvailableDays DECIMAL(5,1) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @AllowedDays INT;
    DECLARE @UsedDays DECIMAL(5,1);
    DECLARE @ApprovedDays DECIMAL(5,1);
    
    -- Récupérer le saldo
    SELECT @AllowedDays = AllowedDays,
           @UsedDays = UsedDays,
           @ApprovedDays = ApprovedDays
    FROM T_GRH_LeaveSaldos
    WHERE EmployeeId = @EmployeeId 
        AND LeaveTypeId = @LeaveTypeId 
        AND Year = @Year;
    
    -- Calculer les jours disponibles
    SET @AvailableDays = @AllowedDays - @UsedDays - @ApprovedDays;
    
    -- Vérifier suffisance
    IF @AvailableDays >= @RequestedDays
        SET @IsValid = 1;
    ELSE
        SET @IsValid = 0;
END;
```

### 4.3 Procédures de Rapports et Statistiques

#### **sp_GetEmployeePayrollHistory**
Historique de paie d'un employé

```sql
CREATE PROCEDURE sp_GetEmployeePayrollHistory
    @EmployeeId INT,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pr.PayrollCode,
        pr.PeriodStartDate,
        pr.PeriodEndDate,
        pd.BaseSalary,
        pd.Gross,
        pd.TaxAmount,
        pd.InsuranceAmount,
        pd.OtherDeductions,
        pd.NetSalary,
        pd.PaymentStatus,
        pd.PaymentDate,
        pd.Reference
    FROM T_GRH_PayrollDetails pd
    INNER JOIN T_GRH_PayrollRuns pr ON pd.PayrollRunId = pr.Id
    WHERE pd.EmployeeId = @EmployeeId
        AND pr.PeriodStartDate >= ISNULL(@FromDate, '1900-01-01')
        AND pr.PeriodEndDate <= ISNULL(@ToDate, GETDATE())
    ORDER BY pr.PeriodStartDate DESC;
END;
```

#### **sp_GetLeaveStatisticsByDepartment**
Statistiques de congés par département

```sql
CREATE PROCEDURE sp_GetLeaveStatisticsByDepartment
    @DepartmentId INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        d.DepartmentName,
        lt.LeaveTypeName,
        COUNT(DISTINCT e.Id) AS EmployeeCount,
        SUM(ls.AllowedDays) AS TotalAllowedDays,
        SUM(ls.UsedDays) AS TotalUsedDays,
        SUM(ls.ApprovedDays) AS TotalApprovedDays,
        SUM(ls.AllowedDays) - SUM(ls.UsedDays) - SUM(ls.ApprovedDays) AS AvailableDays
    FROM T_GRH_Employees e
    INNER JOIN T_GRH_Departments d ON e.DepartmentId = d.Id
    INNER JOIN T_GRH_LeaveSaldos ls ON e.Id = ls.EmployeeId
    INNER JOIN T_GRH_LeaveTypes lt ON ls.LeaveTypeId = lt.Id
    WHERE d.Id = @DepartmentId
        AND ls.Year = @Year
    GROUP BY d.DepartmentName, lt.LeaveTypeName
    ORDER BY d.DepartmentName, lt.LeaveTypeName;
END;
```

#### **sp_GetRecruitmentMetrics**
Métriques de recrutement

```sql
CREATE PROCEDURE sp_GetRecruitmentMetrics
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        jo.JobTitle,
        jo.DepartmentId,
        COUNT(DISTINCT c.Id) AS TotalApplications,
        SUM(CASE WHEN ca.ApplicationStatus = 'Hired' THEN 1 ELSE 0 END) AS Hired,
        SUM(CASE WHEN ca.ApplicationStatus = 'Rejected' THEN 1 ELSE 0 END) AS Rejected,
        SUM(CASE WHEN ca.ApplicationStatus IN ('Applied', 'UnderReview', 'Shortlisted', 'Interviewed', 'OfferExtended') THEN 1 ELSE 0 END) AS InProgress,
        CAST(SUM(CASE WHEN ca.ApplicationStatus = 'Hired' THEN 1 ELSE 0 END) * 100.0 / COUNT(DISTINCT c.Id) AS DECIMAL(5,2)) AS HiringRate
    FROM T_GRH_JobOpenings jo
    LEFT JOIN T_GRH_CandidateApplications ca ON jo.Id = ca.JobOpeningId
    LEFT JOIN T_GRH_Candidates c ON ca.CandidateId = c.Id
    WHERE jo.OpeningDate >= ISNULL(@FromDate, '1900-01-01')
        AND jo.OpeningDate <= ISNULL(@ToDate, GETDATE())
    GROUP BY jo.JobTitle, jo.DepartmentId
    ORDER BY jo.JobTitle;
END;
```

### 4.4 Procédures de Maintenance

#### **sp_ArchiveOldPayrolls**
Archivage des paies anciennes (plus de 3 ans)

```sql
CREATE PROCEDURE sp_ArchiveOldPayrolls
    @RetentionYears INT = 3
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @ArchiveDate DATETIME = DATEADD(YEAR, -@RetentionYears, GETDATE());
        
        -- Insérer dans table d'archive (à créer)
        INSERT INTO T_GRH_PayrollArchive
        SELECT * FROM T_GRH_PayrollRuns WHERE PeriodEndDate < @ArchiveDate;
        
        -- Supprimer les originaux
        DELETE FROM T_GRH_PayrollRuns WHERE PeriodEndDate < @ArchiveDate;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
```

#### **sp_GenerateAnnualLeaveReset**
Réinitialisation des congés annuels (le 1er janvier)

```sql
CREATE PROCEDURE sp_GenerateAnnualLeaveReset
    @CurrentYear INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @AnnualLeaveTypeId INT = (SELECT Id FROM T_GRH_LeaveTypes WHERE LeaveCode = 'ANNUAL');
        DECLARE @CarryoverDays INT = (SELECT CarryoverDays FROM T_GRH_LeaveTypes WHERE Id = @AnnualLeaveTypeId);
        
        -- Réinsérer les soldes pour la nouvelle année
        INSERT INTO T_GRH_LeaveSaldos (EmployeeId, LeaveTypeId, Year, AllowedDays, UsedDays, ApprovedDays, PendingDays, CarriedOverDays, LastUpdated)
        SELECT 
            ls.EmployeeId,
            ls.LeaveTypeId,
            @CurrentYear,
            lt.DaysAllowedPerYear,
            0,
            0,
            0,
            -- Reporter les jours inutilisés de l'année précédente (limité à CarryoverDays)
            CASE WHEN (ls.AllowedDays - ls.UsedDays - ls.ApprovedDays) > @CarryoverDays 
                 THEN @CarryoverDays 
                 ELSE (ls.AllowedDays - ls.UsedDays - ls.ApprovedDays)
            END,
            GETDATE()
        FROM T_GRH_LeaveSaldos ls
        INNER JOIN T_GRH_LeaveTypes lt ON ls.LeaveTypeId = lt.Id
        INNER JOIN T_GRH_Employees e ON ls.EmployeeId = e.Id
        WHERE ls.Year = @CurrentYear - 1
            AND lt.Id = @AnnualLeaveTypeId
            AND e.EmployeeStatus = 'Active'
            AND NOT EXISTS (SELECT 1 FROM T_GRH_LeaveSaldos WHERE EmployeeId = ls.EmployeeId AND Year = @CurrentYear AND LeaveTypeId = @AnnualLeaveTypeId);
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
```

### 4.5 Procédures de Sécurité et Audit

#### **sp_LogEmployeeAction**
Enregistrement des actions RH importantes

```sql
CREATE PROCEDURE sp_LogEmployeeAction
    @EmployeeId INT,
    @ActionType VARCHAR(50),
    @Description NVARCHAR(MAX),
    @PerformedBy INT,
    @OldValues NVARCHAR(MAX) = NULL,
    @NewValues NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO T_GRH_AuditLog (EmployeeId, ActionType, Description, OldValues, NewValues, PerformedBy, ActionDate)
    VALUES (@EmployeeId, @ActionType, @Description, @OldValues, @NewValues, @PerformedBy, GETDATE());
END;
```

#### **sp_ValidatePayrollConsistency**
Validation de la cohérence des données de paie

```sql
CREATE PROCEDURE sp_ValidatePayrollConsistency
    @PayrollRunId INT,
    @ErrorCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @ErrorCount = 0;
    
    -- Vérifier que le total des détails égale le total du payroll
    DECLARE @DetailTotal DECIMAL(18,2) = (SELECT SUM(NetSalary) FROM T_GRH_PayrollDetails WHERE PayrollRunId = @PayrollRunId);
    DECLARE @PayrollTotal DECIMAL(18,2) = (SELECT TotalNetSalary FROM T_GRH_PayrollRuns WHERE Id = @PayrollRunId);
    
    IF @DetailTotal != @PayrollTotal
    BEGIN
        SET @ErrorCount = @ErrorCount + 1;
        PRINT 'ERROR: Payroll totals do not match';
    END;
    
    -- Vérifier qu'il n'y a pas de salaires négatifs
    IF EXISTS (SELECT 1 FROM T_GRH_PayrollDetails WHERE PayrollRunId = @PayrollRunId AND NetSalary < 0)
    BEGIN
        SET @ErrorCount = @ErrorCount + 1;
        PRINT 'ERROR: Found negative salaries';
    END;
END;
```

---

## 5. Indices et Performance

### 5.1 Indices recommandés

```sql
-- Recherche rapide par employé
CREATE INDEX IX_Employee_DepartmentId ON T_GRH_Employees(DepartmentId);
CREATE INDEX IX_Employee_JobPositionId ON T_GRH_Employees(JobPositionId);
CREATE INDEX IX_Employee_EmployeeStatus ON T_GRH_Employees(EmployeeStatus);

-- Recherche de congés
CREATE INDEX IX_LeaveRequest_EmployeeId_Year ON T_GRH_LeaveRequests(EmployeeId, YEAR(StartDate));
CREATE INDEX IX_LeaveRequest_Status ON T_GRH_LeaveRequests(LeaveStatus);

-- Recherche de paie
CREATE INDEX IX_PayrollDetail_PayrollRunId ON T_GRH_PayrollDetails(PayrollRunId);
CREATE INDEX IX_PayrollRun_PeriodDates ON T_GRH_PayrollRuns(PeriodStartDate, PeriodEndDate);

-- Recherche de candidatures
CREATE INDEX IX_CandidateApplication_JobOpeningId ON T_GRH_CandidateApplications(JobOpeningId);
CREATE INDEX IX_CandidateApplication_Status ON T_GRH_CandidateApplications(ApplicationStatus);

-- Recherche d'absences
CREATE INDEX IX_Absence_EmployeeId_Date ON T_GRH_Absences(EmployeeId, AbsenceDate);
```

---

## 6. Intégrations et Workflows

### 6.1 Workflows critiques

1. **Processus d'embauche**
   - Création offre → Candidatures → Entretiens → Sélection → Contrat → Création employé → Intégration

2. **Processus de paie**
   - Données de base → Éléments variables → Calcul paie → Approbation → Paiement → Vérification

3. **Gestion congés**
   - Demande de congé → Approbation → Mise à jour saldo → Notifications → Retour travail

4. **Évaluation performance**
   - Période d'évaluation → Auto-évaluation → Évaluation manager → Avis 360° → Décisions (augmentation, promotion)

### 6.2 Notifications à prévoir

- Demande de congé approuvée/rejetée
- Absence enregistrée
- Rappel fiche de paie
- Rappel renouvellement certificats
- Rappel formation obligatoire
- Fin de contrat

---

## 7. DTOs recommandées (Application Layer)

```csharp
// Employee DTOs
public record EmployeeCreateDto(
    string EmployeeCode, string FirstName, string LastName, 
    string Email, DateTime JoiningDate, int DepartmentId, int JobPositionId
);

public record EmployeeListDto(
    int Id, string EmployeeCode, string FullName, 
    string Email, string Department, string Position, string Status
);

public record EmployeeDetailDto(
    int Id, string EmployeeCode, string FirstName, string LastName,
    string Email, string Phone, DateTime BirthDate, string Gender,
    int DepartmentId, int JobPositionId, EmployeeStatus Status,
    IEnumerable<EmployeeContractDto> Contracts,
    IEnumerable<LeaveRequestDto> LeaveRequests
);

// Payroll DTOs
public record PayrollRunDto(
    int Id, string PayrollCode, DateTime PeriodStartDate, DateTime PeriodEndDate,
    decimal TotalGrossSalary, decimal TotalNetSalary, string Status
);

public record PayrollDetailDto(
    int EmployeeId, string EmployeeName, decimal BaseSalary,
    decimal Gross, decimal TaxAmount, decimal NetSalary, string PaymentStatus
);

// Leave DTOs
public record LeaveRequestDto(
    int Id, int EmployeeId, string LeaveTypeName, DateTime StartDate,
    DateTime EndDate, decimal NumberOfDays, string Status
);

public record LeaveAvailabilityDto(
    int EmployeeId, string LeaveTypeName, int AllowedDays,
    int UsedDays, int AvailableDays, int Year
);
```

---

## 8. Sécurité et Permissions

### 8.1 Permissions recommandées

```csharp
public static class GrhPermissions
{
    // Employees
    public const string ViewEmployees = "GRH.Employees.View";
    public const string CreateEmployee = "GRH.Employees.Create";
    public const string EditEmployee = "GRH.Employees.Edit";
    public const string DeleteEmployee = "GRH.Employees.Delete";
    
    // Payroll
    public const string ViewPayroll = "GRH.Payroll.View";
    public const string CreatePayroll = "GRH.Payroll.Create";
    public const string ApprovePayroll = "GRH.Payroll.Approve";
    public const string ProcessPayment = "GRH.Payroll.Process";
    
    // Leave Management
    public const string ApproveLeave = "GRH.Leave.Approve";
    public const string ViewLeaveReport = "GRH.Leave.Report";
    
    // Recruitment
    public const string ManageRecruitment = "GRH.Recruitment.Manage";
    
    // Performance
    public const string PerformEvaluation = "GRH.Evaluation.Perform";
}
```

### 8.2 Validation des données sensibles

- Masquer les numéros de compte bancaire en affichage (afficher que les 4 derniers chiffres)
- Chiffrer les salaires en base de données (considérer)
- Auditer tous les accès aux données salariales
- Limiter les exports à administration/RH
- Audit des suppressions de documents

---

## 9. Validation de l'Analyse

### ✅ À confirmer avec le client:

**Données personnelles:**
- [ ] Inclure photo d'identité/passeport ?
- [ ] Numéro de sécurité sociale / identification fiscale ?
- [ ] Adresse du domicile ou adresse professionnelle seule ?
- [ ] Données médicales (groupe sanguin, allergies) ?

**Paie et Avantages:**
- [ ] Types de déductions spécifiques à votre contexte (CNSS, impôts, etc.) ?
- [ ] Prime de performance/bonus ? Structure ?
- [ ] Heures supplémentaires ? Comment calculer ?
- [ ] Indemnité de licenciement ?

**Congés:**
- [ ] Types de congés spécifiques (maternité étendue, sabbatique, etc.) ?
- [ ] Politique de report de congés ?
- [ ] Congés optionnels spécifiques à l'industrie/pays ?

**Recrutement:**
- [ ] Pipeline de recrutement complet ou simplifié ?
- [ ] Tests techniques/psychotechniques requis ?
- [ ] Vérifications d'antécédents ?

**Autres domaines:**
- [ ] Gestion des compétences : obligatoire ?
- [ ] Programme de mentorat/coaching ?
- [ ] Évaluation 360° obligatoire ?
- [ ] Communication interne (annonces, notifications) ?
- [ ] Export de données RH vers système tiers ?

**Conformité et Audit:**
- [ ] Audit complet requis pour toutes les modifications ?
- [ ] Conformité RGPD/protection des données ?
- [ ] Archivage des données anciennes ?

---

## 10. Prochaines Étapes

1. **Validation client** - Confirmer l'analyse avec le propriétaire du produit
2. **Conception BDD** - Scripts SQL de création
3. **Implémentation Domain** - Créer toutes les entités
4. **Implémentation Infrastructure** - Repositories et configurations EF Core
5. **Implémentation Application** - Services et logique métier
6. **Implémentation Web** - Pages Razor et composants Blazor
7. **Tests** - Unitaires, intégration, acceptation
8. **Déploiement** - Migration BDD + lancement module

---

**Document créé:** Juin 2026  
**Version:** 1.0 (Analyse préliminaire)  
**Statut:** 🔴 En attente de validation client
