namespace AppPlusPlus.Application.DTOs.Demandes;

public class DemandeDetailLineDto
{
    public int DetailId { get; set; }
    public string ArticleId { get; set; } = string.Empty;
    public string? ArticleDescription { get; set; }
    public decimal QteDemandee { get; set; }
    public decimal? QteApprouvee { get; set; }
    public decimal DepotQte { get; set; }
    public int DepotSeuil { get; set; }
    public decimal QteMaxApprovable { get; set; }
}

public class DemandeRowDto
{
    public int IdDemande { get; set; }
    public int Statut { get; set; }
    public string StatutLabel { get; set; } = string.Empty;
    public string? LocalisationDemandeur { get; set; }
    public string? LocalisationSource { get; set; }
    public DateTime DateCreation { get; set; }
    public int NbArticles { get; set; }
    public decimal TotalQteDemandee { get; set; }
    public bool CanAdminApprove { get; set; }
    public bool CanAgentApprove { get; set; }
}

public class DemandeActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
