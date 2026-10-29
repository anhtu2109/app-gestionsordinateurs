using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace GestionOrdinateurs.Client.Auth;

public class KeycloakAuthorizationHandler : AuthorizationMessageHandler
{
    public KeycloakAuthorizationHandler(IAccessTokenProvider provider,
        NavigationManager navigation, IConfiguration configuration) : base(provider, navigation)
    {
        // Le jeton est ajoute uniquement aux appels vers notre Gateway.
        ConfigureHandler(authorizedUrls: [configuration["GatewayUrl"]!]);
    }
}
