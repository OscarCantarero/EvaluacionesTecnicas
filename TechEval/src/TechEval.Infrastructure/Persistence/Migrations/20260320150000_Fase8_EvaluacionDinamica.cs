using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechEval.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fase8_EvaluacionDinamica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModoSeleccionPreguntas",
                table: "evaluaciones",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Fijas");

            migrationBuilder.AddColumn<int>(
                name: "CantidadPreguntasSesion",
                table: "evaluaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "distribucion_facil",
                table: "evaluaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "distribucion_medio",
                table: "evaluaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "distribucion_dificil",
                table: "evaluaciones",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ModoSeleccionPreguntas", table: "evaluaciones");
            migrationBuilder.DropColumn(name: "CantidadPreguntasSesion", table: "evaluaciones");
            migrationBuilder.DropColumn(name: "distribucion_facil", table: "evaluaciones");
            migrationBuilder.DropColumn(name: "distribucion_medio", table: "evaluaciones");
            migrationBuilder.DropColumn(name: "distribucion_dificil", table: "evaluaciones");
        }
    }
}
