namespace AppPlusPlus.Application.DTOs.Finance;

public class ClotureSummaryDto
{
    public int NbFactures { get; set; }
    public decimal TotalFactures { get; set; }
    public int NbPaiements { get; set; }
    public decimal TotalPaiements { get; set; }
    public bool DejaClotureExiste { get; set; }

    public int NbOperations { get; set; }
    public decimal MontantTotal => TotalPaiements > 0 ? TotalPaiements : TotalFactures;

    /// <summary>Une clôture approuvée existe déjà pour cette date — seules les ventes postérieures sont comptées.</summary>
    public bool ApresCloturePrecedente { get; set; }
}
