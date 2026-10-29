using GestionOrdinateurs.API.Services;

namespace GestionOrdinateurs.API.Evenements;

public class JournalOrdinateurs(ILogger<JournalOrdinateurs> logger, JournalService journal)
{
    public void AfficherMessageAjout(object? sender, OrdinateurAjouteEventArgs e)
    {
        logger.LogInformation("Ordinateur {Id} ajouté.", e.Ordinateur.Id);
        journal.EcrireOrdinateur($"{e.Ordinateur.Nom} ajouté (Id {e.Ordinateur.Id})");
    }

    public void AfficherMessageSuppression(object? sender, OrdinateurSupprimeEventArgs e)
    {
        logger.LogInformation("Ordinateur {Id} supprimé.", e.Ordinateur.Id);
        journal.EcrireOrdinateur($"{e.Ordinateur.Nom} supprimé (Id {e.Ordinateur.Id})");
    }

    public void AfficherMessageMiseAJour(object? sender, OrdinateurMisAJourEventArgs e)
    {
        logger.LogInformation("Ordinateur {Id} mis à jour.", e.Ordinateur.Id);
        journal.EcrireOrdinateur($"{e.Ordinateur.Nom} modifié (Id {e.Ordinateur.Id})");
    }
}
