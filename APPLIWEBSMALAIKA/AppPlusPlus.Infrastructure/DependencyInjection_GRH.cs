// ============================================================================
// CONFIGURATION - INJECTION DE DÉPENDANCES GRH
// Fichier: AppPlusPlus.Infrastructure/DependencyInjection_GRH.cs
// ============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Application.Services.GRH;
using AppPlusPlus.Infrastructure.Persistence;
using AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

namespace AppPlusPlus.Infrastructure;

/// <summary>
/// Extension pour enregistrer les services GRH dans le conteneur DI
/// </summary>
public static class DependencyInjectionGRH
{
    /// <summary>
    /// Enregistre tous les services et repositories du module GRH
    /// </summary>
    public static IServiceCollection AddGrhServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ========== DBCONTEXT GRH ==========
        // Enregistrer le DbContext GRH avec la connection string dédiée
        var grhConnectionString = configuration.GetConnectionString("GRH");
        if (string.IsNullOrWhiteSpace(grhConnectionString)
            || grhConnectionString.Contains("VOTRE_", StringComparison.OrdinalIgnoreCase))
        {
            return services;
        }

        // AddDbContextFactory crée un nouveau GrhDbContext par opération → pas de concurrence Blazor Server
        services.AddDbContextFactory<GrhDbContext>(options =>
            options.UseSqlServer(grhConnectionString,
                sqlOptions => sqlOptions
                    .MigrationsAssembly("AppPlusPlus.Infrastructure")
                    .MigrationsHistoryTable("__EFMigrationsHistory_GRH")));

        // ========== REPOSITORIES GRH ==========
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IJobPositionRepository, JobPositionRepository>();
        services.AddScoped<IEmployeeCategoryRepository, EmployeeCategoryRepository>();
        services.AddScoped<IEmployeeClassificationRepository, EmployeeClassificationRepository>();
        services.AddScoped<ILeaveRepository, LeaveRepository>();
        services.AddScoped<IPayrollRepository, PayrollRepository>();
        services.AddScoped<IAbsenceRepository, AbsenceRepository>();
        services.AddScoped<IRecruitmentRepository, RecruitmentRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddScoped<ITrainingRepository, TrainingRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
        services.AddScoped<IAbsenceTypeRepository, AbsenceTypeRepository>();
        services.AddScoped<IEmployeeContractRepository, EmployeeContractRepository>();
        services.AddScoped<ISalaryComponentRepository, SalaryComponentRepository>();
        services.AddScoped<IPayrollDetailRepository, PayrollDetailRepository>();

        // ========== SERVICES MÉTIER GRH ==========
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IAbsenceService, AbsenceService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IJobPositionService, JobPositionService>();
        services.AddScoped<IRecruitmentService, RecruitmentService>();
        services.AddScoped<IEvaluationService, EvaluationService>();
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<ILeaveTypeService, LeaveTypeService>();
        services.AddScoped<IAbsenceTypeService, AbsenceTypeService>();
        services.AddScoped<IEmployeeCategoryService, EmployeeCategoryService>();
        services.AddScoped<IEmployeeClassificationService, EmployeeClassificationService>();
        services.AddScoped<ISalaryComponentService, SalaryComponentService>();

        return services;
    }
}
