using AppPlusPlus.Domain.Entities.GRH.Employee;

namespace AppPlusPlus.Application.Services.GRH;

/// <summary>
/// Service pour gérer les employés
/// </summary>
public interface IEmployeeService
{
    Task<Employee> CreateEmployeeAsync(string employeeCode, string firstName, string lastName, string email, int companyId, int departmentId, int serviceId, int jobPositionId);
    Task<Employee> GetEmployeeByIdAsync(int id);
    Task<Employee> GetEmployeeByCodeAsync(string employeeCode);
    Task<IEnumerable<Employee>> GetAllActiveEmployeesAsync();
    Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(int departmentId);
    Task UpdateEmployeeAsync(Employee employee);
    Task DeactivateEmployeeAsync(int employeeId);
    Task<int> GetTotalEmployeeCountAsync();
    Task<decimal> GetAverageAgeAsync();
    Task<IEnumerable<EmployeeContract>> GetEmployeeContractsAsync(int employeeId);
    Task<EmployeeContract> AddContractAsync(EmployeeContract contract);
}
