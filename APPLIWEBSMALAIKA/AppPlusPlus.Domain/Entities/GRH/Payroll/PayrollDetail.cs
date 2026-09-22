namespace AppPlusPlus.Domain.Entities.GRH.Payroll;

/// <summary>
/// Ligne de bulletin de paie par employé pour un PayrollRun
/// </summary>
public class PayrollDetail
{
    public int Id { get; set; }
    public int PayrollRunId { get; set; }
    public int EmployeeId { get; set; }

    // Montants calculés (colonnes réelles de T_GRH_PayrollDetails)
    public decimal BaseSalary { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal InsuranceAmount { get; set; }
    public decimal OtherDeductions { get; set; }

    public string? PaymentStatus { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public PayrollRun PayrollRun { get; set; } = null!;
    public Employee.Employee Employee { get; set; } = null!;
}
