using AppPlusPlus.Domain.Entities.Fournisseurs;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<List<Supplier>> SearchByNameAsync(string name);
    Task<List<Supplier>> GetByServiceAsync(int serviceId);
    Task<List<SupplierService>> GetAllServicesAsync();
}
