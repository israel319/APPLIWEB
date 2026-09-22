using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Organization;

namespace AppPlusPlus.Application.Services.GRH;

public class EmployeeCategoryService : IEmployeeCategoryService
{
    private readonly IEmployeeCategoryRepository _repo;

    public EmployeeCategoryService(IEmployeeCategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<EmployeeCategory>> GetAllActiveAsync()
    {
        var all = await _repo.GetAllAsync();
        return all.Where(c => c.IsActive).OrderBy(c => c.CategoryName);
    }

    public Task<EmployeeCategory> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
}
