namespace AppPlusPlus.Domain.Entities.GRH.Payroll;

/// <summary>
/// Entité représentant l'historique des paiements
/// </summary>
public class PaymentHistory
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int PayrollRunId { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? PaymentMethod { get; set; } // Bank/Check/Cash
    public string? Reference { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee.Employee Employee { get; set; } = null!;
    public PayrollRun PayrollRun { get; set; } = null!;
}
