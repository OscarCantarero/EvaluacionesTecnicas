using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechEval.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixValueGeneratedNever : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_puntuaciones_pregunta_resultados_evaluacion_ResultadoEvalua~",
                table: "puntuaciones_pregunta");

            migrationBuilder.AlterColumn<Guid>(
                name: "ResultadoEvaluacionId",
                table: "puntuaciones_pregunta",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_puntuaciones_pregunta_resultados_evaluacion_ResultadoEvalua~",
                table: "puntuaciones_pregunta",
                column: "ResultadoEvaluacionId",
                principalTable: "resultados_evaluacion",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_puntuaciones_pregunta_resultados_evaluacion_ResultadoEvalua~",
                table: "puntuaciones_pregunta");

            migrationBuilder.AlterColumn<Guid>(
                name: "ResultadoEvaluacionId",
                table: "puntuaciones_pregunta",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_puntuaciones_pregunta_resultados_evaluacion_ResultadoEvalua~",
                table: "puntuaciones_pregunta",
                column: "ResultadoEvaluacionId",
                principalTable: "resultados_evaluacion",
                principalColumn: "id");
        }
    }
}
