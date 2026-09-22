using AppPlusPlus.Domain.Entities.GRH.Absence;

namespace AppPlusPlus.Application.Services.GRH;

public interface IAbsenceTypeService
{
    Task<IEnumerable<AbsenceType>> GetAllActiveAsync();
    Task<AbsenceType> GetByIdAsync(int id);
}
