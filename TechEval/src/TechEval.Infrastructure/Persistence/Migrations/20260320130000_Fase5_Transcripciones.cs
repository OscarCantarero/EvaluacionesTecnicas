using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechEval.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fase5_Transcripciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transcripciones_evaluacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Contenido = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: true),
                    UrlArchivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PuntajeIA = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    JustificacionIA = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    CreadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResultadoEvaluacionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transcripciones_evaluacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transcripciones_evaluacion_resultados_evaluacion_ResultadoEvaluacionId",
                        column: x => x.ResultadoEvaluacionId,
                        principalTable: "resultados_evaluacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transcripciones_evaluacion_ResultadoEvaluacionId",
                table: "transcripciones_evaluacion",
                column: "ResultadoEvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_transcripciones_evaluacion_Tipo",
                table: "transcripciones_evaluacion",
                column: "Tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transcripciones_evaluacion");
        }
    }
}
