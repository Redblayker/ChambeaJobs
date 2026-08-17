using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChambeaJobs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDatosContactoEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreContacto",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectorEmpresarial",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoContacto",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreContacto",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "SectorEmpresarial",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "TelefonoContacto",
                table: "Empresas");
        }
    }
}
