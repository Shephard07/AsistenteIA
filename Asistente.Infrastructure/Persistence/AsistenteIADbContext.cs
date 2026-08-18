
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asistente.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Persistence;

/// Contexto de Entity Framework Core para la base de datos AsistenteIA.

public class AsistenteIADbContext : DbContext
{
    public AsistenteIADbContext(
        DbContextOptions<AsistenteIADbContext> options)
        : base(options)
    {
    }

    public DbSet<Conversacion> Conversaciones => Set<Conversacion>();

    public DbSet<Mensaje> Mensajes => Set<Mensaje>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Conversacion>(entity =>
        {
            entity.ToTable("Conversacion");

            entity.HasKey(x => x.IdConversacion);

            entity.Property(x => x.IdConversacion)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.FechaInicio)
                .IsRequired();

            entity.Property(x => x.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.HasMany(x => x.Mensajes)
                .WithOne(x => x.Conversacion)
                .HasForeignKey(x => x.IdConversacion)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Mensaje>(entity =>
        {
            entity.ToTable("Mensaje");

            entity.HasKey(x => x.IdMensaje);

            entity.Property(x => x.IdMensaje)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Rol)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Contenido)
                .IsRequired();

            entity.Property(x => x.FechaHora)
                .IsRequired();
        });
    }
}