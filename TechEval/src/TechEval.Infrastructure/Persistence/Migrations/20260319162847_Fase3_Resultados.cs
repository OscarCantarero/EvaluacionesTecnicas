using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechEval.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fase3_Resultados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "resultados_evaluacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sesion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluacion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidato_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    nombre_candidato = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    titulo_evaluacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    fecha_completacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    puntuacion_total = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    puntuacion_maxima = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    porcentaje_obtenido = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    estado_revision = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio_revision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_fin_revision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    evaluador_revision = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    violaciones_pestana = table.Column<int>(type: "integer", nullable: false),
                    tiempo_total_segundos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resultados_evaluacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "puntuaciones_pregunta",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pregunta_sesion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_pregunta = table.Column<int>(type: "integer", nullable: false),
                    tipo_pregunta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    respuesta = table.Column<string>(type: "text", nullable: false),
                    puntuacion_automatica = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    puntuacion_manual = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    puntuacion_ia_sugerida = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    justificacion_ia = table.Column<string>(type: "text", nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    fue_expirada = table.Column<bool>(type: "boolean", nullable: false),
                    url_adjunto = table.Column<string>(type: "text", nullable: true),
                    tiempo_empleado_segundos = table.Column<int>(type: "integer", nullable: false),
                    ResultadoEvaluacionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_puntuaciones_pregunta", x => x.id);
                    table.ForeignKey(
                        name: "FK_puntuaciones_pregunta_resultados_evaluacion_ResultadoEvalua~",
                        column: x => x.ResultadoEvaluacionId,
                        principalTable: "resultados_evaluacion",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_puntuaciones_pregunta_pregunta_sesion_id",
                table: "puntuaciones_pregunta",
                column: "pregunta_sesion_id");

            migrationBuilder.CreateIndex(
                name: "IX_puntuaciones_pregunta_ResultadoEvaluacionId",
                table: "puntuaciones_pregunta",
                column: "ResultadoEvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_resultados_evaluacion_candidato_id",
                table: "resultados_evaluacion",
                column: "candidato_id");

            migrationBuilder.CreateIndex(
                name: "IX_resultados_evaluacion_estado_revision",
                table: "resultados_evaluacion",
                column: "estado_revision");

            migrationBuilder.CreateIndex(
                name: "IX_resultados_evaluacion_evaluacion_id",
                table: "resultados_evaluacion",
                column: "evaluacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_resultados_evaluacion_sesion_id",
                table: "resultados_evaluacion",
                column: "sesion_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "puntuaciones_pregunta");

            migrationBuilder.DropTable(
                name: "resultados_evaluacion");
        }
    }
}
