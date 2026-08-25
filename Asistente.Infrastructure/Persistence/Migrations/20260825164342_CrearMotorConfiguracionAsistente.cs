using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asistente.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearMotorConfiguracionAsistente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdAsistente",
                table: "Conversacion",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Asistente",
                columns: table => new
                {
                    IdAsistente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ModeloIA = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Idioma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LongitudRespuesta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Formalidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FormatoRespuesta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Restricciones = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    MensajeBienvenida = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Temperatura = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    MaxTokens = table.Column<int>(type: "int", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistente", x => x.IdAsistente);
                });

            migrationBuilder.CreateTable(
                name: "PromptSistema",
                columns: table => new
                {
                    IdPrompt = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAsistente = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptSistema", x => x.IdPrompt);
                    table.ForeignKey(
                        name: "FK_PromptSistema_Asistente_IdAsistente",
                        column: x => x.IdAsistente,
                        principalTable: "Asistente",
                        principalColumn: "IdAsistente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialPrompt",
                columns: table => new
                {
                    IdHistorial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPrompt = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MotivoCambio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialPrompt", x => x.IdHistorial);
                    table.ForeignKey(
                        name: "FK_HistorialPrompt_PromptSistema_IdPrompt",
                        column: x => x.IdPrompt,
                        principalTable: "PromptSistema",
                        principalColumn: "IdPrompt",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversacion_IdAsistente",
                table: "Conversacion",
                column: "IdAsistente");

            migrationBuilder.CreateIndex(
                name: "IX_Asistente_Nombre",
                table: "Asistente",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistorialPrompt_IdPrompt",
                table: "HistorialPrompt",
                column: "IdPrompt");

            migrationBuilder.CreateIndex(
                name: "IX_PromptSistema_IdAsistente_Activo",
                table: "PromptSistema",
                columns: new[] { "IdAsistente", "Activo" },
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PromptSistema_IdAsistente_Version",
                table: "PromptSistema",
                columns: new[] { "IdAsistente", "Version" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversacion_Asistente_IdAsistente",
                table: "Conversacion",
                column: "IdAsistente",
                principalTable: "Asistente",
                principalColumn: "IdAsistente",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversacion_Asistente_IdAsistente",
                table: "Conversacion");

            migrationBuilder.DropTable(
                name: "HistorialPrompt");

            migrationBuilder.DropTable(
                name: "PromptSistema");

            migrationBuilder.DropTable(
                name: "Asistente");

            migrationBuilder.DropIndex(
                name: "IX_Conversacion_IdAsistente",
                table: "Conversacion");

            migrationBuilder.DropColumn(
                name: "IdAsistente",
                table: "Conversacion");
        }
    }
}
