namespace AppPlusPlus.Application.DTOs.GRH.Organization;

public class CreateDepartmentDto
{
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string? Description { get; set; }
    public int? ManagerEmployeeId { get; set; }
    public decimal? BudgetAmount { get; set; }
}

public class UpdateDepartmentDto
{
    public int Id { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ManagerEmployeeId { get; set; }
    public decimal? BudgetAmount { get; set; }
}

public class DepartmentReadDto
{
    public int Id { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public int EmployeeCount { get; set; }
    public decimal? BudgetAmount { get; set; }
}

public class CreateServiceDto
{
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string? Description { get; set; }
    public int? ManagerEmployeeId { get; set; }
}

public class UpdateServiceDto
{
    public int Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ManagerEmployeeId { get; set; }
}

public class ServiceReadDto
{
    public int Id { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public int EmployeeCount { get; set; }
}

public class CreateJobPositionDto
{
    public string PositionCode { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string? Description { get; set; }
    public string? EducationLevel { get; set; }
    public int? ExperienceYearsRequired { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public string? PositionLevel { get; set; }
}

public class UpdateJobPositionDto
{
    public int Id { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public string? PositionLevel { get; set; }
}

public class JobPositionReadDto
{
    public int Id { get; set; }
    public string PositionCode { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? EducationLevel { get; set; }
    public int? ExperienceYearsRequired { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public int EmployeeCount { get; set; }
}
