using GestionOrdinateurs.Client;
using GestionOrdinateurs.Client.Auth;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
// Le client public utilise Authorization Code + PKCE, sans secret embarque.
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Keycloak", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("email");
    options.UserOptions.NameClaim = "preferred_username";
    options.UserOptions.RoleClaim = "roles";
}).AddAccountClaimsPrincipalFactory<KeycloakClaimsPrincipalFactory>();
builder.Services.AddTransient<KeycloakAuthorizationHandler>();
// Partie 5 : une seule adresse pour le client, toujours celle de la Gateway.
builder.Services.AddHttpClient("Gateway", client =>
    client.BaseAddress = new Uri(builder.Configuration["GatewayUrl"]!))
    .AddHttpMessageHandler<KeycloakAuthorizationHandler>();
builder.Services.AddScoped(s => s.GetRequiredService<IHttpClientFactory>().CreateClient("Gateway"));
await builder.Build().RunAsync();
