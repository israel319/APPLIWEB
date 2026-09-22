using AppPlusPlus.Domain.Entities.GRH.Employee;
using AppPlusPlus.Application.Repositories.GRH;

namespace AppPlusPlus.Application.Services.GRH;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeContractRepository _contractRepository;

    public EmployeeService(IEmployeeRepository employeeRepository,
                           IEmployeeContractRepository contractRepository)
    {
        _employeeRepository = employeeRepository;
        _contractRepository = contractRepository;
    }

    public async Task<Employee> CreateEmployeeAsync(string employeeCode, string firstName, string lastName, string email, 
        int companyId, int departmentId, int serviceId, int jobPositionId)
    {
        var employee = new Employee
        {
            EmployeeCode = employeeCode,
            FirstName    = firstName,
            LastName     = lastName,
            Email        = email,
            CompanyId    = companyId,
            DepartmentId = departmentId,
            ServiceId    = serviceId,
            JobPositionId = jobPositionId,
            HireDate     = DateTime.UtcNow,
            Status       = AppPlusPlus.Domain.Enums.GRH.EmployeeStatusEnum.Active,
            CreatedAt    = DateTime.UtcNow
        };

        await _employeeRepository.AddAsync(employee);
        return employee;
    }

    public async Task<Employee> GetEmployeeByIdAsync(int id)
    {
        return await _employeeRepository.GetByIdAsync(id);
    }

    public async Task<Employee> GetEmployeeByCodeAsync(string employeeCode)
    {
        return await _employeeRepository.GetByCodeAsync(employeeCode) 
            ?? throw new InvalidOperationException($"Employee {employeeCode} not found");
    }

    public async Task<IEnumerable<Employee>> GetAllActiveEmployeesAsync()
    {
        return await _employeeRepository.GetAllActiveAsync();
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(int departmentId)
    {
        return await _employeeRepository.GetByDepartmentAsync(departmentId);
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        employee.UpdatedAt = DateTime.UtcNow;
        await _employeeRepository.UpdateAsync(employee);
    }

    public async Task DeactivateEmployeeAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        employee.Status  = AppPlusPlus.Domain.Enums.GRH.EmployeeStatusEnum.Inactive;
        employee.UpdatedAt = DateTime.UtcNow;
        await _employeeRepository.UpdateAsync(employee);
    }

    public async Task<int> GetTotalEmployeeCountAsync()
    {
        return await _employeeRepository.GetTotalCountAsync();
    }

    public async Task<decimal> GetAverageAgeAsync()
    {
        var employees = await _employeeRepository.GetAllActiveAsync();
        var today = DateTime.UtcNow;
        var averageAge = employees.Average(e => 
            (today.Year - e.DateOfBirth.Year) - 
            (e.DateOfBirth > today.AddYears(-(today.Year - e.DateOfBirth.Year)) ? 1 : 0)
        );
        return (decimal)averageAge;
    }

    public async Task<IEnumerable<EmployeeContract>> GetEmployeeContractsAsync(int employeeId)
    {
        return await _contractRepository.GetByEmployeeAsync(employeeId);
    }

    public async Task<EmployeeContract> AddContractAsync(EmployeeContract contract)
    {
        contract.CreatedAt = DateTime.UtcNow;
        await _contractRepository.AddAsync(contract);
        return contract;
    }
}
