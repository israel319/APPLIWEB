using AppPlusPlus.Domain.Enums.GRH;
using AppPlusPlus.Domain.Entities.GRH.Absence;
using AppPlusPlus.Domain.Entities.GRH.Documents;
using AppPlusPlus.Domain.Entities.GRH.Evaluation;
using AppPlusPlus.Domain.Entities.GRH.Leave;
using AppPlusPlus.Domain.Entities.GRH.Payroll;
using AppPlusPlus.Domain.Entities.GRH.Training;
using GrhAbsence = AppPlusPlus.Domain.Entities.GRH.Absence.Absence;

namespace AppPlusPlus.Domain.Entities.GRH.Employee;

/// <summary>
/// Entité représentant un employé
/// </summary>
public class Employee
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = null!;
    public string? PersonalEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalId { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string Gender { get; set; } = null!; // M/F
    public string? MaritalStatus { get; set; } // Single/Married/Divorced/Widowed
    public string? Nationality { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    
    // Professional Information
    public int CompanyId { get; set; }
    public int DepartmentId { get; set; }
    public int ServiceId { get; set; }
    public int JobPositionId { get; set; }
    public int EmployeeCategoryId { get; set; }
    public int EmployeeClassificationId { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public DateTime? EndDate { get; set; }
    public EmployeeStatusEnum Status { get; set; } = EmployeeStatusEnum.Active;
    public string? Manager_EmployeeCode { get; set; }
    
    // Contract Information
    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public ContractTypeEnum ContractType { get; set; } = ContractTypeEnum.CDI;
    
    // Bank Information
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? IBAN { get; set; }
    public string? BIC { get; set; }
    
    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? EmergencyContactPhone { get; set; }
    
    // System
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Organization.Company Company { get; set; } = null!;
    public Organization.Department Department { get; set; } = null!;
    public Organization.Service Service { get; set; } = null!;
    public Organization.JobPosition JobPosition { get; set; } = null!;
    public Organization.EmployeeCategory EmployeeCategory { get; set; } = null!;
    public Organization.EmployeeClassification EmployeeClassification { get; set; } = null!;
    
    public ICollection<EmployeeContract> Contracts { get; set; } = new List<EmployeeContract>();
    public ICollection<EmployeeEducation> Educations { get; set; } = new List<EmployeeEducation>();
    public ICollection<EmployeeSkill> Skills { get; set; } = new List<EmployeeSkill>();
    public ICollection<Salary> Salaries { get; set; } = new List<Salary>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveSaldo> LeaveSaldos { get; set; } = new List<LeaveSaldo>();
    public ICollection<GrhAbsence> Absences { get; set; } = new List<GrhAbsence>();
    public ICollection<PaymentHistory> PaymentHistories { get; set; } = new List<PaymentHistory>();
    public ICollection<PerformanceEvaluation> Evaluations { get; set; } = new List<PerformanceEvaluation>();
    public ICollection<EmployeeTraining> Trainings { get; set; } = new List<EmployeeTraining>();
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public ICollection<EmployeeAttestation> Attestations { get; set; } = new List<EmployeeAttestation>();
}
