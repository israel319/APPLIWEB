namespace AppPlusPlus.Domain.Entities.GRH.Payroll;

/// <summary>
/// Entité représentant le salaire fixe d'un employé
/// </summary>
public class Salary
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int SalaryComponentId { get; set; }
    public decimal BaseSalary { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Currency { get; set; }
    public string? PaymentFrequency { get; set; } // Monthly/Bi-weekly/Annual
    public string? SalaryBand { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee.Employee Employee { get; set; } = null!;
    public SalaryComponent SalaryComponent { get; set; } = null!;
}
