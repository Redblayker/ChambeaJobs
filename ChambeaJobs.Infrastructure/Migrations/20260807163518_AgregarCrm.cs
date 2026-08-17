using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChambeaJobs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCrm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrmEmpresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: true),
                    NombreEmpresa = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Etapa = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ContactoPrincipal = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RUC = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SectorEmpresarial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoEmpresa = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SitioWeb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RedesSociales = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UsuarioCreadorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmEmpresas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmEmpresas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CrmActividades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrmEmpresaId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FechaActividad = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmActividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmActividades_CrmEmpresas_CrmEmpresaId",
                        column: x => x.CrmEmpresaId,
                        principalTable: "CrmEmpresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrmActividades_CrmEmpresaId_FechaActividad",
                table: "CrmActividades",
                columns: new[] { "CrmEmpresaId", "FechaActividad" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmEmpresas_EmpresaId",
                table: "CrmEmpresas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmEmpresas_Etapa",
                table: "CrmEmpresas",
                column: "Etapa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrmActividades");

            migrationBuilder.DropTable(
                name: "CrmEmpresas");
        }
    }
}
