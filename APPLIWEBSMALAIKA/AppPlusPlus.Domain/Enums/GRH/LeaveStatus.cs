namespace AppPlusPlus.Domain.Enums.GRH;

/// <summary>
/// Statuts des demandes de congés
/// </summary>
public enum LeaveStatusEnum
{
    /// <summary>Brouillon</summary>
    Draft = 1,
    
    /// <summary>En attente d'approbation</summary>
    Pending = 2,
    
    /// <summary>Approuvé</summary>
    Approved = 3,
    
    /// <summary>Rejeté</summary>
    Rejected = 4,
    
    /// <summary>Annulé</summary>
    Cancelled = 5
}
