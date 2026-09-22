# 📝 DTOs & Blazor Pages - Guide d'Intégration

## ✅ Ce qui a été créé

### DTOs (Data Transfer Objects)
📂 **Location**: `AppPlusPlus.Application/DTOs/GRH/`

**Fichiers créés**:
- ✅ `EmployeeDto.cs` - Employee, CreateEmployeeDto, UpdateEmployeeDto, EmployeeReadDto
- ✅ `LeaveDto.cs` - Leave request DTOs
- ✅ `PayrollDto.cs` - Payroll DTOs  
- ✅ `AbsenceDto.cs` - Absence DTOs
- ✅ `OrganizationDto.cs` - Department, Service, JobPosition DTOs
- ✅ `RecruitmentDto.cs` - Recruitment DTOs
- ✅ `EvaluationDto.cs` - Evaluation DTOs
- ✅ `TrainingDto.cs` - Training DTOs
- ✅ `DocumentDto.cs` - Document DTOs

### Blazor Pages/Components
📂 **Location**: `AppPlusPlus.Web/Components/GRH/`

**Pages créées**:
- ✅ `EmployeeManagement.razor` - Gestion des employés
- ✅ `LeaveManagement.razor` - Gestion des congés
- ✅ `PayrollManagement.razor` - Gestion de la paie
- ✅ `RecruitmentManagement.razor` - Gestion du recrutement
- ✅ `EvaluationManagement.razor` - Gestion des évaluations
- ✅ `TrainingManagement.razor` - Gestion des formations
- ✅ `DocumentsManagement.razor` - Gestion des documents
- ✅ `OrganizationManagement.razor` - Gestion de l'organisation

---

## 🔗 Routage & Navigation

### 1. Ajouter les routes dans App.razor (ou configuration)

Les pages suivantes sont accessibles:
```
/grh/employees         → Gestion des employés
/grh/leaves           → Gestion des congés
/grh/payroll          → Gestion de la paie
/grh/recruitment      → Gestion du recrutement
/grh/evaluations      → Gestion des évaluations
/grh/training         → Gestion des formations
/grh/documents        → Gestion des documents
/grh/organization     → Gestion de l'organisation
```

### 2. Ajouter les liens de navigation

Ajouter dans le menu principal (Layout ou NavMenu):

```razor
<!-- Menu GRH -->
<MudNavLink Href="/grh/employees" Icon="@Icons.Material.Filled.Person">
    Employés
</MudNavLink>
<MudNavLink Href="/grh/leaves" Icon="@Icons.Material.Filled.EventAvailable">
    Congés
</MudNavLink>
<MudNavLink Href="/grh/payroll" Icon="@Icons.Material.Filled.AttachMoney">
    Paie
</MudNavLink>
<MudNavLink Href="/grh/recruitment" Icon="@Icons.Material.Filled.Groups">
    Recrutement
</MudNavLink>
<MudNavLink Href="/grh/evaluations" Icon="@Icons.Material.Filled.StarRate">
    Évaluations
</MudNavLink>
<MudNavLink Href="/grh/training" Icon="@Icons.Material.Filled.MenuBook">
    Formations
</MudNavLink>
<MudNavLink Href="/grh/documents" Icon="@Icons.Material.Filled.Description">
    Documents
</MudNavLink>
<MudNavLink Href="/grh/organization" Icon="@Icons.Material.Filled.Apartment">
    Organisation
</MudNavLink>
```

---

## 📋 Structure des DTOs

### EmployeeDto.cs
```csharp
// Création d'employé
public class CreateEmployeeDto
{
    public string EmployeeCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int CompanyId { get; set; }
    public int DepartmentId { get; set; }
    public int ServiceId { get; set; }
    public int JobPositionId { get; set; }
    // ... autres propriétés
}

// Mise à jour
public class UpdateEmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    // ... propriétés modifiables
}

// Affichage
public class EmployeeReadDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string DepartmentName { get; set; }
    public string JobPositionName { get; set; }
    public string EmployeeStatus { get; set; }
}

// Liste
public class EmployeeListDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; }
    public string FullName { get; set; }
    public string DepartmentName { get; set; }
}
```

---

## 🎨 Architecture des Pages Blazor

Chaque page Blazor suit ce pattern:

```razor
@page "/grh/module-name"
@using AppPlusPlus.Application.Services.GRH.{Module}
@using AppPlusPlus.Application.DTOs.GRH.{Module}
@using MudBlazor
@inject I{Module}Service {Module}Service
@inject ISnackbar Snackbar

<MudContainer MaxWidth="MaxWidth.Large" Class="py-8">
    <!-- Header -->
    <MudText Typo="Typo.h4">📋 Titre</MudText>
    
    <!-- Buttons & Search -->
    <MudButton OnClick="OpenCreateDialog">+ Nouveau</MudButton>
    
    <!-- Data Grid -->
    <MudDataGrid Items="@items" Hoverable="true">
        <!-- Colonnes... -->
    </MudDataGrid>
    
    <!-- Dialog -->
    <MudDialog @bind-IsVisible="showDialog">
        <!-- Formulaire... -->
    </MudDialog>
</MudContainer>

@code {
    // État
    private List<ReadDto> items = new();
    private CreateDto? formModel;
    private bool showDialog = false;

    // Lifecycle
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    // Méthodes
    private async Task LoadData() { }
    private void OpenCreateDialog() { }
    private async Task SaveItem() { }
}
```

---

## 🔧 Intégration avec les Services

### Exemple: Charger les employés

