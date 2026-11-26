using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiPaisesMVC_SQLServer.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Universidades_Nombre_Pais",
                table: "Universidades",
                columns: new[] { "Nombre", "Pais" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Paises_Nombre",
                table: "Paises",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Universidades_Nombre_Pais",
                table: "Universidades");

            migrationBuilder.DropIndex(
                name: "IX_Paises_Nombre",
                table: "Paises");
        }
    }
}
