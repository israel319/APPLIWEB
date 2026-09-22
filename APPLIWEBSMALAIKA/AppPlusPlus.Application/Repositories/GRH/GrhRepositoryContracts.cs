using AppPlusPlus.Domain.Entities.GRH.Absence;
using AppPlusPlus.Domain.Entities.GRH.Documents;
using AppPlusPlus.Domain.Entities.GRH.Evaluation;
using AppPlusPlus.Domain.Entities.GRH.Leave;
using AppPlusPlus.Domain.Entities.GRH.Payroll;
using AppPlusPlus.Domain.Entities.GRH.Recruitment;
using AppPlusPlus.Domain.Entities.GRH.Training;
using AppPlusPlus.Domain.Entities.GRH.Organization;
using AppPlusPlus.Domain.Entities.GRH.Employee;
using GrhEmployee = AppPlusPlus.Domain.Entities.GRH.Employee.Employee;

namespace AppPlusPlus.Application.Repositories.GRH;

public interface ICompanyRepository
{
    Task<Company> GetByIdAsync(int id);
    Task<Company?> GetByCodeAsync(string companyCode);
    Task<IEnumerable<Company>> GetAllAsync();
    Task AddAsync(Company company);
    Task UpdateAsync(Company company);
    Task DeleteAsync(int id);
}

public interface IEmployeeRepository
{
    Task<GrhEmployee> GetByIdAsync(int id);
    Task<GrhEmployee?> GetByCodeAsync(string employeeCode);
    Task<IEnumerable<GrhEmployee>> GetAllActiveAsync();
    Task<IEnumerable<GrhEmployee>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<GrhEmployee>> GetByServiceAsync(int serviceId);
    Task<IEnumerable<GrhEmployee>> GetByPositionAsync(int jobPositionId);
    Task AddAsync(GrhEmployee employee);
    Task UpdateAsync(GrhEmployee employee);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<GrhEmployee>> GetPagedAsync(int pageNumber, int pageSize);
}

public interface ILeaveRepository
{
    Task<LeaveRequest> GetByIdAsync(int id);
    Task<LeaveRequest?> GetByNumberAsync(string requestNumber);
    Task<IEnumerable<LeaveRequest>> GetAllAsync();
    Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(int employeeId);
    Task<IEnumerable<LeaveRequest>> GetPendingAsync();
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status);
    Task AddAsync(LeaveRequest leaveRequest);
    Task UpdateAsync(LeaveRequest leaveRequest);
    Task DeleteAsync(int id);
    /// <summary>Executes sp_UpdateLeaveSaldo for an employee.</summary>
    Task ExecuteUpdateLeaveSaldoAsync(int employeeId, int year);
    /// <summary>Executes sp_GenerateAnnualLeaveReset for the given year.</summary>
    Task<(string Result, string Message)> ExecuteAnnualLeaveResetAsync(int year);
}

public interface IPayrollRepository
{
    Task<PayrollRun> GetByIdAsync(int id);
    Task<PayrollRun?> GetByNumberAsync(string payrollNumber);
    Task<IEnumerable<PayrollRun>> GetByYearAndMonthAsync(int year, int month);
    Task<IEnumerable<PayrollRun>> GetByStatusAsync(string status);
    Task<IEnumerable<PayrollRun>> GetAllAsync();
    Task AddAsync(PayrollRun payrollRun);
    Task UpdateAsync(PayrollRun payrollRun);
    Task DeleteAsync(int id);
    /// <summary>Executes sp_CalculatePayroll and returns (result, message).</summary>
    Task<(string Result, string Message)> ExecuteCalculatePayrollAsync(int payrollRunId, DateTime periodStart, DateTime periodEnd, int calculatedBy);
}

public interface IAbsenceRepository
{
    Task<Absence> GetByIdAsync(int id);
    Task<Absence?> GetByNumberAsync(string absenceNumber);
    Task<IEnumerable<Absence>> GetAllAsync();
    Task<IEnumerable<Absence>> GetByEmployeeAsync(int employeeId);
    Task<IEnumerable<Absence>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Absence>> GetByStatusAsync(string status);
    Task AddAsync(Absence absence);
    Task UpdateAsync(Absence absence);
    Task DeleteAsync(int id);
}

public interface IEmployeeContractRepository
{
    Task<IEnumerable<EmployeeContract>> GetByEmployeeAsync(int employeeId);
    Task<EmployeeContract> GetByIdAsync(int id);
    Task AddAsync(EmployeeContract contract);
    Task UpdateAsync(EmployeeContract contract);
    Task DeleteAsync(int id);
}

public interface IDepartmentRepository
{
    Task<Department> GetByIdAsync(int id);
    Task<Department?> GetByCodeAsync(string departmentCode);
    Task<IEnumerable<Department>> GetByCompanyAsync(int companyId);
    Task<IEnumerable<Department>> GetAllAsync();
    Task AddAsync(Department department);
    Task UpdateAsync(Department department);
    Task DeleteAsync(int id);
}

public interface IServiceRepository
{
    Task<Service> GetByIdAsync(int id);
    Task<Service?> GetByCodeAsync(string serviceCode);
    Task<IEnumerable<Service>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Service>> GetAllAsync();
    Task AddAsync(Service service);
    Task UpdateAsync(Service service);
    Task DeleteAsync(int id);
}

public interface IJobPositionRepository
{
    Task<JobPosition> GetByIdAsync(int id);
    Task<JobPosition?> GetByCodeAsync(string positionCode);
    Task<IEnumerable<JobPosition>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<JobPosition>> GetAllAsync();
    Task AddAsync(JobPosition jobPosition);
    Task UpdateAsync(JobPosition jobPosition);
    Task DeleteAsync(int id);
}

