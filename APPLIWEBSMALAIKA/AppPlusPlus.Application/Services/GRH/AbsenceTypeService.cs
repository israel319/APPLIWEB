using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Absence;

namespace AppPlusPlus.Application.Services.GRH;

public class AbsenceTypeService : IAbsenceTypeService
{
    private readonly IAbsenceTypeRepository _repo;

    public AbsenceTypeService(IAbsenceTypeRepository repo)
    {
        _repo = repo;
    }

    public Task<IEnumerable<AbsenceType>> GetAllActiveAsync() => _repo.GetAllActiveAsync();

    public Task<AbsenceType> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
}
