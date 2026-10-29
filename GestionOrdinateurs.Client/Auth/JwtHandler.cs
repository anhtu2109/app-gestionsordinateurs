using System.Net;
using System.Net.Http.Headers;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components;

namespace GestionOrdinateurs.Client.Auth;

public class JwtHandler(ISessionStorageService storage, JwtAuthenticationStateProvider auth,
    NavigationManager navigation) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await auth.GetAuthenticationStateAsync();
        var token = await storage.GetItemAsync<string>("jwtToken");
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);
        // Un JWT expiré ou invalide ne doit pas laisser l'utilisateur sur une liste protégée.
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await auth.DeconnecterAsync();
            navigation.NavigateTo("login");
        }
        return response;
    }
}
