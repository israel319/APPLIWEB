using AppPlusPlus.Domain.Entities.GRH.Leave;
using AppPlusPlus.Domain.Enums.GRH;
using AppPlusPlus.Application.Repositories.GRH;

namespace AppPlusPlus.Application.Services.GRH;

public class LeaveService : ILeaveService
{
    private readonly ILeaveRepository _leaveRepository;

    public LeaveService(ILeaveRepository leaveRepository)
    {
        _leaveRepository = leaveRepository;
    }

    public async Task<LeaveRequest> CreateLeaveRequestAsync(int employeeId, int leaveTypeId, DateTime startDate, DateTime endDate, string reason)
    {
        var numberOfDays = (int)(endDate - startDate).TotalDays + 1;
        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            RequestNumber = $"LR-{DateTime.UtcNow:yyyyMMddHHmmss}",
            StartDate = startDate,
            EndDate = endDate,
            NumberOfDays = numberOfDays,
            Reason = reason,
            Status = LeaveStatusEnum.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _leaveRepository.AddAsync(leaveRequest);
        return leaveRequest;
    }

    public async Task<LeaveRequest> GetLeaveRequestAsync(int id)
    {
        return await _leaveRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync()
    {
        return await _leaveRepository.GetAllAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync()
    {
        return await _leaveRepository.GetPendingAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetEmployeeLeaveHistoryAsync(int employeeId)
    {
        return await _leaveRepository.GetByEmployeeAsync(employeeId);
    }

    public async Task ApproveLeaveRequestAsync(int leaveRequestId, int approverId)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(leaveRequestId);
        leaveRequest.Status = LeaveStatusEnum.Approved;
        leaveRequest.ApprovedById = approverId;
        leaveRequest.ApprovedDate = DateTime.UtcNow;
        await _leaveRepository.UpdateAsync(leaveRequest);

        // Recalculate leave balance via SP
        await _leaveRepository.ExecuteUpdateLeaveSaldoAsync(
            leaveRequest.EmployeeId, leaveRequest.StartDate.Year);
    }

    public async Task RejectLeaveRequestAsync(int leaveRequestId, string reason)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(leaveRequestId);
        leaveRequest.Status = LeaveStatusEnum.Rejected;
        leaveRequest.RejectionReason = reason;
        await _leaveRepository.UpdateAsync(leaveRequest);
    }

    public async Task<LeaveSaldo> GetLeaveSaldoAsync(int employeeId, int leaveTypeId, int year)
    {
        throw new NotImplementedException("Implement with ILeaveSaldoRepository");
    }

    public async Task UpdateLeaveSaldoAsync(int employeeId, int leaveTypeId, int year, decimal usedDays)
    {
        throw new NotImplementedException("Implement with ILeaveSaldoRepository");
    }

    public async Task<decimal> GetRemainingLeaveDaysAsync(int employeeId, int leaveTypeId)
    {
        throw new NotImplementedException("Implement with ILeaveSaldoRepository");
    }

    public async Task RefreshLeaveSaldoAsync(int employeeId, int year)
        => await _leaveRepository.ExecuteUpdateLeaveSaldoAsync(employeeId, year);

    public async Task<(string Result, string Message)> ResetAnnualLeavesAsync(int year)
        => await _leaveRepository.ExecuteAnnualLeaveResetAsync(year);
}
