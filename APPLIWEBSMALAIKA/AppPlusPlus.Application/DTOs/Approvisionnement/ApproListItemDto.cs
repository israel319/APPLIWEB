namespace AppPlusPlus.Application.DTOs.Approvisionnement;

public class ApproListItemDto
{
    public int Id { get; set; }
    public string ArticleName { get; set; } = "";
    public decimal Qte { get; set; }
    public decimal PA { get; set; }
    public decimal PV { get; set; }
    public decimal Ben { get; set; }
    public decimal TotalAchatFc { get; set; }
    public decimal BenTotalFc { get; set; }
    public string? Commentaire { get; set; }
    public bool IsUsd { get; set; } = true;
    public decimal Taux { get; set; }
    public DateOnly Date { get; set; }
    public string User { get; set; } = "";
    public string? Reference { get; set; }
    public string LocalisationLabel { get; set; } = "";
    public bool IsFromCommand { get; set; }
    public int? CommandId { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsTransfer { get; set; }
    public int? StatusId { get; set; }
    public int LineCount { get; set; } = 1;
}
