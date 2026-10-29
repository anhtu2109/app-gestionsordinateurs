using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Data;

public class ParcInformatiqueContext(DbContextOptions<ParcInformatiqueContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Reparation> Reparations => Set<Reparation>();
    public DbSet<Ordinateur> Ordinateurs => Set<Ordinateur>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlite("Data Source=parcinfo.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Partie 1 : l'index unique protège aussi contre deux insertions simultanées.
        modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();
        modelBuilder.Entity<Ordinateur>().Property(o => o.Vendu).HasDefaultValue(false);

        var ordinateur = modelBuilder.Entity<Ordinateur>();
        ordinateur.HasMany(o => o.Reparations)
            .WithOne(r => r.UnOrdinateur).HasForeignKey(r => r.OrdinateurId)
            .OnDelete(DeleteBehavior.Cascade);

        // Réutiliser les colonnes du TP3 conserve les caractéristiques existantes.
        ordinateur.Property(o => o.Etat).HasColumnName("EtatOrdinateur");
        ordinateur.OwnsOne(o => o.Caracteristiques, caracteristiques =>
        {
            caracteristiques.Property(c => c.RamGo).HasColumnName("RAM");
            caracteristiques.Property(c => c.Marque).HasColumnName("Marque");
            caracteristiques.Property(c => c.Processeur).HasColumnName("Processeur");
            caracteristiques.Property(c => c.StockageGo).HasDefaultValue(256);
        });
        ordinateur.Navigation(o => o.Caracteristiques).IsRequired();
        ordinateur.Property(o => o.Prix).HasConversion<double>();
        ordinateur.Property(o => o.Stock).HasDefaultValue(0);
    }
}
