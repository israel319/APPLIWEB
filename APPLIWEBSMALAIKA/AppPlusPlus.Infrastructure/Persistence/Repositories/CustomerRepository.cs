using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Common;
using AppPlusPlus.Domain.Entities.Clients;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
{
    public CustomerRepository(IDbContextFactory<AppDbContext> dbFactory) : base(dbFactory) { }

    public async Task<List<Customer>> GetPermanentCustomersAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Customers.Where(c => c.IsPermanent == true).ToListAsync();
    }

    public async Task<List<Customer>> SearchByNameAsync(string name)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Customers
            .Where(c => c.CustomerName != null && c.CustomerName.Contains(name))
            .ToListAsync();
    }

    public async Task<List<Customer>> GetByTypeAsync(int customerTypeId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Customers.Where(c => c.CustomerTypeId == customerTypeId).ToListAsync();
    }

    public async Task<List<CustomerType>> GetAllTypesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.CustomerTypes.ToListAsync();
    }

    public async Task<Customer?> GetPermanentByContactAsync(string contact)
    {
        var normalized = ClientPhoneHelper.Normalize(contact);
        if (string.IsNullOrEmpty(normalized))
            return null;

        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var candidates = await ctx.Customers
            .Where(c => c.IsPermanent == true && c.Contact != null)
            .ToListAsync();

        return candidates.FirstOrDefault(c => ClientPhoneHelper.Normalize(c.Contact) == normalized);
    }
}
