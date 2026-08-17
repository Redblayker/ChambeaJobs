using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChambeaJobs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModuloFinanzasV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AjustesFinancieros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoEntidad = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EntidadId = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MontoAnterior = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoNuevo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AjustesFinancieros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditoriasFinancieras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RegistroAfectado = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ValorNuevo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DireccionIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Resultado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriasFinancieras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasFinancieras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasFinancieras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeriodosFinancieros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cerrado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CerradoPorUsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosFinancieros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GastosFinancieros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaFinancieraId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NumeroReferencia = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ArchivoComprobanteId = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PeriodoFinancieroId = table.Column<int>(type: "int", nullable: false),
                    RegistradoPorUsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnuladoPorUsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GastosFinancieros", x => x.Id);
                    table.CheckConstraint("CK_GastoFinanciero_Monto", "[Monto] > 0");
                    table.ForeignKey(
                        name: "FK_GastosFinancieros_Archivos_ArchivoComprobanteId",
                        column: x => x.ArchivoComprobanteId,
                        principalTable: "Archivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GastosFinancieros_CategoriasFinancieras_CategoriaFinancieraId",
                        column: x => x.CategoriaFinancieraId,
                        principalTable: "CategoriasFinancieras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GastosFinancieros_PeriodosFinancieros_PeriodoFinancieroId",
                        column: x => x.PeriodoFinancieroId,
                        principalTable: "PeriodosFinancieros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IngresosFinancieros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PagoId = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    PlanSuscripcionId = table.Column<int>(type: "int", nullable: true),
                    CategoriaFinancieraId = table.Column<int>(type: "int", nullable: false),
                    PeriodoFinancieroId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    GeneradoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnuladoPorUsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngresosFinancieros", x => x.Id);
                    table.CheckConstraint("CK_IngresoFinanciero_Monto", "[Monto] > 0");
                    table.ForeignKey(
                        name: "FK_IngresosFinancieros_CategoriasFinancieras_CategoriaFinancieraId",
                        column: x => x.CategoriaFinancieraId,
                        principalTable: "CategoriasFinancieras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IngresosFinancieros_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IngresosFinancieros_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IngresosFinancieros_PeriodosFinancieros_PeriodoFinancieroId",
                        column: x => x.PeriodoFinancieroId,
                        principalTable: "PeriodosFinancieros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IngresosFinancieros_PlanesSuscripcion_PlanSuscripcionId",
                        column: x => x.PlanSuscripcionId,
                        principalTable: "PlanesSuscripcion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AjustesFinancieros_TipoEntidad_EntidadId",
                table: "AjustesFinancieros",
                columns: new[] { "TipoEntidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriasFinancieras_Modulo_FechaHora",
                table: "AuditoriasFinancieras",
                columns: new[] { "Modulo", "FechaHora" });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasFinancieras_Tipo_Nombre",
                table: "CategoriasFinancieras",
                columns: new[] { "Tipo", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GastosFinancieros_ArchivoComprobanteId",
                table: "GastosFinancieros",
                column: "ArchivoComprobanteId");

            migrationBuilder.CreateIndex(
                name: "IX_GastosFinancieros_CategoriaFinancieraId",
                table: "GastosFinancieros",
                column: "CategoriaFinancieraId");

            migrationBuilder.CreateIndex(
                name: "IX_GastosFinancieros_PeriodoFinancieroId_Estado",
                table: "GastosFinancieros",
                columns: new[] { "PeriodoFinancieroId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_IngresosFinancieros_CategoriaFinancieraId",
                table: "IngresosFinancieros",
                column: "CategoriaFinancieraId");

            migrationBuilder.CreateIndex(
                name: "IX_IngresosFinancieros_EmpresaId",
                table: "IngresosFinancieros",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_IngresosFinancieros_PagoId",
                table: "IngresosFinancieros",
                column: "PagoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IngresosFinancieros_PeriodoFinancieroId_Estado",
                table: "IngresosFinancieros",
                columns: new[] { "PeriodoFinancieroId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_IngresosFinancieros_PlanSuscripcionId",
                table: "IngresosFinancieros",
                column: "PlanSuscripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosFinancieros_Tipo_Anio_Mes",
                table: "PeriodosFinancieros",
                columns: new[] { "Tipo", "Anio", "Mes" },
                unique: true,
                filter: "[Mes] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AjustesFinancieros");

            migrationBuilder.DropTable(
                name: "AuditoriasFinancieras");

            migrationBuilder.DropTable(
                name: "GastosFinancieros");

            migrationBuilder.DropTable(
                name: "IngresosFinancieros");

            migrationBuilder.DropTable(
                name: "CategoriasFinancieras");

            migrationBuilder.DropTable(
                name: "PeriodosFinancieros");
        }
    }
}
