using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Leave;

namespace AppPlusPlus.Application.Services.GRH;

public class LeaveTypeService : ILeaveTypeService
{
    private readonly ILeaveTypeRepository _repo;

    public LeaveTypeService(ILeaveTypeRepository repo)
    {
        _repo = repo;
    }

    public Task<IEnumerable<LeaveType>> GetAllActiveAsync() => _repo.GetAllActiveAsync();

    public Task<LeaveType> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
}
