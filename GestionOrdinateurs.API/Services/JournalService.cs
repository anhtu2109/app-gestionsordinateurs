namespace GestionOrdinateurs.API.Services;

public class JournalService
{
    private readonly string dossier;
    private readonly object verrou = new();

    public JournalService(string dossier)
    {
        this.dossier = dossier;
        Directory.CreateDirectory(Path.Combine(dossier, "Reparations"));
        Directory.CreateDirectory(Path.Combine(dossier, "Ordinateurs"));
    }

    public void Ecrire(string message) => EcrireDans("Reparations", "reparations.log", message);

    public void EcrireOrdinateur(string message) => EcrireDans("Ordinateurs", "ordinateurs.log", message);

    private void EcrireDans(string categorie, string fichier, string message)
    {
        // Le même singleton protège les écritures des réparations simultanées.
        lock (verrou)
        {
            using var writer = new StreamWriter(Path.Combine(dossier, categorie, fichier), append: true);
            string ligne = message.Replace("\r", " ").Replace("\n", " ");
            writer.WriteLine($"{DateTime.Now:dd/MM/yyyy} - {ligne}");
        }
        // using ferme le fichier immédiatement, sans attendre le Garbage Collector.
    }
}
