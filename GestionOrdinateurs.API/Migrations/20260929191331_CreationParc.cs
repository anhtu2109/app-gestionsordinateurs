using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionOrdinateurs.API.Migrations
{
    /// <inheritdoc />
    public partial class CreationParc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ordinateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    Marque = table.Column<string>(type: "TEXT", nullable: false),
                    RAM = table.Column<int>(type: "INTEGER", nullable: false),
                    Processeur = table.Column<string>(type: "TEXT", nullable: false),
                    EtatOrdinateur = table.Column<int>(type: "INTEGER", nullable: false),
                    TypeOrdinateur = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    EcranConnecte = table.Column<bool>(type: "INTEGER", nullable: true),
                    NiveauBatterie = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordinateurs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ordinateurs");
        }
    }
}
