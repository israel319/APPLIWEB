namespace AppPlusPlus.Application.DTOs.Vente;

/// <summary>
/// Totaux encaissés hier et aujourd'hui pour un opérateur.
/// </summary>
public class OperatorSalesSummaryDto
{
    public OperatorDailySalesDto Today { get; set; } = new();
    public OperatorDailySalesDto Yesterday { get; set; } = new();
    public decimal CombinedUsd => Today.CollectedUsd + Yesterday.CollectedUsd;
}
