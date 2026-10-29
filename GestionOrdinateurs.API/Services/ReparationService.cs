using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public class ReparationService(
    IDbContextFactory<ParcInformatiqueContext> fabrique,
    ILogger<ReparationService> logger, JournalService journal)
{
    public async Task SimulerReparationAsync(Ordinateur ordinateur, string technicien = "Atelier")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(technicien);
        // Task.WhenAll appelle cette méthode plusieurs fois : chaque appel a son contexte.
        await using var contexte = await fabrique.CreateDbContextAsync();
        var reparation = new Reparation
        {
            OrdinateurId = ordinateur.Id,
            Technicien = technicien,
            TypeReparation = "Diagnostic et remise en état",
            DateDebut = DateTime.UtcNow,
            Statut = "En cours"
        };
        contexte.Reparations.Add(reparation);
        await contexte.SaveChangesAsync();

        await Task.Delay(2000);
        reparation.Statut = "Terminée";
        await contexte.SaveChangesAsync();
        logger.LogInformation("{Nom} réparé.", ordinateur.Nom);
        journal.Ecrire($"{ordinateur.Nom} réparé par {technicien}");
    }

    public async Task<List<Reparation>> ListerAsync()
    {
        await using var contexte = await fabrique.CreateDbContextAsync();
        // Include charge les informations de l'ordinateur dans la même requête.
        return await contexte.Reparations.AsNoTracking()
            .Include(r => r.UnOrdinateur).OrderByDescending(r => r.DateDebut)
            .ThenBy(r => r.Id).ToListAsync();
    }

    public record HistoriqueReparationRecord(
        int OrdinateurId,
        string NomOrdinateur,
        string Technicien,
        string TypeReparation,
        DateTime DateIntervention);

    public async Task<List<HistoriqueReparationRecord>> HistoriqueAsync()
    {
        await using var contexte = await fabrique.CreateDbContextAsync();
        return await contexte.Reparations.AsNoTracking()
            .Where(r => r.Statut == "Terminée").OrderByDescending(r => r.DateDebut)
            .Select(r => new HistoriqueReparationRecord(r.OrdinateurId, r.UnOrdinateur.Nom,
                r.Technicien, r.TypeReparation, r.DateDebut)).ToListAsync();
    }
}
