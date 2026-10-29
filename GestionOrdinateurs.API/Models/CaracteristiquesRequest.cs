using System.ComponentModel.DataAnnotations;
using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Models;

public class CaracteristiquesRequest
{
    [Range(1, int.MaxValue)]
    public int RamGo { get; set; }

    [Range(1, int.MaxValue)]
    public int StockageGo { get; set; }

    [EnumDataType(typeof(TypeOrdinateur))]
    public TypeOrdinateur TypeOrdi { get; set; }

    [Required]
    public string Marque { get; set; } = "";

    [Required]
    public string Processeur { get; set; } = "";

    public Caracteristiques VersModele()
        => new(RamGo, StockageGo, TypeOrdi, Marque, Processeur);
}
