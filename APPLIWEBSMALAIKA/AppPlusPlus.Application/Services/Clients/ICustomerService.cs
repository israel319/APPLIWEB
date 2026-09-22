using AppPlusPlus.Domain.Entities.Clients;

namespace AppPlusPlus.Application.Services.Clients;

public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByContactAsync(string contact);
    Task<Customer> RegisterOnlineAsync(string name, string contact, string? adress);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(int id);
}
