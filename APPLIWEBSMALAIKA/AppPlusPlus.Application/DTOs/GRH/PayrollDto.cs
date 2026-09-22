namespace AppPlusPlus.Application.DTOs.GRH.Payroll;

public class CreatePayrollRunDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime PayPeriodStartDate { get; set; }
    public DateTime PayPeriodEndDate { get; set; }
    public string? Notes { get; set; }
}

public class PayrollRunReadDto
{
    public int Id { get; set; }
    public string PayrollNumber { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime PayPeriodStartDate { get; set; }
    public DateTime PayPeriodEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetSalary { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AddPayrollDetailDto
{
    public int PayrollRunId { get; set; }
    public int EmployeeId { get; set; }
    public int SalaryComponentId { get; set; }
    public decimal Amount { get; set; }
    public decimal? Percentage { get; set; }
    public int? ProRataDays { get; set; }
    public string? Description { get; set; }
}

public class PaymentHistoryReadDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? BankReference { get; set; }
}
