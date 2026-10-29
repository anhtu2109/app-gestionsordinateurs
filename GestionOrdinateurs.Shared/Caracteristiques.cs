namespace GestionOrdinateurs.Shared;

public class Caracteristiques
{
    public int RamGo { get; private set; }
    public int StockageGo { get; private set; }
    public TypeOrdinateur TypeOrdi { get; private set; }
    public string Marque { get; private set; }
    public string Processeur { get; private set; }

    private Caracteristiques()
    {
        Marque = Processeur = "";
    }

    public Caracteristiques(int ramGo, int stockageGo, TypeOrdinateur typeOrdi,
        string marque, string processeur)
    {
        if (ramGo <= 0) throw new ArgumentOutOfRangeException(nameof(ramGo));
        if (stockageGo <= 0) throw new ArgumentOutOfRangeException(nameof(stockageGo));
        if (!Enum.IsDefined(typeOrdi)) throw new ArgumentException("Type invalide.", nameof(typeOrdi));
        ArgumentException.ThrowIfNullOrWhiteSpace(marque);
        ArgumentException.ThrowIfNullOrWhiteSpace(processeur);

        RamGo = ramGo;
        StockageGo = stockageGo;
        TypeOrdi = typeOrdi;
        Marque = marque;
        Processeur = processeur;
    }
}
