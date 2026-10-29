using GestionOrdinateurs.API.Models;
using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Gestionnaire")]
[ApiController]
[Route("api/[controller]")]
public class OrdinateursController : ControllerBase
{
    private readonly IOrdinateurService service;
    private readonly ILogger<OrdinateursController> logger;
    private readonly IConfiguration configuration;

    public OrdinateursController(IOrdinateurService service, ILogger<OrdinateursController> logger, IConfiguration configuration)
    {
        this.service = service;
        this.logger = logger;
        this.configuration = configuration;
    }

    // Objectif API distante : le prix en base reste en EUR, seul l'affichage est converti.
    [HttpGet("devises/{devise}")]
    public async Task<IActionResult> Taux(string devise, [FromServices] IHttpClientFactory factory)
    {
        devise = devise.ToUpperInvariant();
        if (devise == "EUR") return Ok(new { devise, taux = 1m });
        if (devise is not ("USD" or "GBP")) return BadRequest(new { message = "Devise non prise en charge." });
        try
        {
            using var response = await factory.CreateClient("Devises").GetAsync($"v1/latest?base=EUR&symbols={devise}");
            response.EnsureSuccessStatusCode();
            using var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return Ok(new { devise, taux = json.RootElement.GetProperty("rates").GetProperty(devise).GetDecimal() });
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException or KeyNotFoundException)
        {
            return StatusCode(503, new { message = "Le service de devises est indisponible. Les prix restent en EUR." });
        }
    }

    [HttpGet("info")]
    public IActionResult Informations()
    {
        logger.LogInformation("Informations de l’application demandées.");
        return Ok(new { Name = configuration["Application:Name"], Version = configuration["Application:Version"] });
    }

    [HttpGet]
    public async Task<IActionResult> Lister()
    {
        logger.LogInformation("Liste des ordinateurs demandée.");
        return Ok(await service.ListerAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetComputerById(int id)
    {
        logger.LogInformation("Recherche de l’ordinateur {Id}.", id);
        var ordinateur = await service.TrouverParIdAsync(id);
        if (ordinateur == null)
        {
            return NotFound();
        }

        return Ok(ordinateur);
    }

    // Partie 3 : les lectures sont partagées, les mutations sont réservées à Admin.
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Ajouter(OrdinateurRequest demande)
    {
        logger.LogInformation("Ajout d’un ordinateur demandé.");
        var ordinateur = new Ordinateur(demande.Nom, demande.Caracteristiques.VersModele());
        ordinateur.ModifierVente(demande.Stock, demande.Prix);
        ordinateur.ChangerEtat(demande.Etat);
        await service.AjouterAsync(ordinateur);

        // La réponse indique aussi l'adresse du nouvel ordinateur.
        return CreatedAtAction(nameof(GetComputerById), new { id = ordinateur.Id }, ordinateur);
    }

    // Partie 3 : les lectures sont partagées, les mutations sont réservées à Admin.
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modifier(int id, OrdinateurRequest demande)
    {
        logger.LogInformation("Modification de l’ordinateur {Id}.", id);
        bool modifie = await service.ModifierAsync(id, demande.Nom, demande.Caracteristiques.VersModele(),
            demande.Etat, demande.Stock, demande.Prix);
        if (!modifie)
        {
            return NotFound();
        }

        return Ok(await service.TrouverParIdAsync(id));
    }

    // Partie 3 : les lectures sont partagées, les mutations sont réservées à Admin.
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int id)
    {
        logger.LogInformation("Suppression de l’ordinateur {Id}.", id);
        if (!await service.SupprimerAsync(id))
        {
            return NotFound();
        }

        return NoContent();
    }
    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles()
    {
        logger.LogInformation("Recherche des ordinateurs en stock.");
        return Ok(await service.ListerDisponiblesAsync());
    }

    [HttpGet("marque/{marque}")]
    public async Task<IActionResult> ParMarque(string marque)
    {
        logger.LogInformation("Recherche de la marque {Marque}.", marque);
        return Ok(await service.RechercherParMarqueAsync(marque));
    }

    [HttpGet("prix")]
    public async Task<IActionResult> ParPrix()
    {
        logger.LogInformation("Tri des ordinateurs par prix décroissant.");
        return Ok(await service.TrierParPrixAsync());
    }

    [HttpGet("{id:int}/diagnostic")]
    public async Task<IActionResult> Diagnostic(int id)
    {
        logger.LogInformation("Diagnostic du poste {Id}.", id);
        var ordinateur = await service.TrouverParIdAsync(id);
        if (ordinateur == null) return NotFound();
        return Ok(new { diagnostic = service.Diagnostiquer(ordinateur) });
    }

    [HttpGet("statistiques")]
    public async Task<IActionResult> Statistiques()
    {
        logger.LogInformation("Calcul des statistiques.");
        var (total, enStock, valeur, reparations) = await service.CalculerStatistiquesAsync();
        return Ok(new { totalOrdinateur = total, ordiDisponibles = enStock,
            valeurStock = valeur, nbReparations = reparations });
    }

    [HttpGet("inventaire/{nom}")]
    public async Task<IActionResult> Chercher(string nom)
    {
        logger.LogInformation("Recherche dans l'inventaire : {Nom}.", nom);
        var inventaire = new InventaireOrdinateurs { Ordinateurs = await service.ListerAsync() };
        var ordinateur = inventaire[nom];
        return ordinateur == null ? NotFound() : Ok(ordinateur);
    }

    [HttpGet("inventaire/id/{id:int}")]
    public async Task<IActionResult> ChercherParId(int id)
    {
        logger.LogInformation("Recherche dans l'inventaire : {Id}.", id);
        var inventaire = new InventaireOrdinateurs { Ordinateurs = await service.ListerAsync() };
        var ordinateur = inventaire[id];
        return ordinateur == null ? NotFound() : Ok(ordinateur);
    }
}
