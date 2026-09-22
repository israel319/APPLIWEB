using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Organization;

namespace AppPlusPlus.Application.Services.GRH;

public class EmployeeClassificationService : IEmployeeClassificationService
{
    private readonly IEmployeeClassificationRepository _repo;

    public EmployeeClassificationService(IEmployeeClassificationRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<EmployeeClassification>> GetAllActiveAsync()
    {
        var all = await _repo.GetAllAsync();
        return all.Where(c => c.IsActive).OrderBy(c => c.ClassificationName);
    }

    public Task<EmployeeClassification> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
}
