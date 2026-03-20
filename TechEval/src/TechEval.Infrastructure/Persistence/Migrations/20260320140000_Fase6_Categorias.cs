using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechEval.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fase6_Categorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreadoPor = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.Id);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "categoria_id",
                table: "preguntas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_preguntas_categoria_id",
                table: "preguntas",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_categorias_Nombre",
                table: "categorias",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_preguntas_categorias_categoria_id",
                table: "preguntas",
                column: "categoria_id",
                principalTable: "categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_preguntas_categorias_categoria_id",
                table: "preguntas");

            migrationBuilder.DropIndex(
                name: "IX_preguntas_categoria_id",
                table: "preguntas");

            migrationBuilder.DropIndex(
                name: "IX_categorias_Nombre",
                table: "categorias");

            migrationBuilder.DropColumn(
                name: "categoria_id",
                table: "preguntas");

            migrationBuilder.DropTable(
                name: "categorias");
        }
    }
}
