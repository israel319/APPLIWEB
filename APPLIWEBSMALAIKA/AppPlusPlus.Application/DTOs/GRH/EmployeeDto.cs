namespace AppPlusPlus.Application.DTOs.GRH.Employee;

public class CreateEmployeeDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public int CompanyId { get; set; }
    public int DepartmentId { get; set; }
    public int ServiceId { get; set; }
    public int JobPositionId { get; set; }
    public int EmployeeCategoryId { get; set; }
    public int EmployeeClassificationId { get; set; }
    public string? ContractType { get; set; }
    public DateTime HireDate { get; set; }
    public string? ManagerEmployeeCode { get; set; }
    public string? BankAccountIban { get; set; }
    public string? BankAccountBic { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}

public class UpdateEmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public int DepartmentId { get; set; }
    public int ServiceId { get; set; }
    public int JobPositionId { get; set; }
    public string? ManagerEmployeeCode { get; set; }
    public string? BankAccountIban { get; set; }
    public string? BankAccountBic { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}

public class EmployeeReadDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year;
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string JobPositionName { get; set; } = string.Empty;
    public string EmployeeStatus { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EmployeeListDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string JobPositionName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
