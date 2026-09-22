namespace AppPlusPlus.Domain.Enums.GRH;

/// <summary>
/// Statuts des paies
/// </summary>
public enum PayrollStatusEnum
{
    /// <summary>Brouillon</summary>
    Draft = 1,
    
    /// <summary>Préparée</summary>
    Prepared = 2,
    
    /// <summary>Approuvée</summary>
    Approved = 3,
    
    /// <summary>Payée</summary>
    Paid = 4,
    
    /// <summary>Annulée</summary>
    Cancelled = 5
}
