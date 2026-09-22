using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Domain.Entities.Finance;

namespace AppPlusPlus.Application.Services.Approvisionnement;

public interface IApproService
{
    Task<List<Appro>> GetApprosByLocalisationsAsync(List<int> localisationIds);
    Task DeleteApproAsync(int approId);

    Task<ApproExpense?> GetApproExpenseByIdAsync(int id);
    Task AddApproExpenseAsync(ApproExpense expense);
    Task UpdateApproExpenseAsync(ApproExpense expense);
}
