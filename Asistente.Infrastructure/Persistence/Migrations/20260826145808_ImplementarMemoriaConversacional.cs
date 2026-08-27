using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asistente.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImplementarMemoriaConversacional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimaActividad",
                table: "Conversacion",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IdUsuario",
                table: "Conversacion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumenContexto",
                table: "Conversacion",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "Conversacion",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalMensajes",
                table: "Conversacion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE conversacion
                SET
                    TotalMensajes = ISNULL(datos.TotalMensajes, 0),
                    FechaUltimaActividad = COALESCE(
                        datos.FechaUltimoMensaje,
                        conversacion.FechaFin,
                        conversacion.FechaInicio)
                FROM Conversacion AS conversacion
                OUTER APPLY
                (
                    SELECT
                        COUNT(*) AS TotalMensajes,
                        MAX(mensaje.FechaHora) AS FechaUltimoMensaje
                    FROM Mensaje AS mensaje
                    WHERE mensaje.IdConversacion = conversacion.IdConversacion
                ) AS datos;
                """);

            migrationBuilder.CreateTable(
                name: "ConfiguracionMemoria",
                columns: table => new
                {
                    IdConfiguracion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaximoMensajesContexto = table.Column<int>(type: "int", nullable: false),
                    MaximoTokensContexto = table.Column<int>(type: "int", nullable: false),
                    LongitudResumen = table.Column<int>(type: "int", nullable: false),
                    CantidadConversacionesVisibles = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionMemoria", x => x.IdConfiguracion);
                });

            migrationBuilder.InsertData(
                table: "ConfiguracionMemoria",
                columns: new[] { "IdConfiguracion", "Activo", "CantidadConversacionesVisibles", "LongitudResumen", "MaximoMensajesContexto", "MaximoTokensContexto" },
                values: new object[] { 1, true, 20, 800, 10, 3000 });

            migrationBuilder.CreateIndex(
                name: "IX_Conversacion_IdUsuario_Estado_FechaUltimaActividad",
                table: "Conversacion",
                columns: new[] { "IdUsuario", "Estado", "FechaUltimaActividad" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionMemoria_Activo",
                table: "ConfiguracionMemoria",
                column: "Activo",
                unique: true,
                filter: "[Activo] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversacion_Usuario_IdUsuario",
                table: "Conversacion",
                column: "IdUsuario",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversacion_Usuario_IdUsuario",
                table: "Conversacion");

            migrationBuilder.DropTable(
                name: "ConfiguracionMemoria");

            migrationBuilder.DropIndex(
                name: "IX_Conversacion_IdUsuario_Estado_FechaUltimaActividad",
                table: "Conversacion");

            migrationBuilder.DropColumn(
                name: "FechaUltimaActividad",
                table: "Conversacion");

            migrationBuilder.DropColumn(
                name: "IdUsuario",
                table: "Conversacion");

            migrationBuilder.DropColumn(
                name: "ResumenContexto",
                table: "Conversacion");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "Conversacion");

            migrationBuilder.DropColumn(
                name: "TotalMensajes",
                table: "Conversacion");
        }
    }
}
