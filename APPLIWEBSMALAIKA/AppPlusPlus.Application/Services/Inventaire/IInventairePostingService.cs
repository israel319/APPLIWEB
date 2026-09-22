using AppPlusPlus.Application.DTOs.Inventaire;

namespace AppPlusPlus.Application.Services.Inventaire;

public interface IInventairePostingService
{
    Task CloseReceptionAsync(int receptionId, string userLogin, CancellationToken cancellationToken = default);
}
