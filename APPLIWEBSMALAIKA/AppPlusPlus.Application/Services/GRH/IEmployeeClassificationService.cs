using AppPlusPlus.Domain.Entities.GRH.Organization;

namespace AppPlusPlus.Application.Services.GRH;

public interface IEmployeeClassificationService
{
    Task<IEnumerable<EmployeeClassification>> GetAllActiveAsync();
    Task<EmployeeClassification> GetByIdAsync(int id);
}
