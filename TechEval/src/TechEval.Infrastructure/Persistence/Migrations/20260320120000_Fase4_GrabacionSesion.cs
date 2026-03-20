using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechEval.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fase4_GrabacionSesion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UrlGrabacionSesion",
                table: "sesiones_evaluacion",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlGrabacionAudio",
                table: "sesiones_evaluacion",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxViolacionesPestana",
                table: "sesiones_evaluacion",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlGrabacionSesion",
                table: "sesiones_evaluacion");

            migrationBuilder.DropColumn(
                name: "UrlGrabacionAudio",
                table: "sesiones_evaluacion");

            migrationBuilder.DropColumn(
                name: "MaxViolacionesPestana",
                table: "sesiones_evaluacion");
        }
    }
}
