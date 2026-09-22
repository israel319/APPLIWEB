namespace AppPlusPlus.Application.DTOs.Vente;

/// <summary>
/// Total encaissé par un opérateur pour une journée (paiements enregistrés).
/// </summary>
public class OperatorDailySalesDto
{
    public decimal CollectedUsd { get; set; }
    public int PaymentCount { get; set; }
}
