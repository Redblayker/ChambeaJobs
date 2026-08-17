using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChambeaJobs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUltimoInicioSesion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoInicioSesion",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UltimoInicioSesion",
                table: "AspNetUsers");
        }
    }
}
