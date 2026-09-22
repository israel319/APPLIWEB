using AppPlusPlus.Domain.Entities.GRH.Leave;

namespace AppPlusPlus.Application.Services.GRH;

/// <summary>
/// Service pour gérer les congés
/// </summary>
public interface ILeaveService
{
    Task<LeaveRequest> CreateLeaveRequestAsync(int employeeId, int leaveTypeId, DateTime startDate, DateTime endDate, string reason);
    Task<LeaveRequest> GetLeaveRequestAsync(int id);
    Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync();
    Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync();
    Task<IEnumerable<LeaveRequest>> GetEmployeeLeaveHistoryAsync(int employeeId);
    Task ApproveLeaveRequestAsync(int leaveRequestId, int approverId);
    Task RejectLeaveRequestAsync(int leaveRequestId, string reason);
    Task<LeaveSaldo> GetLeaveSaldoAsync(int employeeId, int leaveTypeId, int year);
    Task UpdateLeaveSaldoAsync(int employeeId, int leaveTypeId, int year, decimal usedDays);
    Task<decimal> GetRemainingLeaveDaysAsync(int employeeId, int leaveTypeId);
    /// <summary>Calls sp_UpdateLeaveSaldo — recalculates leave balances for an employee/year.</summary>
    Task RefreshLeaveSaldoAsync(int employeeId, int year);
    /// <summary>Calls sp_GenerateAnnualLeaveReset — carries over balances to the new year.</summary>
    Task<(string Result, string Message)> ResetAnnualLeavesAsync(int year);
}
