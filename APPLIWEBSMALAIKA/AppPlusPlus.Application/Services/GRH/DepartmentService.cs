using AppPlusPlus.Domain.Entities.GRH.Organization;
using AppPlusPlus.Application.Repositories.GRH;

namespace AppPlusPlus.Application.Services.GRH;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<Department> CreateDepartmentAsync(string departmentCode, string departmentName, int companyId)
    {
        var department = new Department
        {
            DepartmentCode = departmentCode,
            DepartmentName = departmentName,
            CompanyId = companyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _departmentRepository.AddAsync(department);
        return department;
    }

    public async Task<Department> GetDepartmentAsync(int id)
    {
        return await _departmentRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Department>> GetCompanyDepartmentsAsync(int companyId)
    {
        return await _departmentRepository.GetByCompanyAsync(companyId);
    }

    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        return await _departmentRepository.GetAllAsync();
    }

    public async Task UpdateDepartmentAsync(Department department)
    {
        department.UpdatedAt = DateTime.UtcNow;
        await _departmentRepository.UpdateAsync(department);
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        await _departmentRepository.DeleteAsync(id);
    }

    public async Task<int> GetEmployeeCountAsync(int departmentId)
    {
        throw new NotImplementedException("Implement with IEmployeeRepository");
    }
}
