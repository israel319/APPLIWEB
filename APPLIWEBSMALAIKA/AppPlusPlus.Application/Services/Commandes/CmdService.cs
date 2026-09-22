using AppPlusPlus.Application.DTOs.Commandes;
using AppPlusPlus.Domain.Entities.CommandesInternes;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Application.Services.Commandes;

public class CmdService : ICmdService
{
    private readonly ICmdRepository _cmdRepo;

    public CmdService(ICmdRepository cmdRepo)
    {
        _cmdRepo = cmdRepo;
    }

    public async Task<List<CmdRowDto>> GetCmdsWithStatusUpdateAsync()
    {
        // 1. Load all Cmds with Supplier + Details
        var cmds = await _cmdRepo.GetAllWithDetailsAndSupplierAsync();

        // 2. Project into CmdRowDto with aggregated quantities
        var rows = cmds.Select(c =>
        {
            var totalOrder = c.Details.Sum(d => (decimal)(d.QteOrder ?? 0));
            var totalReceived = c.Details.Sum(d => (decimal)(d.QteReceive ?? 0));
            return new CmdRowDto
            {
                Cmd = c,
                NbArticles = c.Details.Count,
                TotalOrder = totalOrder,
                TotalReceived = Math.Min(totalReceived, totalOrder),
                HasRemaining = totalOrder > totalReceived,
                HasStock = totalReceived > 0
            };
        }).ToList();

        // 3. Auto-correct statuses derived from actual quantities
        var cmdsToUpdate = new List<Cmd>();
        foreach (var row in rows)
        {
            // Skip cancelled orders -- status 3 is manually set, never auto-corrected
            if (row.Cmd.Status == 3) continue;

            int expected;
            if (row.TotalReceived >= row.TotalOrder && row.TotalOrder > 0)
                expected = 2; // Fully received
            else if (row.TotalReceived > 0)
                expected = 1; // Partially received (in progress)
            else
                expected = 0; // New / nothing received

            if (row.Cmd.Status != expected)
            {
                row.Cmd.Status = expected;
                cmdsToUpdate.Add(row.Cmd);
            }
        }

        // 4. Persist any corrected statuses
        if (cmdsToUpdate.Any())
        {
            await _cmdRepo.UpdateCmdStatusBatchAsync(cmdsToUpdate);
        }

        return rows;
    }

    public async Task<List<Cmd>> GetCmdsWithDetailsAsync()
    {
        return await _cmdRepo.GetAllWithDetailsAndSupplierAsync();
    }

    public async Task DeleteCmdAsync(int cmdId)
    {
        var cmd = await _cmdRepo.GetByIdAsync(cmdId);
        if (cmd != null)
            await _cmdRepo.DeleteAsync(cmd);
    }

    public async Task<Cmd?> GetCmdWithDetailsAsync(int cmdId)
        => await _cmdRepo.GetWithDetailsAsync(cmdId);
}
