using AppPlusPlus.Domain.Entities.GRH.Leave;

namespace AppPlusPlus.Application.Services.GRH;

public interface ILeaveTypeService
{
    Task<IEnumerable<LeaveType>> GetAllActiveAsync();
    Task<LeaveType> GetByIdAsync(int id);
}
