using FluentAssertions;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace GestionOrdinateurs.Tests;

public class OrdinateurServiceTest : IDisposable
{
    private readonly string dossierLogs = Path.Combine(Path.GetTempPath(), "tp4-" + Guid.NewGuid());

    public void Dispose()
    {
        if (Directory.Exists(dossierLogs)) Directory.Delete(dossierLogs, recursive: true);
    }

    private ParcInformatiqueContext CreerContexte()
    {
        var options = new DbContextOptionsBuilder<ParcInformatiqueContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new ParcInformatiqueContext(options);
    }

    // Les contextes d'un test partagent sa base, mais les tests restent isolés.
    private sealed class FabriqueTest(DbContextOptions<ParcInformatiqueContext> options)
        : IDbContextFactory<ParcInformatiqueContext>
    {
        public ParcInformatiqueContext CreateDbContext() => new(options);
    }

    private static FabriqueTest CreerFabrique(ParcInformatiqueContext contexte)
    {
        var options = (DbContextOptions<ParcInformatiqueContext>)contexte.GetService<IDbContextOptions>();
        return new FabriqueTest(options);
    }

    private static Ordinateur Poste(string nom = "Bureau", string marque = "Dell",
        int stock = 1, decimal prix = 500)
    {
        var ordinateur = new Ordinateur(nom, new Caracteristiques(16, 512, TypeOrdinateur.Fixe, marque, "Intel Core i5"));
        ordinateur.ModifierVente(stock, prix);
        return ordinateur;
    }

    [Fact]
    public async Task Ajouter_PersisteLesInformations()
    {
        await using var contexte = CreerContexte();
        var service = new OrdinateurService(CreerFabrique(contexte));
        var ordinateur = await service.AjouterAsync(Poste(stock: 3, prix: 750.50m));
        var relu = await service.TrouverParIdAsync(ordinateur.Id);
        relu.Should().NotBeNull();
        relu!.Stock.Should().Be(3);
        relu.Prix.Should().Be(750.50m);
        (await service.ListerAsync()).Should().ContainSingle();
    }

    [Fact]
    public async Task Modifier_ConserveIdEtMetAJourPrixStock()
    {
        await using var contexte = CreerContexte();
        var service = new OrdinateurService(CreerFabrique(contexte));
        var ordinateur = await service.AjouterAsync(Poste());
        (await service.ModifierAsync(ordinateur.Id, "Nouveau", new Caracteristiques(32, 1024, TypeOrdinateur.Server, "HP", "Ryzen"),
            EtatOrdinateur.EnMaintenance, 0, 1000)).Should().BeTrue();
        var relu = await service.TrouverParIdAsync(ordinateur.Id);
        relu!.Nom.Should().Be("Nouveau");
        relu.Id.Should().Be(ordinateur.Id);
        relu.Stock.Should().Be(0);
        relu.Prix.Should().Be(1000);
        relu.Etat.Should().Be(EtatOrdinateur.EnMaintenance);
    }

