namespace AppPlusPlus.Application.DTOs.Approvisionnement;

public class ApproDetailDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public DateTime DateSys { get; set; }
    public string? Reference { get; set; }
    public string Commentaire { get; set; } = "";
    public decimal Taux { get; set; }
    public string User { get; set; } = "";
    public string LocalisationLabel { get; set; } = "";
    public string? SupplierName { get; set; }
    public bool IsFromCommand { get; set; }
    public int? CommandId { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsTransfer { get; set; }
    public int LineCount { get; set; }
    public decimal TotalQte { get; set; }
    public decimal TotalAchatFc { get; set; }
    public decimal TotalVenteFc { get; set; }
    public decimal BenTotalFc { get; set; }
    public List<ApproDetailLineDto> Lines { get; set; } = new();
}

public class ApproDetailLineDto
{
    public int Id { get; set; }
    public string IdArticle { get; set; } = "";
    public string ArticleName { get; set; } = "";
    public string LocalisationLabel { get; set; } = "";
    public decimal Qte { get; set; }
    public decimal? PA { get; set; }
    public decimal? PV { get; set; }
    public decimal MontantAchatFc { get; set; }
    public decimal MontantVenteFc { get; set; }
    public decimal BenLineFc { get; set; }
    public DateOnly? DateExpiration { get; set; }
}
