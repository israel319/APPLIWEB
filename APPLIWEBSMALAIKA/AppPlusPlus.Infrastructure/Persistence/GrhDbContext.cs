// ============================================================================
// CONFIGURATION - MULTIPLE DBCONTEXTS
// Fichier: AppPlusPlus.Infrastructure/Persistence/GrhDbContext.cs
// ============================================================================

using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Entities.GRH.Organization;
using AppPlusPlus.Domain.Entities.GRH.Employee;
using AppPlusPlus.Domain.Entities.GRH.Leave;
using AppPlusPlus.Domain.Entities.GRH.Absence;
using AppPlusPlus.Domain.Entities.GRH.Payroll;
using AppPlusPlus.Domain.Entities.GRH.Recruitment;
using AppPlusPlus.Domain.Entities.GRH.Evaluation;
using AppPlusPlus.Domain.Entities.GRH.Training;
using AppPlusPlus.Domain.Entities.GRH.Documents;

namespace AppPlusPlus.Infrastructure.Persistence;

/// <summary>
/// DbContext isolé pour le module GRH
/// Base de données: AppPlusPlus_GRH
/// </summary>
public class GrhDbContext : DbContext
{
    public GrhDbContext(DbContextOptions<GrhDbContext> options) : base(options)
    {
    }

    // ========== ORGANIZATION ==========
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<JobPosition> JobPositions { get; set; } = null!;
    public DbSet<EmployeeCategory> EmployeeCategories { get; set; } = null!;
    public DbSet<EmployeeClassification> EmployeeClassifications { get; set; } = null!;
    public DbSet<Hierarchy> Hierarchies { get; set; } = null!;

    // ========== EMPLOYEE ==========
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<EmployeeContract> EmployeeContracts { get; set; } = null!;
    public DbSet<EmployeeEducation> EmployeeEducations { get; set; } = null!;
    public DbSet<EmployeeSkill> EmployeeSkills { get; set; } = null!;

    // ========== LEAVE & ABSENCE ==========
    public DbSet<LeaveType> LeaveTypes { get; set; } = null!;
    public DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;
    public DbSet<LeaveSaldo> LeaveSaldos { get; set; } = null!;
    public DbSet<Absence> Absences { get; set; } = null!;
    public DbSet<AbsenceType> AbsenceTypes { get; set; } = null!;

    // ========== PAYROLL ==========
    public DbSet<Salary> Salaries { get; set; } = null!;
    public DbSet<SalaryComponent> SalaryComponents { get; set; } = null!;
    public DbSet<PayrollRun> PayrollRuns { get; set; } = null!;
    public DbSet<PayrollDetail> PayrollDetails { get; set; } = null!;
    public DbSet<PaymentHistory> PaymentHistories { get; set; } = null!;

    // ========== RECRUITMENT ==========
    public DbSet<JobOpening> JobOpenings { get; set; } = null!;
    public DbSet<Candidate> Candidates { get; set; } = null!;
    public DbSet<CandidateApplication> CandidateApplications { get; set; } = null!;
    public DbSet<Interview> Interviews { get; set; } = null!;

    // ========== EVALUATION ==========
    public DbSet<PerformanceEvaluation> PerformanceEvaluations { get; set; } = null!;
    public DbSet<EvaluationCriteria> EvaluationCriteria { get; set; } = null!;
    public DbSet<EmployeeReview> EmployeeReviews { get; set; } = null!;

    // ========== TRAINING ==========
    public DbSet<TrainingProgram> TrainingPrograms { get; set; } = null!;
    public DbSet<TrainingSession> TrainingSessions { get; set; } = null!;
    public DbSet<EmployeeTraining> EmployeeTrainings { get; set; } = null!;
    public DbSet<TrainingBudget> TrainingBudgets { get; set; } = null!;

    // ========== DOCUMENTS ==========
    public DbSet<DocumentType> DocumentTypes { get; set; } = null!;
    public DbSet<EmployeeDocument> EmployeeDocuments { get; set; } = null!;
    public DbSet<EmployeeAttestation> EmployeeAttestations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Appliquer les configurations EF Core
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GrhDbContext).Assembly, 
            type => type.Namespace?.Contains("GRH") == true);

        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(entityType => entityType.GetProperties())
                     .Where(property =>
                         property.ClrType == typeof(decimal) ||
                         property.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }
    }
}
