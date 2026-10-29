namespace GestionOrdinateurs.Shared;

// Chaque réparation conserve le lien vers l'ordinateur déposé à l'atelier.
public class Reparation
{
    public int Id { get; set; }
    public int OrdinateurId { get; set; }
    public Ordinateur UnOrdinateur { get; set; } = null!;
    public string Technicien { get; set; } = "";
    public string TypeReparation { get; set; } = "";
    public DateTime DateDebut { get; set; }
    public string Statut { get; set; } = "En cours";
}
