using GrhEmployee = AppPlusPlus.Domain.Entities.GRH.Employee.Employee;

namespace AppPlusPlus.Domain.Entities.GRH.Organization;

/// <summary>
/// Entité représentant un service/division au sein d'un département
/// </summary>
public class Service
{
    public int Id { get; set; }
    public string ServiceCode { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public int DepartmentId { get; set; }
    public int CompanyId { get; set; }
    public string? Description { get; set; }
    public int? HeadId { get; set; }
    public decimal? Budget { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Department Department { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public ICollection<GrhEmployee> Employees { get; set; } = new List<GrhEmployee>();
    public ICollection<JobPosition> JobPositions { get; set; } = new List<JobPosition>();
}
