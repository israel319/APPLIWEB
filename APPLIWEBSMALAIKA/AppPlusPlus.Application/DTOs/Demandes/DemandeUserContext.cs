using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Application.DTOs.Demandes;

public class DemandeUserContext
{
    public string Login { get; init; } = "";
    public string? RoleDescription { get; init; }
    public PermissionSnapshot Permissions { get; init; } = PermissionSnapshot.Empty;
    public List<int> LocalisationIds { get; init; } = new();

    public bool HasGlobalScope => SystemRolePolicy.HasGlobalDemandeScope(RoleDescription);

    public bool CanApproveDepot =>
        SystemRolePolicy.CanApproveDemandeDepot(RoleDescription, Permissions);

    public bool CanConfirmForLocalisation(int localisationDemandeurId) =>
        SystemRolePolicy.CanConfirmDemandeMagasin(Permissions, localisationDemandeurId, LocalisationIds);
}
