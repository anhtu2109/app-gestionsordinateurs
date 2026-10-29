using System.ComponentModel.DataAnnotations;
using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Models;

// La forme reçue suit le modèle simplifié du TP4.
public class OrdinateurRequest
{
    [Required]
    public string Nom { get; set; } = "";

    [EnumDataType(typeof(EtatOrdinateur))]
    public EtatOrdinateur Etat { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal Prix { get; set; }

    [Required]
    public CaracteristiquesRequest Caracteristiques { get; set; } = null!;
}
