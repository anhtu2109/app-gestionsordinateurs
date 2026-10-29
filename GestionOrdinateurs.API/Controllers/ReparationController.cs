using GestionOrdinateurs.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Gestionnaire")]
[ApiController]
[Route("api/reparation")]
[Route("api/reparations")]
public class ReparationController(
    IOrdinateurService ordinateurs,
    ReparationService reparations,
    StockService stock,
    ILogger<ReparationController> logger) : ControllerBase
{
    // Partie 3 : les lectures sont partagées, les mutations sont réservées à Admin.
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    [HttpPost("SimulerReparation")]
    public async Task<IActionResult> SimulerReparation([FromQuery] string technicien = "Atelier")
    {
        logger.LogInformation("Simulation de cinq réparations simultanées.");
        var liste = await ordinateurs.ListerAsync();
        if (liste.Count < 5)
            return BadRequest(new { message = "Ajouter au moins cinq ordinateurs avant la simulation." });

        if (string.IsNullOrWhiteSpace(technicien))
            return BadRequest(new { message = "Le technicien est obligatoire." });
        await Task.WhenAll(liste.Take(5)
            .Select(ordinateur => reparations.SimulerReparationAsync(ordinateur, technicien)));
        return Ok(new { message = "Les cinq réparations sont terminées." });
    }

    [HttpGet]
    public async Task<IActionResult> Lister()
    {
        logger.LogInformation("Liste des réparations et de leurs ordinateurs.");
        return Ok(await reparations.ListerAsync());
    }

    // Partie 3 : les lectures sont partagées, les mutations sont réservées à Admin.
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    [HttpGet("GetStockSSD")]
    public async Task<IActionResult> GetStockSSD()
    {
        // Les dix tâches sont attendues avant de lire le stock final.
        await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => stock.ConsommerSSD()));
        logger.LogInformation("Stock SSD après consommation : {StockSSD}.", stock.StockSSD);
        return Ok(stock.StockSSD);
    }

    [HttpGet("historique")]
    public async Task<IActionResult> Historique()
    {
        logger.LogInformation("Lecture de l'historique des réparations.");
        return Ok(await reparations.HistoriqueAsync());
    }
}
