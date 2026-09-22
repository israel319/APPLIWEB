using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Domain.Enums;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface IFactureRepository : IRepository<Fact>
{
    Task<Fact?> GetWithDetailsAsync(int id);
    Task<Fact?> GetWithDetailsAndPaymentsAsync(int id);
    Task<List<Fact>> GetByDateRangeAsync(DateOnly from, DateOnly to);
    Task<List<Fact>> GetByStatusAsync(FactureStatus status);
    Task<List<Fact>> GetByUserAsync(string userLogin);
    Task<List<Fact>> GetByCustomerAsync(int customerId);
    Task<List<Fact>> GetByLocalisationIdsAsync(List<int> localisationIds);
    Task<List<Payment>> GetPaymentsByFactIdAsync(int factId);
    Task AddPaymentAsync(Payment payment);
}
