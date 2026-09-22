namespace AppPlusPlus.Application.Services.Approvisionnement;

public interface IApproPostingService
{
    Task<int> SaveDirectApproAsync(DirectApproSaveRequest request, CancellationToken cancellationToken = default);

    Task<int> ReceiveFromCommandAsync(CommandReceptionRequest request, CancellationToken cancellationToken = default);

    Task CancelApproAsync(int approId, string userLogin, CancellationToken cancellationToken = default);

    Task DeleteApproAsync(int approId, CancellationToken cancellationToken = default);
}
