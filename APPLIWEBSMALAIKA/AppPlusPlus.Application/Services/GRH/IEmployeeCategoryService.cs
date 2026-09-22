using AppPlusPlus.Domain.Entities.GRH.Organization;

namespace AppPlusPlus.Application.Services.GRH;

public interface IEmployeeCategoryService
{
    Task<IEnumerable<EmployeeCategory>> GetAllActiveAsync();
    Task<EmployeeCategory> GetByIdAsync(int id);
}
