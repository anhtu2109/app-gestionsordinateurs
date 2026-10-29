using GestionOrdinateurs.Shared;
namespace GestionOrdinateurs.API.Evenements;

// L'abonné reçoit l'ordinateur après la sauvegarde.
public class OrdinateurAjouteEventArgs(Ordinateur ordinateur) : EventArgs
{
    public Ordinateur Ordinateur { get; } = ordinateur;
}
