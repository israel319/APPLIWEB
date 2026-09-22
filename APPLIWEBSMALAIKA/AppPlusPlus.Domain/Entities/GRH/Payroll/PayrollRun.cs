using AppPlusPlus.Domain.Enums.GRH;

namespace AppPlusPlus.Domain.Entities.GRH.Payroll;

/// <summary>
/// Entité représentant une paie
/// </summary>
public class PayrollRun
{
    public int Id { get; set; }
    public string PayrollNumber { get; set; } = null!;
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public PayrollStatusEnum Status { get; set; } = PayrollStatusEnum.Draft;
    public DateTime? ProcessedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public int NumberOfEmployees { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetSalary { get; set; }
    public string? PaymentMethod { get; set; } // Bank/Check/Cash
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();
}
