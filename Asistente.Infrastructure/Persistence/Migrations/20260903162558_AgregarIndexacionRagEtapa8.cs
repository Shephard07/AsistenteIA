using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asistente.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIndexacionRagEtapa8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentoIndexado",
                columns: table => new
                {
                    IdDocumentoIndexado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdDocumentoProcesado = table.Column<int>(type: "int", nullable: false),
                    IdentificadorVectorial = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaIndexacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TotalChunks = table.Column<int>(type: "int", nullable: false),
                    TotalEmbeddings = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoIndexado", x => x.IdDocumentoIndexado);
                    table.ForeignKey(
                        name: "FK_DocumentoIndexado_DocumentoProcesado_IdDocumentoProcesado",
                        column: x => x.IdDocumentoProcesado,
                        principalTable: "DocumentoProcesado",
                        principalColumn: "IdDocumentoProcesado",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmbeddingConfiguracion",
                columns: table => new
                {
                    IdConfiguracion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Proveedor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModeloEmbeddings = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BaseVectorial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CantidadResultados = table.Column<int>(type: "int", nullable: false),
                    PuntajeMinimo = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    LongitudMaximaContexto = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmbeddingConfiguracion", x => x.IdConfiguracion);
                });

            migrationBuilder.InsertData(
                table: "EmbeddingConfiguracion",
                columns: new[] { "IdConfiguracion", "Activo", "BaseVectorial", "CantidadResultados", "LongitudMaximaContexto", "ModeloEmbeddings", "Proveedor", "PuntajeMinimo" },
                values: new object[] { 1, true, "ChromaDB", 4, 6000, "nomic-embed-text", "Ollama", 0.35m });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoIndexado_Estado_FechaInicio",
                table: "DocumentoIndexado",
                columns: new[] { "Estado", "FechaInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoIndexado_IdDocumentoProcesado",
                table: "DocumentoIndexado",
                column: "IdDocumentoProcesado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoIndexado_IdentificadorVectorial",
                table: "DocumentoIndexado",
                column: "IdentificadorVectorial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmbeddingConfiguracion_Activo",
                table: "EmbeddingConfiguracion",
                column: "Activo",
                unique: true,
                filter: "[Activo] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentoIndexado");

            migrationBuilder.DropTable(
                name: "EmbeddingConfiguracion");
        }
    }
}
