using AppPlusPlus.Domain.Entities.GRH.Payroll;

namespace AppPlusPlus.Application.Services.GRH;

public interface IPayrollService
{
    Task<IEnumerable<PayrollRun>> GetAllAsync();
    Task<IEnumerable<PayrollRun>> GetPayrollsByYearAndMonthAsync(int year, int month);
    Task<PayrollRun> GetPayrollRunAsync(int id);
    Task<PayrollRun> CreatePayrollRunAsync(DateTime payPeriodStart, DateTime payPeriodEnd);
    Task ProcessPayrollAsync(int payrollRunId);
    Task ApprovePayrollAsync(int payrollRunId);
    Task MarkAsPaidAsync(int payrollRunId);
    Task DeletePayrollRunAsync(int id);
    Task<IEnumerable<PayrollDetail>> GetPayrollDetailsAsync(int payrollRunId);
    Task<PayrollDetail> AddOrUpdatePayrollDetailAsync(PayrollDetail detail);
    /// <summary>Calls sp_CalculatePayroll to auto-compute all employee payslips for this run.</summary>
    Task<(string Result, string Message)> CalculatePayrollAsync(int payrollRunId, DateTime periodStart, DateTime periodEnd, int calculatedBy = 0);
}
