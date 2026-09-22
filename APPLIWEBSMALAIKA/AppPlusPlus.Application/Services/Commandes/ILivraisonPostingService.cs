namespace AppPlusPlus.Application.Services.Commandes;

public interface ILivraisonPostingService
{
    Task<int> SaveAsync(LivraisonSaveRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int livraisonId, string userLogin, CancellationToken cancellationToken = default);
}
