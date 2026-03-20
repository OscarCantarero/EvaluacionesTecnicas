using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechEval.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixSesionPreguntasRelacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_preguntas_sesion_sesiones_evaluacion_SesionEvaluacionId",
                table: "preguntas_sesion");

            migrationBuilder.AddForeignKey(
                name: "FK_preguntas_sesion_sesiones_evaluacion_SesionEvaluacionId",
                table: "preguntas_sesion",
                column: "SesionEvaluacionId",
                principalTable: "sesiones_evaluacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_preguntas_sesion_sesiones_evaluacion_SesionEvaluacionId",
                table: "preguntas_sesion");

            migrationBuilder.AddForeignKey(
                name: "FK_preguntas_sesion_sesiones_evaluacion_SesionEvaluacionId",
                table: "preguntas_sesion",
                column: "SesionEvaluacionId",
                principalTable: "sesiones_evaluacion",
                principalColumn: "Id");
        }
    }
}
