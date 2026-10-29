using Blazored.SessionStorage;
using GestionOrdinateurs.Client;
using GestionOrdinateurs.Client.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddTransient<JwtHandler>();
// Partie 5 : une seule adresse pour le client, toujours celle de la Gateway.
builder.Services.AddHttpClient("Gateway", client =>
    client.BaseAddress = new Uri(builder.Configuration["GatewayUrl"]!))
    .AddHttpMessageHandler<JwtHandler>();
builder.Services.AddScoped(s => s.GetRequiredService<IHttpClientFactory>().CreateClient("Gateway"));
await builder.Build().RunAsync();
