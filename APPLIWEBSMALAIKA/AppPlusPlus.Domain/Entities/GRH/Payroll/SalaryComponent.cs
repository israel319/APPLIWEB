namespace AppPlusPlus.Domain.Entities.GRH.Payroll;

/// <summary>
/// Composante salariale (salaire de base, prime, cotisation, retenue…)
/// </summary>
public class SalaryComponent
{
    public int Id { get; set; }
    public string ComponentCode { get; set; } = null!;
    public string ComponentName { get; set; } = null!;
    public string? Description { get; set; }
    public string ComponentType { get; set; } = null!;      // Base/Allowance/Deduction/Bonus
    public string CalculationMethod { get; set; } = "Fixed"; // Fixed/Percentage
    public decimal? DefaultAmount { get; set; }
    public decimal? Percentage { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
