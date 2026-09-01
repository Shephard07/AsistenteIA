using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asistente.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProcesamientoDocumentalEtapa7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentoProcesado",
                columns: table => new
                {
                    IdDocumentoProcesado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVersionDocumento = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalPaginas = table.Column<int>(type: "int", nullable: false),
                    TotalCaracteres = table.Column<int>(type: "int", nullable: false),
                    TotalChunks = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoProcesado", x => x.IdDocumentoProcesado);
                    table.ForeignKey(
                        name: "FK_DocumentoProcesado_DocumentoVersion_IdVersionDocumento",
                        column: x => x.IdVersionDocumento,
                        principalTable: "DocumentoVersion",
                        principalColumn: "IdVersion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoChunk",
                columns: table => new
                {
                    IdChunk = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdDocumentoProcesado = table.Column<int>(type: "int", nullable: false),
                    IdDocumento = table.Column<int>(type: "int", nullable: false),
                    IdVersionDocumento = table.Column<int>(type: "int", nullable: false),
                    IdCategoria = table.Column<int>(type: "int", nullable: false),
                    NumeroChunk = table.Column<int>(type: "int", nullable: false),
                    PaginaInicial = table.Column<int>(type: "int", nullable: false),
                    PaginaFinal = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalCaracteres = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoChunk", x => x.IdChunk);
                    table.ForeignKey(
                        name: "FK_DocumentoChunk_DocumentoProcesado_IdDocumentoProcesado",
                        column: x => x.IdDocumentoProcesado,
                        principalTable: "DocumentoProcesado",
                        principalColumn: "IdDocumentoProcesado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoChunk_IdDocumento_IdCategoria",
                table: "DocumentoChunk",
                columns: new[] { "IdDocumento", "IdCategoria" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoChunk_IdDocumentoProcesado_NumeroChunk",
                table: "DocumentoChunk",
                columns: new[] { "IdDocumentoProcesado", "NumeroChunk" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoChunk_IdDocumentoProcesado_Orden",
                table: "DocumentoChunk",
                columns: new[] { "IdDocumentoProcesado", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoChunk_IdVersionDocumento",
                table: "DocumentoChunk",
                column: "IdVersionDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoProcesado_Estado_FechaInicio",
                table: "DocumentoProcesado",
                columns: new[] { "Estado", "FechaInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoProcesado_IdVersionDocumento",
                table: "DocumentoProcesado",
                column: "IdVersionDocumento",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentoChunk");

            migrationBuilder.DropTable(
                name: "DocumentoProcesado");
        }
    }
}
