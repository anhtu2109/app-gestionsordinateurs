using GestionOrdinateurs.Shared;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Evenements;
using Microsoft.EntityFrameworkCore;
namespace GestionOrdinateurs.API.Services;

public class OrdinateurService(IDbContextFactory<ParcInformatiqueContext> fabrique) : IOrdinateurService
{
    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;

    // Le singleton ne partage jamais un contexte entre plusieurs requêtes.
    public async Task<List<Ordinateur>> ListerAsync()
    {
        await using var contexte = await fabrique.CreateDbContextAsync();
        return await contexte.Ordinateurs.AsNoTracking().OrderBy(o => o.Id).ToListAsync();
    }

    public async Task<Ordinateur?> TrouverParIdAsync(int id)
    {
        await using var contexte = await fabrique.CreateDbContextAsync();
        return await contexte.Ordinateurs.AsNoTracking().SingleOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Ordinateur> AjouterAsync(Ordinateur ordinateur)
    {
        await using var contexte = await fabrique.CreateDbContextAsync();
        contexte.Ordinateurs.Add(ordinateur);
        await contexte.SaveChangesAsync();
        OrdinateurAjoute?.Invoke(this, new OrdinateurAjouteEventArgs(ordinateur));
        return ordinateur;
    }

    public async Task<bool> ModifierAsync(int id, string nom, Caracteristiques caracteristiques,
        EtatOrdinateur etat, int stock, decimal prix)
    {
        await using var contexte = await fabrique.CreateDbContextAsync();
        var ordinateur = await contexte.Ordinateurs.SingleOrDefaultAsync(o => o.Id == id);
        if (ordinateur == null) return false;
        if (!Enum.IsDefined(etat)) throw new ArgumentException("L'état est invalide.", nameof(etat));
        ordinateur.ModifierVente(stock, prix);
        ordinateur.ModifierInformations(nom, caracteristiques);
        ordinateur.ChangerEtat(etat);
        await contexte.SaveChangesAsync();
        OrdinateurMisAJour?.Invoke(this, new OrdinateurMisAJourEventArgs(ordinateur));
        return true;
    }

    public async Task<bool> SupprimerAsync(int id)
    {
        await using var contexte = await fabrique.CreateDbContextAsync();
        var ordinateur = await contexte.Ordinateurs.SingleOrDefaultAsync(o => o.Id == id);
        if (ordinateur == null) return false;
        contexte.Ordinateurs.Remove(ordinateur);
        await contexte.SaveChangesAsync();
        // Aucun évènement de suppression n'est envoyé pour un identifiant absent.
        OrdinateurSupprime?.Invoke(this, new OrdinateurSupprimeEventArgs(ordinateur));
        return true;
    }
    public async Task<List<Ordinateur>> ListerDisponiblesAsync()
    {
        await using var contexte = await fabrique.CreateDbContextAsync();
        return await contexte.Ordinateurs.AsNoTracking()
            .Where(o => o.Stock > 0).OrderBy(o => o.Id).ToListAsync();
    }

    public async Task<List<Ordinateur>> RechercherParMarqueAsync(string marque)
    {
        await using var contexte = await fabrique.CreateDbContextAsync();
        return await contexte.Ordinateurs.AsNoTracking()
            .Where(o => o.Caracteristiques.Marque == marque).OrderBy(o => o.Id).ToListAsync();
    }

    public async Task<List<Ordinateur>> TrierParPrixAsync()
    {
        await using var contexte = await fabrique.CreateDbContextAsync();
        return await contexte.Ordinateurs.AsNoTracking()
            .OrderByDescending(o => o.Prix).ThenBy(o => o.Id).ToListAsync();
    }

    public string Diagnostiquer(Ordinateur ordinateur)
    {
        // HorsService correspond au « HS » utilisé dans l'exemple de l'énoncé.
        return ordinateur switch
        {
            { Etat: EtatOrdinateur.HorsService } => "Remplacement",
            { Etat: EtatOrdinateur.Lent, Caracteristiques: { RamGo: < 8 } } => "Ajouter RAM",
            _ => "Aucune action"
        };
    }

    public async Task<(int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations)>
        CalculerStatistiquesAsync()
    {
        await using var contexte = await fabrique.CreateDbContextAsync();
        var ordinateurs = await contexte.Ordinateurs.AsNoTracking().ToListAsync();
        int reparations = await contexte.Reparations.CountAsync();
        // Le calcul en decimal évite les arrondis d'une somme REAL dans SQLite.
        return (ordinateurs.Count, ordinateurs.Count(o => o.Stock > 0),
            ordinateurs.Sum(o => o.Prix * o.Stock), reparations);
    }
}
