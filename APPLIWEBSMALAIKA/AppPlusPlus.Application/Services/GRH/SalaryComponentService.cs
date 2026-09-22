using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Payroll;

namespace AppPlusPlus.Application.Services.GRH;

public class SalaryComponentService : ISalaryComponentService
{
    private readonly ISalaryComponentRepository _repo;

    public SalaryComponentService(ISalaryComponentRepository repo)
    {
        _repo = repo;
    }

    public Task<IEnumerable<SalaryComponent>> GetAllActiveAsync() => _repo.GetAllActiveAsync();

    public Task<SalaryComponent> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<SalaryComponent> AddAsync(SalaryComponent component)
    {
        component.CreatedAt = DateTime.UtcNow;
        component.UpdatedAt = DateTime.UtcNow;
        await _repo.AddAsync(component);
        return component;
    }

    public async Task UpdateAsync(SalaryComponent component)
    {
        component.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(component);
    }

    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
}
