using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace GestionOrdinateurs.Client.Auth;

public class KeycloakClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor)
    : AccountClaimsPrincipalFactory<RemoteUserAccount>(accessor)
{
    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
        RemoteUserAccount account, RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);
        // Keycloak transmet un tableau JSON ; IsInRole attend un claim par role.
        if (user.Identity is ClaimsIdentity identity && identity.IsAuthenticated
            && !string.IsNullOrWhiteSpace(options.RoleClaim)
            && account.AdditionalProperties.TryGetValue(options.RoleClaim, out var value)
            && value is JsonElement roles && roles.ValueKind == JsonValueKind.Array)
        {
            foreach (var claim in identity.FindAll(options.RoleClaim).ToList())
                identity.RemoveClaim(claim);
            foreach (var role in roles.EnumerateArray())
                if (role.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(role.GetString()))
                    identity.AddClaim(new Claim(options.RoleClaim, role.GetString()!));
        }
        return user;
    }
}
