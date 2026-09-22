using AppPlusPlus.Domain.Entities.GRH.Organization;

namespace AppPlusPlus.Application.Services.GRH;

/// <summary>
/// Service pour gérer les départements
/// </summary>
public interface IDepartmentService
{
    Task<Department> CreateDepartmentAsync(string departmentCode, string departmentName, int companyId);
    Task<Department> GetDepartmentAsync(int id);
    Task<IEnumerable<Department>> GetCompanyDepartmentsAsync(int companyId);
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();
    Task UpdateDepartmentAsync(Department department);
    Task DeleteDepartmentAsync(int id);
    Task<int> GetEmployeeCountAsync(int departmentId);
}
