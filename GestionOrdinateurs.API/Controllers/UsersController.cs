using GestionOrdinateurs.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(ParcInformatiqueContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Tous() => Ok(await db.Users.AsNoTracking().ToListAsync());

    // Partie 2 : recherche publique nécessaire avant la connexion sans mot de passe du TP.
    [AllowAnonymous]
    [HttpGet("{login}")]
    public async Task<IActionResult> ParLogin(string login)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Login == login);
        return user is null ? NotFound(new { message = "Cet utilisateur n'existe pas." }) : Ok(user);
    }
}
