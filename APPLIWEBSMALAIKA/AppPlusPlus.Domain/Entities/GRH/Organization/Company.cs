using GrhEmployee = AppPlusPlus.Domain.Entities.GRH.Employee.Employee;

namespace AppPlusPlus.Domain.Entities.GRH.Organization;

/// <summary>
/// Entité représentant une entreprise
/// </summary>
public class Company
{
    public int Id { get; set; }
    public string CompanyCode { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string? LegalName { get; set; }
    public string? Description { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Logo { get; set; }
    public string? HeadquartersLocation { get; set; }
    public int NumberOfEmployees { get; set; }
    public string Currency { get; set; } = "TND";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<JobPosition> JobPositions { get; set; } = new List<JobPosition>();
    public ICollection<GrhEmployee> Employees { get; set; } = new List<GrhEmployee>();
}
