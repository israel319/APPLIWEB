using GrhEmployee = AppPlusPlus.Domain.Entities.GRH.Employee.Employee;

namespace AppPlusPlus.Domain.Entities.GRH.Organization;

/// <summary>
/// Entité représentant une catégorie d'employé avec ses avantages
/// </summary>
public class EmployeeCategory
{
    public int Id { get; set; }
    public string CategoryCode { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
    public int? CategoryLevel { get; set; } // 1-10
    public string? BaseSalaryBand { get; set; }
    public string? HealthInsuranceType { get; set; } // Full/Partial/None
    public decimal? PensionContributionRate { get; set; } // %
    public bool VehicleAllowance { get; set; }
    public bool TelephoneAllowance { get; set; }
    public bool FoodAllowance { get; set; }
    public decimal? FoodAllowanceAmount { get; set; }
    public bool TransportAllowance { get; set; }
    public decimal? TransportAllowanceAmount { get; set; }
    public bool HousingAllowance { get; set; }
    public decimal? HousingAllowanceAmount { get; set; }
    public bool ChildAllowance { get; set; }
    public decimal? ChildAllowanceAmount { get; set; }
    public bool PerformanceBonusEligible { get; set; }
    public int LeaveEntitlementDays { get; set; } = 20;
    public bool AnnualHealthCheckup { get; set; }
    public bool FamilyMemberInsurance { get; set; }
    public decimal? TrainingBudgetPerYear { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public ICollection<GrhEmployee> Employees { get; set; } = new List<GrhEmployee>();
}
