using AppPlusPlus.Domain.Entities.CommandesInternes;

namespace AppPlusPlus.Application.DTOs.Commandes;

/// <summary>
/// Flattened row for internal orders (Cmd) displayed in CommandeHub grid.
/// Carries the entity plus pre-computed aggregates derived from order details.
/// </summary>
public class CmdRowDto
{
    public Cmd Cmd { get; set; } = null!;
    public int NbArticles { get; set; }
    public decimal TotalOrder { get; set; }
    public decimal TotalReceived { get; set; }
    public bool HasRemaining { get; set; }
    public bool HasStock { get; set; }
}