public interface IEmployeeCategoryRepository
{
    Task<EmployeeCategory> GetByIdAsync(int id);
    Task<EmployeeCategory?> GetByCodeAsync(string categoryCode);
    Task<IEnumerable<EmployeeCategory>> GetAllAsync();
    Task AddAsync(EmployeeCategory category);
    Task UpdateAsync(EmployeeCategory category);
    Task DeleteAsync(int id);
}

public interface IEmployeeClassificationRepository
{
    Task<EmployeeClassification> GetByIdAsync(int id);
    Task<EmployeeClassification?> GetByCodeAsync(string classificationCode);
    Task<IEnumerable<EmployeeClassification>> GetAllAsync();
    Task AddAsync(EmployeeClassification classification);
    Task UpdateAsync(EmployeeClassification classification);
    Task DeleteAsync(int id);
}

public interface IRecruitmentRepository
{
    Task<JobOpening> GetJobOpeningByIdAsync(int id);
    Task<Candidate> GetCandidateByIdAsync(int id);
    Task<Interview> GetInterviewByIdAsync(int id);
    Task<IEnumerable<JobOpening>> GetOpenJobsAsync();
    Task<IEnumerable<Candidate>> GetCandidatesByOpeningAsync(int openingId);
    Task<IEnumerable<Interview>> GetInterviewsByCandidateAsync(int candidateId);
    Task AddJobOpeningAsync(JobOpening jobOpening);
    Task AddCandidateAsync(Candidate candidate);
    Task AddInterviewAsync(Interview interview);
    Task UpdateCandidateAsync(Candidate candidate);
    Task DeleteCandidateAsync(int id);
}

public interface IEvaluationRepository
{
    Task<PerformanceEvaluation> GetByIdAsync(int id);
    Task<PerformanceEvaluation?> GetByCodeAsync(string evaluationCode);
    Task<IEnumerable<PerformanceEvaluation>> GetByEmployeeAsync(int employeeId);
    Task<IEnumerable<PerformanceEvaluation>> GetByYearAsync(int year);
    Task<IEnumerable<PerformanceEvaluation>> GetByStatusAsync(string status);
    Task AddAsync(PerformanceEvaluation evaluation);
    Task UpdateAsync(PerformanceEvaluation evaluation);
    Task DeleteAsync(int id);
}

public interface ITrainingRepository
{
    Task<TrainingProgram> GetProgramByIdAsync(int id);
    Task<TrainingSession> GetSessionByIdAsync(int id);
    Task<EmployeeTraining> GetEmployeeTrainingByIdAsync(int id);
    Task<IEnumerable<TrainingProgram>> GetAllProgramsAsync();
    Task<IEnumerable<TrainingSession>> GetSessionsByProgramAsync(int programId);
    Task<IEnumerable<EmployeeTraining>> GetByEmployeeAsync(int employeeId);
    Task<IEnumerable<EmployeeTraining>> GetBySessionAsync(int sessionId);
    Task AddProgramAsync(TrainingProgram program);
    Task AddSessionAsync(TrainingSession session);
    Task AddEmployeeTrainingAsync(EmployeeTraining employeeTraining);
    Task UpdateProgramAsync(TrainingProgram program);
    Task UpdateSessionAsync(TrainingSession session);
    Task DeleteProgramAsync(int id);
}

public interface IDocumentRepository
{
    Task<EmployeeDocument> GetByIdAsync(int id);
    Task<EmployeeDocument?> GetByNumberAsync(string documentNumber);
    Task<IEnumerable<EmployeeDocument>> GetByEmployeeAsync(int employeeId);
    Task<IEnumerable<EmployeeDocument>> GetExpiringDocumentsAsync(int daysUntilExpiry);
    Task<IEnumerable<EmployeeAttestation>> GetAttestationsByEmployeeAsync(int employeeId);
    Task AddDocumentAsync(EmployeeDocument document);
    Task AddAttestationAsync(EmployeeAttestation attestation);
    Task UpdateDocumentAsync(EmployeeDocument document);
    Task DeleteDocumentAsync(int id);
}

public interface ISalaryComponentRepository
{
    Task<SalaryComponent> GetByIdAsync(int id);
    Task<IEnumerable<SalaryComponent>> GetAllActiveAsync();
    Task AddAsync(SalaryComponent component);
    Task UpdateAsync(SalaryComponent component);
    Task DeleteAsync(int id);
}

public interface IPayrollDetailRepository
{
    Task<IEnumerable<PayrollDetail>> GetByPayrollRunAsync(int payrollRunId);
    Task<PayrollDetail?> GetByEmployeeAndRunAsync(int payrollRunId, int employeeId);
    Task AddAsync(PayrollDetail detail);
    Task UpdateAsync(PayrollDetail detail);
    Task DeleteByRunAsync(int payrollRunId);
}

public interface ILeaveTypeRepository
{
    Task<LeaveType> GetByIdAsync(int id);
    Task<IEnumerable<LeaveType>> GetAllActiveAsync();
    Task AddAsync(LeaveType leaveType);
    Task UpdateAsync(LeaveType leaveType);
}

public interface IAbsenceTypeRepository
{
    Task<AbsenceType> GetByIdAsync(int id);
    Task<IEnumerable<AbsenceType>> GetAllActiveAsync();
    Task AddAsync(AbsenceType absenceType);
    Task UpdateAsync(AbsenceType absenceType);
}
