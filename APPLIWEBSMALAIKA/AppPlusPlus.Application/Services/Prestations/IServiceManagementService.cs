using AppPlusPlus.Application.Common;
using AppPlusPlus.Application.DTOs.Prestations;
using AppPlusPlus.Domain.Entities.Prestations;

namespace AppPlusPlus.Application.Services.Prestations;

public interface IServiceManagementService
{
    Task<List<ServiceProjectRowDto>> GetProjectsAsync(string? search = null);
    Task<ServiceProjectDetailDto?> GetProjectAsync(int id);
    Task<ServiceResult<int>> SaveProjectAsync(ServiceProjectDetailDto dto, string login);

    Task<List<ServiceTimesheetRowDto>> GetTimesheetsAsync(int? projectId = null, string? search = null);
    Task<ServiceResult<int>> SaveTimesheetAsync(ServiceTimesheet timesheet, string login);
    Task<ServiceResult> ApproveTimesheetAsync(int timesheetId, string login);
    Task<ServiceResult> CancelTimesheetAsync(int timesheetId, string login);

    Task<List<ServiceTaskRowDto>> GetTasksAsync(int? projectId = null, string? search = null);
    Task<ServiceResult<int>> SaveTaskAsync(ServiceTask task, string login);
    Task<ServiceResult> DeleteTaskAsync(int taskId);

    Task<List<ServiceDeliverableRowDto>> GetDeliverablesAsync(int? projectId = null, string? search = null);
    Task<ServiceResult<int>> SaveDeliverableAsync(ServiceDeliverable deliverable, string login);
    Task<ServiceResult> DeleteDeliverableAsync(int deliverableId);

    Task<ServiceResult<int>> GenerateInvoiceFromProjectAsync(int projectId, string login);

    Task<List<ServiceInvoiceRowDto>> GetServiceInvoicesAsync(int? projectId = null, string? search = null);
    Task<ServiceInvoiceDetailDto?> GetServiceInvoiceAsync(int factId);
    Task<ServiceResult> ValidateServiceInvoiceAsync(int factId, string login);
    Task<ServiceResult> DeleteServiceInvoiceDraftAsync(int factId, string login);
}
