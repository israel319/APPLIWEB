using AppPlusPlus.Domain.Entities.GRH.Organization;

namespace AppPlusPlus.Application.Services.GRH;

/// <summary>
/// Service pour gérer les services
/// </summary>
public interface IServiceService
{
    Task<Service> CreateServiceAsync(string serviceCode, string serviceName, int departmentId, int companyId);
    Task<Service> GetServiceAsync(int id);
    Task<IEnumerable<Service>> GetDepartmentServicesAsync(int departmentId);
    Task<IEnumerable<Service>> GetAllServicesAsync();
    Task UpdateServiceAsync(Service service);
    Task DeleteServiceAsync(int id);
}
