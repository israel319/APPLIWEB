namespace AppPlusPlus.Application.Services.Stock;

public interface IInternalTransferService
{
    Task<int> SaveAsync(InternalTransferSaveRequest request, CancellationToken cancellationToken = default);

    Task CancelAsync(int approId, string userLogin, CancellationToken cancellationToken = default);

    Task DeleteAsync(int approId, CancellationToken cancellationToken = default);
}
