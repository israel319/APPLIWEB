using GrhEmployee = AppPlusPlus.Domain.Entities.GRH.Employee.Employee;

namespace AppPlusPlus.Domain.Entities.GRH.Organization;

/// <summary>
/// Entité représentant un poste/métier
/// </summary>
public class JobPosition
{
    public int Id { get; set; }
    public string PositionCode { get; set; } = null!;
    public string PositionName { get; set; } = null!;
    public int DepartmentId { get; set; }
    public int CompanyId { get; set; }
    public string? Description { get; set; }
    public decimal? SalaryRange_Min { get; set; }
    public decimal? SalaryRange_Max { get; set; }
    public string? Level { get; set; } // Junior/Middle/Senior/Lead/Manager/Director
    public string? RequiredEducation { get; set; }
    public int? RequiredExperience { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Department Department { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public ICollection<GrhEmployee> Employees { get; set; } = new List<GrhEmployee>();
}