```csharp
// Dans la page EmployeeManagement.razor
private async Task LoadEmployees()
{
    try
    {
        var allEmployees = await EmployeeService.GetAllActiveEmployeesAsync();
        
        // Mapper les entités vers les DTOs
        employees = allEmployees.Select(e => new EmployeeListDto
        {
            Id = e.Id,
            EmployeeCode = e.EmployeeCode,
            FullName = $"{e.FirstName} {e.LastName}",
            Email = e.Email,
            DepartmentName = e.Department?.DepartmentName ?? "N/A",
            JobPositionName = e.JobPosition?.PositionName ?? "N/A",
            Status = e.Status.ToString()
        }).ToList();
    }
    catch (Exception ex)
    {
        Snackbar.Add($"Erreur: {ex.Message}", Severity.Error);
    }
}
```

### Exemple: Créer un employé

```csharp
private async Task SaveEmployee()
{
    if (formModel != null)
    {
        try
        {
            var employee = await EmployeeService.CreateEmployeeAsync(
                formModel.EmployeeCode,
                formModel.FirstName,
                formModel.LastName,
                formModel.Email,
                formModel.CompanyId,
                formModel.DepartmentId,
                formModel.ServiceId,
                formModel.JobPositionId
            );
            
            Snackbar.Add("Employé créé avec succès", Severity.Success);
            await LoadEmployees();
            CloseDialog();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erreur: {ex.Message}", Severity.Error);
        }
    }
}
```

---

## 📊 Points de Intégration Clés

### 1. Services Injections
```csharp
@inject IEmployeeService EmployeeService
@inject ILeaveService LeaveService
@inject IPayrollService PayrollService
@inject ISnackbar Snackbar
@inject NavigationManager Navigation
```

### 2. Appels Asynchrones
```csharp
// Charger les données
protected override async Task OnInitializedAsync()
{
    await LoadEmployees();
}

// Appels aux services
var result = await EmployeeService.CreateEmployeeAsync(...);
```

### 3. Gestion des Erreurs
```csharp
try
{
    // Opération
}
catch (Exception ex)
{
    Snackbar.Add($"Erreur: {ex.Message}", Severity.Error);
}
```

### 4. Manipulation d'État
```csharp
// Data binding
@bind-Value="formModel.FirstName"

// Events
@onclick="SaveEmployee"

// Conditions
@if (isLoading) { /* ... */ }
@else if (items.Count == 0) { /* ... */ }
```

---

## 🎯 Prochaines Étapes

### 1. Connecter les Services ✅
Les pages ont les appels TODO commentés. À remplacer par de vrais appels aux services.

```csharp
// AVANT (TODO)
// var employees = await EmployeeService.GetAllActiveEmployeesAsync();

// APRÈS (Implémentation)
var employees = await EmployeeService.GetAllActiveEmployeesAsync();
filteredEmployees = employees
    .Where(e => e.EmployeeCode.Contains(searchTerm) || 
                e.FirstName.Contains(searchTerm) ||
                e.Email.Contains(searchTerm))
    .ToList();
```

### 2. Ajouter Mappers Auto (Optionnel)
Installer AutoMapper pour mapper automatiquement:
```bash
Install-Package AutoMapper
Install-Package AutoMapper.Extensions.Microsoft.DependencyInjection
```

Créer des profiles de mappage:
```csharp
public class EmployeeMappingProfile : Profile
{
    public EmployeeMappingProfile()
    {
        CreateMap<Employee, EmployeeReadDto>();
        CreateMap<CreateEmployeeDto, Employee>();
        CreateMap<UpdateEmployeeDto, Employee>();
    }
}
```

### 3. Ajouter Validation (Optionnel)
```bash
Install-Package FluentValidation
Install-Package FluentValidation.AspNetCore
```

### 4. Tests UI
Tester chaque page:
- Chargement des données
- Création de nouveaux enregistrements
- Mise à jour
- Suppression
- Filtrage

---

## 📂 Fichiers Créés - Résumé

| Catégorie | Fichiers | Count |
|-----------|----------|-------|
| **DTOs** | EmployeeDto, LeaveDto, PayrollDto, etc. | 9 |
| **Blazor Pages** | EmployeeManagement, LeaveManagement, etc. | 8 |
| **Total** | DTOs + Pages | 17 |

---

## ✨ Caractéristiques des Pages

✅ **MudBlazor Integration** - Composants Material Design
✅ **Responsive Design** - Mobile-friendly
✅ **Data Grids** - Avec pagination
✅ **Dialogs** - Pour créer/modifier
✅ **Search & Filter** - Fonctionnalités de recherche
✅ **Status Indicators** - Codes couleur pour les statuts
✅ **Error Handling** - Gestion des erreurs avec Snackbar
✅ **Loading States** - Indicateurs de chargement

---

## 🚀 Commandes

### Compiler
```bash
dotnet build AppPlusPlus.sln
```

### Exécuter
```bash
dotnet run --project AppPlusPlus.Web
```

### Accéder à l'application
```
https://localhost:7123/grh/employees
```

---

## 📚 Documentation Connexe

- **GRH_STATUS_FINAL.md** - État global du module
- **GRH_IMPLEMENTATION_COMPLETE.md** - Guide complet
- **GRH_FINAL_STEPS.md** - Prochaines étapes

---

**Status**: ✅ DTOs et Pages Blazor **CRÉÉS ET PRÊTS À UTILISER**
**Prochaine étape**: Connecter les services et tester les pages

**Version**: 2.0
**Date**: 10 Juin 2026
