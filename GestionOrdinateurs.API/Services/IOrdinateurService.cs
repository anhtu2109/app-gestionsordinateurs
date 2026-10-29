using GestionOrdinateurs.Shared;
using GestionOrdinateurs.API.Evenements;
namespace GestionOrdinateurs.API.Services;

public interface IOrdinateurService
{
    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;
    Task<List<Ordinateur>> ListerAsync();
    Task<Ordinateur?> TrouverParIdAsync(int id);
    Task<Ordinateur> AjouterAsync(Ordinateur ordinateur);
    Task<bool> ModifierAsync(int id, string nom, Caracteristiques caracteristiques, EtatOrdinateur etat, int stock, decimal prix);
    Task<bool> SupprimerAsync(int id);
    Task<List<Ordinateur>> ListerDisponiblesAsync();
    Task<List<Ordinateur>> RechercherParMarqueAsync(string marque);
    Task<List<Ordinateur>> TrierParPrixAsync();
    string Diagnostiquer(Ordinateur ordinateur);
    Task<(int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations)> CalculerStatistiquesAsync();
}
