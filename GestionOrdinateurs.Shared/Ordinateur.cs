using System.Text.Json.Serialization;

namespace GestionOrdinateurs.Shared;

public class Ordinateur
{
    public int Id { get; private set; }
    public string Nom { get; private set; }
    public EtatOrdinateur Etat { get; private set; }
    public int Stock { get; private set; }
    public decimal Prix { get; private set; }

    // Partie 4 : les anciens ordinateurs restent non vendus après la migration.
    public bool Vendu { get; set; } = false;
    public Caracteristiques Caracteristiques { get; private set; }

    // La liste inverse reste disponible pour EF sans créer de boucle dans le JSON.
    [JsonIgnore]
    public List<Reparation> Reparations { get; private set; } = new();

    private Ordinateur()
    {
        Nom = "";
        Caracteristiques = null!;
    }

    public Ordinateur(string nom, Caracteristiques caracteristiques)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nom);
        ArgumentNullException.ThrowIfNull(caracteristiques);
        Nom = nom;
        Caracteristiques = caracteristiques;
    }

    public void ModifierInformations(string nom, Caracteristiques caracteristiques)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nom);
        ArgumentNullException.ThrowIfNull(caracteristiques);
        Nom = nom;
        Caracteristiques = caracteristiques;
    }

    public void ModifierVente(int stock, decimal prix)
    {
        if (stock < 0) throw new ArgumentOutOfRangeException(nameof(stock));
        if (prix < 0) throw new ArgumentOutOfRangeException(nameof(prix));
        Stock = stock;
        Prix = prix;
    }

    public void ChangerEtat(EtatOrdinateur etat)
    {
        if (!Enum.IsDefined(etat)) throw new ArgumentException("L'état est invalide.", nameof(etat));
        Etat = etat;
    }
}
