namespace AppPlusPlus.Application.Services.Approvisionnement;

public class ApproLineInput
{
    public string IdArticle { get; set; } = string.Empty;
    public decimal Qte { get; set; }
    public decimal? PA { get; set; }
    public decimal? PV { get; set; }
    public DateOnly? DateExpiration { get; set; }
}

public class DirectApproSaveRequest
{
    public int? ApproId { get; set; }
    public string? Reference { get; set; }
    public int? SupplierId { get; set; }
    public DateOnly Date { get; set; }
    public int LocalisationId { get; set; }
    public string Commentaire { get; set; } = "Approvisionnement";
    public List<ApproLineInput> Lines { get; set; } = new();
    public string UserLogin { get; set; } = string.Empty;
    public decimal Taux { get; set; }
}

public class CommandReceptionLineInput
{
    public int CmdDetailId { get; set; }
    public string IdArticle { get; set; } = string.Empty;
    public decimal Qte { get; set; }
    public decimal? PA { get; set; }
    public decimal? PV { get; set; }
    public DateOnly? DateExpiration { get; set; }
}

public class CommandReceptionRequest
{
    public int CmdId { get; set; }
    public int LocalisationId { get; set; }
    public int? SupplierId { get; set; }
    public List<CommandReceptionLineInput> Lines { get; set; } = new();
    public string UserLogin { get; set; } = string.Empty;
    public decimal Taux { get; set; }
}
