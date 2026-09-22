using AppPlusPlus.Application.Common;
using AppPlusPlus.Domain.Entities.Clients;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Application.Services.Clients;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepo;

    public CustomerService(ICustomerRepository customerRepo)
    {
        _customerRepo = customerRepo;
    }

    public async Task<List<Customer>> GetAllAsync()
        => await _customerRepo.GetAllAsync();

    public async Task<Customer?> GetByIdAsync(int id)
        => await _customerRepo.GetByIdAsync(id);

    public async Task<Customer?> GetByContactAsync(string contact)
        => await _customerRepo.GetPermanentByContactAsync(contact);

    public async Task<Customer> RegisterOnlineAsync(string name, string contact, string? adress)
    {
        var normalizedContact = ClientPhoneHelper.Normalize(contact);
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Le nom est obligatoire.");
        if (string.IsNullOrEmpty(normalizedContact))
            throw new InvalidOperationException("Le numéro de téléphone est invalide.");

        var existing = await _customerRepo.GetPermanentByContactAsync(normalizedContact);
        if (existing != null)
            throw new InvalidOperationException("Ce numéro est déjà inscrit. Connectez-vous.");

        var customer = new Customer
        {
            CustomerName = name.Trim(),
            Contact = normalizedContact,
            Adress = string.IsNullOrWhiteSpace(adress) ? null : adress.Trim(),
            IsPermanent = true,
            UserLogin = "WEB",
            CreationDate = DateOnly.FromDateTime(DateTime.Today)
        };

        await _customerRepo.AddAsync(customer);
        return customer;
    }

    public async Task AddAsync(Customer customer)
        => await _customerRepo.AddAsync(customer);

    public async Task UpdateAsync(Customer customer)
        => await _customerRepo.UpdateAsync(customer);

    public async Task DeleteAsync(int id)
    {
        var customer = await _customerRepo.GetByIdAsync(id);
        if (customer != null)
            await _customerRepo.DeleteAsync(customer);
    }
}
