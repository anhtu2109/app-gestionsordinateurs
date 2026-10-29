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

    // TP6 : le client ne recherche plus publiquement un role avant connexion.
    [Authorize(Roles = "Admin")]
    [HttpGet("{login}")]
    public async Task<IActionResult> ParLogin(string login)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Login == login);
        return user is null ? NotFound(new { message = "Cet utilisateur n'existe pas." }) : Ok(user);
    }
}
