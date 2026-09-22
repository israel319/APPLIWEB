using AppPlusPlus.Domain.Entities.GRH.Training;
using GrhEmployee = AppPlusPlus.Domain.Entities.GRH.Employee.Employee;

namespace AppPlusPlus.Domain.Entities.GRH.Organization;

/// <summary>
/// Entité représentant un département
/// </summary>
public class Department
{
    public int Id { get; set; }
    public string DepartmentCode { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
    public int CompanyId { get; set; }
    public string? Description { get; set; }
    public int? HeadId { get; set; }
    public string? Location { get; set; }
    public decimal? Budget { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Company Company { get; set; } = null!;
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<JobPosition> JobPositions { get; set; } = new List<JobPosition>();
    public ICollection<GrhEmployee> Employees { get; set; } = new List<GrhEmployee>();
    public ICollection<TrainingBudget> TrainingBudgets { get; set; } = new List<TrainingBudget>();
}
