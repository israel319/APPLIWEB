using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Payroll;
using AppPlusPlus.Domain.Enums.GRH;

namespace AppPlusPlus.Application.Services.GRH;

public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IPayrollDetailRepository _detailRepository;

    public PayrollService(IPayrollRepository payrollRepository, IPayrollDetailRepository detailRepository)
    {
        _payrollRepository = payrollRepository;
        _detailRepository  = detailRepository;
    }

    public Task<IEnumerable<PayrollRun>> GetAllAsync()
        => _payrollRepository.GetAllAsync();

    public Task<IEnumerable<PayrollRun>> GetPayrollsByYearAndMonthAsync(int year, int month)
        => _payrollRepository.GetByYearAndMonthAsync(year, month);

    public Task<PayrollRun> GetPayrollRunAsync(int id)
        => _payrollRepository.GetByIdAsync(id);

    public async Task<PayrollRun> CreatePayrollRunAsync(DateTime payPeriodStart, DateTime payPeriodEnd)
    {
        var run = new PayrollRun
        {
            PayrollNumber  = $"PAY-{payPeriodStart:yyyy-MM}",
            PayPeriodStart = payPeriodStart,
            PayPeriodEnd   = payPeriodEnd,
            Status         = PayrollStatusEnum.Draft,
            CreatedAt      = DateTime.UtcNow
        };
        await _payrollRepository.AddAsync(run);
        return run;
    }

    public async Task ProcessPayrollAsync(int payrollRunId)
    {
        var run = await _payrollRepository.GetByIdAsync(payrollRunId);
        run.Status = PayrollStatusEnum.Prepared;
        await _payrollRepository.UpdateAsync(run);
    }

    public async Task ApprovePayrollAsync(int payrollRunId)
    {
        var run = await _payrollRepository.GetByIdAsync(payrollRunId);
        run.Status = PayrollStatusEnum.Approved;
        await _payrollRepository.UpdateAsync(run);
    }

    public async Task MarkAsPaidAsync(int payrollRunId)
    {
        var run = await _payrollRepository.GetByIdAsync(payrollRunId);
        run.Status  = PayrollStatusEnum.Paid;
        run.PaidDate = DateTime.UtcNow;
        await _payrollRepository.UpdateAsync(run);
    }

    public async Task DeletePayrollRunAsync(int id)
    {
        await _detailRepository.DeleteByRunAsync(id);
        await _payrollRepository.DeleteAsync(id);
    }

    public Task<IEnumerable<PayrollDetail>> GetPayrollDetailsAsync(int payrollRunId)
        => _detailRepository.GetByPayrollRunAsync(payrollRunId);

    public async Task<PayrollDetail> AddOrUpdatePayrollDetailAsync(PayrollDetail detail)
    {
        var existing = await _detailRepository.GetByEmployeeAndRunAsync(detail.PayrollRunId, detail.EmployeeId);
        if (existing == null)
        {
            detail.CreatedAt = DateTime.UtcNow;
            await _detailRepository.AddAsync(detail);
            return detail;
        }

        existing.BaseSalary       = detail.BaseSalary;
        existing.GrossSalary      = detail.GrossSalary;
        existing.NetSalary        = detail.NetSalary;
        existing.TaxAmount        = detail.TaxAmount;
        existing.InsuranceAmount  = detail.InsuranceAmount;
        existing.OtherDeductions  = detail.OtherDeductions;
        existing.Notes            = detail.Notes;
        await _detailRepository.UpdateAsync(existing);
        return existing;
    }

    public async Task<(string Result, string Message)> CalculatePayrollAsync(
        int payrollRunId, DateTime periodStart, DateTime periodEnd, int calculatedBy = 0)
    {
        var (result, message) = await _payrollRepository.ExecuteCalculatePayrollAsync(
            payrollRunId, periodStart, periodEnd, calculatedBy);

        if (result == "SUCCESS")
        {
            // Refresh the run from DB so the caller gets updated totals
            await _payrollRepository.GetByIdAsync(payrollRunId);
        }

        return (result, message);
    }
}
