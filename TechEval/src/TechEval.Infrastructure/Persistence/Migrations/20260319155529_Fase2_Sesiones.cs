using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechEval.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fase2_Sesiones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "respuestas_candidato",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreguntaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Texto = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    UrlAdjunto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TiempoEmpleadoSegundos = table.Column<int>(type: "integer", nullable: false),
                    BrindadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FueExpiratoElTiempo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_respuestas_candidato", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sesiones_evaluacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidatoId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CodigoAcceso = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    CreadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IniciadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletadaEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContadorViolacionesPestana = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesiones_evaluacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "preguntas_sesion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreguntaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    LimiteTiempoSegundos = table.Column<int>(type: "integer", nullable: true),
                    RespuestaId = table.Column<Guid>(type: "uuid", nullable: true),
                    FueRespondida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TiempoExpirado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SesionEvaluacionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preguntas_sesion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preguntas_sesion_respuestas_candidato_RespuestaId",
                        column: x => x.RespuestaId,
                        principalTable: "respuestas_candidato",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_preguntas_sesion_sesiones_evaluacion_SesionEvaluacionId",
                        column: x => x.SesionEvaluacionId,
                        principalTable: "sesiones_evaluacion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_preguntas_sesion_PreguntaId",
                table: "preguntas_sesion",
                column: "PreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_preguntas_sesion_RespuestaId",
                table: "preguntas_sesion",
                column: "RespuestaId");

            migrationBuilder.CreateIndex(
                name: "IX_preguntas_sesion_SesionEvaluacionId",
                table: "preguntas_sesion",
                column: "SesionEvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_respuestas_candidato_PreguntaId",
                table: "respuestas_candidato",
                column: "PreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_evaluacion_CandidatoId",
                table: "sesiones_evaluacion",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_evaluacion_CodigoAcceso",
                table: "sesiones_evaluacion",
                column: "CodigoAcceso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_evaluacion_EvaluacionId",
                table: "sesiones_evaluacion",
                column: "EvaluacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "preguntas_sesion");

            migrationBuilder.DropTable(
                name: "respuestas_candidato");

            migrationBuilder.DropTable(
                name: "sesiones_evaluacion");
        }
    }
}
