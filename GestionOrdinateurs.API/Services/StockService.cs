namespace GestionOrdinateurs.API.Services;

public class StockService
{
    private readonly object verrou = new();
    private int stockSSD = 10;

    public int StockSSD
    {
        get { lock (verrou) return stockSSD; }
    }

    // Task permet au contrôleur d'attendre la fin, contrairement à async void.
    public async Task ConsommerSSD()
    {
        await Task.Delay(10);
        lock (verrou)
        {
            // Le test et la décrémentation forment une seule opération protégée.
            if (stockSSD > 0)
                stockSSD--;
        }
    }
}
