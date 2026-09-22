using AppPlusPlus.Application.Common;
using AppPlusPlus.Domain.Entities.Vente;

namespace AppPlusPlus.Application.Services.Vente;

public interface IPaymentService
{
    Task<List<Payment>> GetPaymentsByFactureAsync(int factId);

    /// <summary>
    /// Records a payment within a transaction:
    /// 1) Creates the Payment record
    /// 2) If commandeId is set, updates Commande.MontantPaye / MontantRest
    /// 3) Recalculates Fact.Status (2 = fully paid, 1 = partial)
    /// </summary>
    Task<ServiceResult> RecordPaymentAsync(int factId, int? commandeId, decimal amount, string mode, string? note, string userLogin);
}
