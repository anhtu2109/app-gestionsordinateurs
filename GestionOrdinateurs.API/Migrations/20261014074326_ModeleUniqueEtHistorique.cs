using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionOrdinateurs.API.Migrations
{
    /// <inheritdoc />
    public partial class ModeleUniqueEtHistorique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Caracteristiques_TypeOrdi",
                table: "Ordinateurs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Convertir l'ancien discriminant avant de supprimer l'héritage.
            // Les postes Standard du TP3 sont considérés comme des postes fixes.
            migrationBuilder.Sql("""
                UPDATE Ordinateurs
                SET Caracteristiques_TypeOrdi =
                    CASE WHEN TypeOrdinateur = 'Portable' THEN 0 ELSE 1 END;
                """);

            migrationBuilder.DropColumn(
                name: "EcranConnecte",
                table: "Ordinateurs");

            migrationBuilder.DropColumn(
                name: "NiveauBatterie",
                table: "Ordinateurs");

            migrationBuilder.DropColumn(
                name: "TypeOrdinateur",
                table: "Ordinateurs");

            migrationBuilder.AddColumn<string>(
                name: "Technicien",
                table: "Reparations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Stock",
                table: "Ordinateurs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Caracteristiques_StockageGo",
                table: "Ordinateurs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 256);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Technicien",
                table: "Reparations");

            migrationBuilder.DropColumn(
                name: "Caracteristiques_StockageGo",
                table: "Ordinateurs");

            migrationBuilder.DropColumn(
                name: "Caracteristiques_TypeOrdi",
                table: "Ordinateurs");

            migrationBuilder.AlterColumn<int>(
                name: "Stock",
                table: "Ordinateurs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EcranConnecte",
                table: "Ordinateurs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NiveauBatterie",
                table: "Ordinateurs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeOrdinateur",
                table: "Ordinateurs",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");
        }
    }
}