    [Fact]
    public async Task Supprimer_ExistantPuisAbsent()
    {
        await using var contexte = CreerContexte();
        var service = new OrdinateurService(CreerFabrique(contexte));
        var ordinateur = await service.AjouterAsync(Poste());
        (await service.SupprimerAsync(ordinateur.Id)).Should().BeTrue();
        (await service.SupprimerAsync(ordinateur.Id)).Should().BeFalse();
        (await service.TrouverParIdAsync(ordinateur.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Modifier_AbsentNeCreePasDePoste()
    {
        await using var contexte = CreerContexte();
        var service = new OrdinateurService(CreerFabrique(contexte));
        (await service.ModifierAsync(-1, "Absent", new Caracteristiques(8, 256, TypeOrdinateur.Fixe, "HP", "CPU"),
            EtatOrdinateur.EnService, 1, 100)).Should().BeFalse();
        (await service.ListerAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Linq_FiltreStockMarqueEtTriePrix()
    {
        await using var contexte = CreerContexte();
        var service = new OrdinateurService(CreerFabrique(contexte));
        await service.AjouterAsync(Poste("A", "Dell", 0, 1200));
        await service.AjouterAsync(Poste("B", "HP", 2, 400));
        await service.AjouterAsync(Poste("C", "Dell", 1, 750));
        (await service.ListerDisponiblesAsync()).Select(o => o.Nom).Should().Equal("B", "C");
        (await service.RechercherParMarqueAsync("Dell")).Select(o => o.Nom).Should().Equal("A", "C");
        (await service.RechercherParMarqueAsync("Inconnue")).Should().BeEmpty();
        (await service.TrierParPrixAsync()).Select(o => o.Prix).Should().Equal(1200, 750, 400);
    }

    [Fact]
    public async Task Evenements_DeclenchesSeulementPourOperationsReussies()
    {
        await using var contexte = CreerContexte();
        var service = new OrdinateurService(CreerFabrique(contexte));
        var messages = new List<string>();
        service.OrdinateurAjoute += (_, _) => messages.Add("ajout");
        service.OrdinateurMisAJour += (_, _) => messages.Add("modification");
        service.OrdinateurSupprime += (_, _) => messages.Add("suppression");
        var ordinateur = await service.AjouterAsync(Poste());
        await service.ModifierAsync(ordinateur.Id, "Nouveau", new Caracteristiques(8, 256, TypeOrdinateur.Fixe, "HP", "CPU"),
            EtatOrdinateur.EnService, 2, 200);
        await service.SupprimerAsync(ordinateur.Id);
        await service.SupprimerAsync(ordinateur.Id);
        messages.Should().Equal("ajout", "modification", "suppression");
    }

    [Fact]
    public async Task Reparations_EnregistreCinqResultatsAvecLeurOrdinateur()
    {
        await using var contexte = CreerContexte();
        var fabrique = CreerFabrique(contexte);
        var service = new OrdinateurService(fabrique);
        var postes = new List<Ordinateur>();
        for (int i = 0; i < 5; i++)
            postes.Add(await service.AjouterAsync(Poste($"Poste {i}")));
        var reparations = new ReparationService(fabrique, NullLogger<ReparationService>.Instance, new JournalService(dossierLogs));
        await Task.WhenAll(postes.Select(o => reparations.SimulerReparationAsync(o, "Alice")));
        var resultats = await reparations.ListerAsync();
        resultats.Should().HaveCount(5);
        resultats.Should().OnlyContain(r => r.Statut == "Terminée" && r.UnOrdinateur != null);
        resultats.Select(r => r.OrdinateurId).Should().BeEquivalentTo(postes.Select(o => o.Id));
    }

    [Fact]
    public async Task Stock_DixConsommationsConcurrentesDonnentZero()
    {
        var stock = new StockService();
        stock.StockSSD.Should().Be(10);
        await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => stock.ConsommerSSD()));
        stock.StockSSD.Should().Be(0);
        await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => stock.ConsommerSSD()));
        stock.StockSSD.Should().Be(0);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(1, -10)]
    public void Vente_RefuseLesValeursNegatives(int stock, int prix)
    {
        var action = () => Poste(stock: stock, prix: prix);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task CreationReparation_HistoriqueEtDeconstruction()
    {
        await using var contexte = CreerContexte();
        var fabrique = CreerFabrique(contexte);
        var service = new OrdinateurService(fabrique);
        var ordinateur = await service.AjouterAsync(Poste("Atelier"));
        var reparations = new ReparationService(fabrique,
            NullLogger<ReparationService>.Instance, new JournalService(dossierLogs));
        await reparations.SimulerReparationAsync(ordinateur, "Camille");

        var historique = await reparations.HistoriqueAsync();
        var (id, nom, technicien, type, date) = historique.Should().ContainSingle().Subject;
        id.Should().Be(ordinateur.Id);
        nom.Should().Be("Atelier");
        technicien.Should().Be("Camille");
        type.Should().NotBeNullOrWhiteSpace();
        date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));

        // Le record compare les valeurs ; with crée une copie indépendante.
        var copie = historique[0] with { Technicien = "Alex" };
        copie.Technicien.Should().Be("Alex");
        historique[0].Technicien.Should().Be("Camille");
        File.ReadAllText(Path.Combine(dossierLogs, "Reparations", "reparations.log"))
            .Should().Contain("Atelier réparé par Camille");
    }

    [Theory]
    [InlineData(EtatOrdinateur.HorsService, 16, "Remplacement")]
    [InlineData(EtatOrdinateur.Lent, 4, "Ajouter RAM")]
    [InlineData(EtatOrdinateur.Lent, 8, "Aucune action")]
    [InlineData(EtatOrdinateur.EnService, 4, "Aucune action")]
    public void Diagnostic_RespecteEtatEtSeuilRam(EtatOrdinateur etat, int ram, string attendu)
    {
        using var contexte = CreerContexte();
        var service = new OrdinateurService(CreerFabrique(contexte));
        var ordinateur = new Ordinateur("Diagnostic",
            new Caracteristiques(ram, 256, TypeOrdinateur.Portable, "Dell", "CPU"));
        ordinateur.ChangerEtat(etat);
        service.Diagnostiquer(ordinateur).Should().Be(attendu);
    }

    [Fact]
    public async Task Statistiques_EtIndexeursUtilisentLeModeleSimplifie()
    {
        await using var contexte = CreerContexte();
        var service = new OrdinateurService(CreerFabrique(contexte));
        var premier = await service.AjouterAsync(Poste("Alpha", stock: 2, prix: 10.25m));
        await service.AjouterAsync(Poste("Beta", stock: 0, prix: 100));
        var (total, disponibles, valeur, reparations) = await service.CalculerStatistiquesAsync();
        total.Should().Be(2);
        disponibles.Should().Be(1);
        valeur.Should().Be(20.50m);
        reparations.Should().Be(0);
        var inventaire = new InventaireOrdinateurs { Ordinateurs = await service.ListerAsync() };
        inventaire[premier.Id]!.Nom.Should().Be("Alpha");
        inventaire["alpha"]!.Id.Should().Be(premier.Id);
        inventaire["Absent"].Should().BeNull();
        inventaire[-1].Should().BeNull();
    }

    [Fact]
    public async Task Journal_EcritToutesLesLignesConcurrentesEtLibereLeFichier()
    {
        var journal = new JournalService(dossierLogs);
        await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(i => Task.Run(() => journal.Ecrire("Poste " + i))));
        string chemin = Path.Combine(dossierLogs, "Reparations", "reparations.log");
        File.ReadAllLines(chemin).Should().HaveCount(20);
        using var fichier = new FileStream(chemin, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        fichier.Length.Should().BeGreaterThan(0);
    }
}
