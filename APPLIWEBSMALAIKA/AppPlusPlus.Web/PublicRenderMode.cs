using Microsoft.AspNetCore.Components.Web;

namespace AppPlusPlus.Web;

/// <summary>Mode interactif sans prerender — évite double chargement BDD/JS sur hébergement mutualisé.</summary>
public static class PublicRenderMode
{
    public static readonly InteractiveServerRenderMode Instance = new(prerender: false);
}
