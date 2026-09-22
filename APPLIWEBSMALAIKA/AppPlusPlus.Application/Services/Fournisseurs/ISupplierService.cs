using AppPlusPlus.Domain.Entities.Fournisseurs;

namespace AppPlusPlus.Application.Services.Fournisseurs;

public interface ISupplierService
{
    Task<List<Supplier>> GetAllAsync();
    Task<Supplier?> GetByIdAsync(int id);
    Task AddAsync(Supplier supplier);
    Task UpdateAsync(Supplier supplier);
    Task DeleteAsync(int id);
}
