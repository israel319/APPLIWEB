using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Entities.Fournisseurs;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class SupplierRepository : RepositoryBase<Supplier>, ISupplierRepository
{
    public SupplierRepository(IDbContextFactory<AppDbContext> dbFactory) : base(dbFactory) { }

    public async Task<List<Supplier>> SearchByNameAsync(string name)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Suppliers
            .Where(s => s.SupplierName != null && s.SupplierName.Contains(name))
            .ToListAsync();
    }

    public async Task<List<Supplier>> GetByServiceAsync(int serviceId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Suppliers.Where(s => s.ServiceId == serviceId).ToListAsync();
    }

    public async Task<List<SupplierService>> GetAllServicesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.SupplierServices.ToListAsync();
    }
}
