using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asistente.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarGestorDocumentalEtapa6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdDocumento",
                table: "AuditoriaActividad",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CategoriaDocumento",
                columns: table => new
                {
                    IdCategoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaDocumento", x => x.IdCategoria);
                });

            migrationBuilder.CreateTable(
                name: "Documento",
                columns: table => new
                {
                    IdDocumento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IdCategoria = table.Column<int>(type: "int", nullable: false),
                    VersionActual = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EstadoProcesamiento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioRegistro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documento", x => x.IdDocumento);
                    table.ForeignKey(
                        name: "FK_Documento_CategoriaDocumento_IdCategoria",
                        column: x => x.IdCategoria,
                        principalTable: "CategoriaDocumento",
                        principalColumn: "IdCategoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoVersion",
                columns: table => new
                {
                    IdVersion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdDocumento = table.Column<int>(type: "int", nullable: false),
                    NumeroVersion = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TamanoArchivo = table.Column<long>(type: "bigint", nullable: false),
                    HashArchivo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCarga = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoVersion", x => x.IdVersion);
                    table.ForeignKey(
                        name: "FK_DocumentoVersion_Documento_IdDocumento",
                        column: x => x.IdDocumento,
                        principalTable: "Documento",
                        principalColumn: "IdDocumento",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaActividad_IdDocumento_FechaHora",
                table: "AuditoriaActividad",
                columns: new[] { "IdDocumento", "FechaHora" });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaDocumento_Nombre",
                table: "CategoriaDocumento",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documento_Codigo",
                table: "Documento",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documento_IdCategoria_Estado_FechaRegistro",
                table: "Documento",
                columns: new[] { "IdCategoria", "Estado", "FechaRegistro" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoVersion_IdDocumento_Activo",
                table: "DocumentoVersion",
                columns: new[] { "IdDocumento", "Activo" },
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoVersion_IdDocumento_NumeroVersion",
                table: "DocumentoVersion",
                columns: new[] { "IdDocumento", "NumeroVersion" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditoriaActividad_Documento_IdDocumento",
                table: "AuditoriaActividad",
                column: "IdDocumento",
                principalTable: "Documento",
                principalColumn: "IdDocumento",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditoriaActividad_Documento_IdDocumento",
                table: "AuditoriaActividad");

            migrationBuilder.DropTable(
                name: "DocumentoVersion");

            migrationBuilder.DropTable(
                name: "Documento");

            migrationBuilder.DropTable(
                name: "CategoriaDocumento");

            migrationBuilder.DropIndex(
                name: "IX_AuditoriaActividad_IdDocumento_FechaHora",
                table: "AuditoriaActividad");

            migrationBuilder.DropColumn(
                name: "IdDocumento",
                table: "AuditoriaActividad");
        }
    }
}
