using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestionOrdinateurs.Client.Auth;

public class JwtAuthenticationStateProvider(ISessionStorageService storage) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await storage.GetItemAsync<string>("jwtToken");
        var identity = new ClaimsIdentity();
        if (!string.IsNullOrWhiteSpace(token))
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                if (jwt.ValidTo > DateTime.UtcNow)
                {
                    // Partie 6 : cette lecture sert à l'IHM, seule l'API valide la signature.
                    identity = new ClaimsIdentity(jwt.Claims, "jwt", "unique_name", "role");
                }
                else await storage.RemoveItemAsync("jwtToken");
            }
            catch (ArgumentException)
            {
                await storage.RemoveItemAsync("jwtToken");
            }
        }
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task ConnecterAsync(string token)
    {
        // Partie 2, questions 6 et 7 : sessionStorage survit au rafraîchissement de cet onglet.
        await storage.SetItemAsync("jwtToken", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task DeconnecterAsync()
    {
        // Question 8 : on retire le token et on avertit les composants abonnés.
        await storage.RemoveItemAsync("jwtToken");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
