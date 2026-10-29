using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthentificationApi.Controllers;

[ApiController]
[Route("api/authentication")]
public class AuthenticationController(IHttpClientFactory factory, IConfiguration configuration) : ControllerBase
{
    // L'ancien endpoint sans mot de passe ne doit pas contourner Keycloak.
    [HttpPost]
    public IActionResult Connexion() => StatusCode(StatusCodes.Status410Gone, new
    {
        message = "La connexion du TP5 est desactivee. Utilisez la connexion Keycloak du client."
    });

    [HttpGet("configuration")]
    public IActionResult ConfigurationKeycloak() => Ok(new
    {
        authority = configuration["Keycloak:Authority"],
        clientId = configuration["Keycloak:ClientId"]
    });

    // Code du TP5 conserve pour comparaison, mais jamais expose comme action HTTP.
    [NonAction]
    public async Task<IActionResult> ConnexionHistorique(LoginRequest demande)
    {
        if (string.IsNullOrWhiteSpace(demande.Login) || !Enum.IsDefined(demande.Role))
            return BadRequest(new { message = "Login ou rôle invalide." });

        User? user;
        try
        {
            using var response = await factory.CreateClient("Utilisateurs")
                .GetAsync("api/users/" + Uri.EscapeDataString(demande.Login.Trim()));
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound(new { message = "Cet utilisateur n'existe pas." });
            response.EnsureSuccessStatusCode();
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(new JsonStringEnumConverter());
            user = await response.Content.ReadFromJsonAsync<User>(options);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return StatusCode(503, new { message = "Le service utilisateurs est indisponible." });
        }

        // Partie 2, question 5b : recevoir un rôle ne signifie pas lui faire confiance.
        // Le rôle envoyé doit correspondre à celui enregistré en base.
        if (user is null || user.Role != demande.Role) return StatusCode(403, new { message = "Le rôle ne correspond pas à cet utilisateur." });

        var expiration = DateTime.UtcNow.AddMinutes(15);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Login),
            new Claim("role", user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            issuer: "AuthentificationApi", audience: "GestionOrdinateurs", claims: claims,
            expires: expiration,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256));

        // Sans mot de passe uniquement parce que l'énoncé le demande.
        return Ok(new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expiration));
    }
}
