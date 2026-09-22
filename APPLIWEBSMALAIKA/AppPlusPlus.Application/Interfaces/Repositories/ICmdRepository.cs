using AppPlusPlus.Domain.Entities.CommandesInternes;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface ICmdRepository : IRepository<Cmd>
{
    Task<Cmd?> GetWithDetailsAsync(int id);
    Task<List<Cmd>> GetBySupplierAsync(int supplierId);
    Task<List<Cmd>> GetByStatusAsync(int status);
    Task<List<Cmd>> GetByDateRangeAsync(DateOnly from, DateOnly to);
    Task<List<Cmd>> GetByUserAsync(string userLogin);
    Task<List<CmdDetail>> GetDetailsByCmdIdAsync(int cmdId);

    /// <summary>
    /// Returns all Cmds with Supplier and Details loaded, ordered by DateCommande descending.
    /// </summary>
    Task<List<Cmd>> GetAllWithDetailsAndSupplierAsync();

    /// <summary>
    /// Persists status corrections for the given Cmd entities.
    /// Only updates the Status column.
    /// </summary>
    Task UpdateCmdStatusBatchAsync(List<Cmd> cmds);
}
