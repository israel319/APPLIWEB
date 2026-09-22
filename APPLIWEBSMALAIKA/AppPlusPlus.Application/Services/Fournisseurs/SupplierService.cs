using AppPlusPlus.Domain.Entities.Fournisseurs;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Application.Services.Fournisseurs;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepo;

    public SupplierService(ISupplierRepository supplierRepo)
    {
        _supplierRepo = supplierRepo;
    }

    public async Task<List<Supplier>> GetAllAsync()
        => await _supplierRepo.GetAllAsync();

    public async Task<Supplier?> GetByIdAsync(int id)
        => await _supplierRepo.GetByIdAsync(id);

    public async Task AddAsync(Supplier supplier)
        => await _supplierRepo.AddAsync(supplier);

    public async Task UpdateAsync(Supplier supplier)
        => await _supplierRepo.UpdateAsync(supplier);

    public async Task DeleteAsync(int id)
    {
        var supplier = await _supplierRepo.GetByIdAsync(id);
        if (supplier != null)
            await _supplierRepo.DeleteAsync(supplier);
    }
}
