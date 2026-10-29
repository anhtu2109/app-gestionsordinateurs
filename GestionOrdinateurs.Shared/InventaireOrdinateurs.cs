namespace GestionOrdinateurs.Shared;

public class InventaireOrdinateurs
{
    public List<Ordinateur> Ordinateurs { get; set; } = new();

    // L'entier correspond à l'Id métier, pas à la position dans la liste.
    public Ordinateur? this[int ordinateurId]
        => Ordinateurs.FirstOrDefault(o => o.Id == ordinateurId);

    public Ordinateur? this[string reference]
        => Ordinateurs.FirstOrDefault(o => string.Equals(o.Nom, reference,
            StringComparison.OrdinalIgnoreCase));
}
