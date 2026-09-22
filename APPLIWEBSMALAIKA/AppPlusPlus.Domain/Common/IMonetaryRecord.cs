namespace AppPlusPlus.Domain.Common;

/// <summary>
/// Quatuor monétaire standard persisté en base :
/// Montant (devise native) + Id_Monais + Taux + Montant_Apres_Conversion.
/// </summary>
public interface IMonetaryRecord
{
    decimal? Montant { get; set; }
    int? MoneyId { get; set; }
    decimal? Taux { get; set; }
    decimal? MontantApresConversion { get; set; }
}
