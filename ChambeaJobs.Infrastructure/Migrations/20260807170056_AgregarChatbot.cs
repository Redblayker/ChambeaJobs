using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChambeaJobs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarChatbot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatConversaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RolUsuario = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TicketSoporteId = table.Column<int>(type: "int", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatConversaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrmArchivosAdjuntos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrmEmpresaId = table.Column<int>(type: "int", nullable: false),
                    NombreOriginal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmArchivosAdjuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmArchivosAdjuntos_CrmEmpresas_CrmEmpresaId",
                        column: x => x.CrmEmpresaId,
                        principalTable: "CrmEmpresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMensajes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversacionId = table.Column<int>(type: "int", nullable: false),
                    Emisor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UrlAccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMensajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMensajes_ChatConversaciones_ConversacionId",
                        column: x => x.ConversacionId,
                        principalTable: "ChatConversaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversaciones_UsuarioId_Estado",
                table: "ChatConversaciones",
                columns: new[] { "UsuarioId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMensajes_ConversacionId_FechaEnvio",
                table: "ChatMensajes",
                columns: new[] { "ConversacionId", "FechaEnvio" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmArchivosAdjuntos_CrmEmpresaId",
                table: "CrmArchivosAdjuntos",
                column: "CrmEmpresaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMensajes");

            migrationBuilder.DropTable(
                name: "CrmArchivosAdjuntos");

            migrationBuilder.DropTable(
                name: "ChatConversaciones");
        }
    }
}
