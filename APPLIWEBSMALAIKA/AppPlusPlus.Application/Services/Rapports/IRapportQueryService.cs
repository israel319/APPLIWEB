using AppPlusPlus.Application.DTOs.Rapports;
using AppPlusPlus.Domain.Entities.Finance;

namespace AppPlusPlus.Application.Services.Rapports;

public interface IRapportQueryService
{
    Task<decimal> GetLatestTauxAsync(CancellationToken cancellationToken = default);

    Task<BilanReportResult> GetBilanAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HistoriqueReportRowDto>> GetHistoriqueAsync(
        DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VenteLigneReportDto>> GetVentesLignesAsync(
        DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<FacturesMoisReportResult> GetFacturesMoisAsync(
        int year, int month, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockReportRowDto>> GetStockAsync(
        IReadOnlyList<int> localisationIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CommandeReportRowDto>> GetCommandesFournisseurAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);

    Task<ApproMargesReportResult> GetApproMargesAsync(
        DateOnly? from, DateOnly? to, IReadOnlyList<int> localisationIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MouvementReportRowDto>> GetMouvementsAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}
