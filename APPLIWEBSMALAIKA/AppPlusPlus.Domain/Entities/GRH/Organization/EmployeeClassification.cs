using GrhEmployee = AppPlusPlus.Domain.Entities.GRH.Employee.Employee;

namespace AppPlusPlus.Domain.Entities.GRH.Organization;

/// <summary>
/// Entité représentant une classification d'employé (type de contrat)
/// </summary>
public class EmployeeClassification
{
    public int Id { get; set; }
    public string ClassificationCode { get; set; } = null!;
    public string ClassificationName { get; set; } = null!;
    public string? Description { get; set; }
    public string? ContractType { get; set; } // Permanent/Temporary/Contract/Intern/Consultant
    public bool SocialSecurityEligible { get; set; } = true;
    public bool HealthInsuranceRequired { get; set; } = true;
    public bool BenefitsEligible { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public ICollection<GrhEmployee> Employees { get; set; } = new List<GrhEmployee>();
}
