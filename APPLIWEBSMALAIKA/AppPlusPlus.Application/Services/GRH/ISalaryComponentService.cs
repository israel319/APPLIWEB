using AppPlusPlus.Domain.Entities.GRH.Payroll;

namespace AppPlusPlus.Application.Services.GRH;

public interface ISalaryComponentService
{
    Task<IEnumerable<SalaryComponent>> GetAllActiveAsync();
    Task<SalaryComponent> GetByIdAsync(int id);
    Task<SalaryComponent> AddAsync(SalaryComponent component);
    Task UpdateAsync(SalaryComponent component);
    Task DeleteAsync(int id);
}
