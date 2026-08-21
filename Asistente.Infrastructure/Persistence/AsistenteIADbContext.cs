using Asistente.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Persistence;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos AsistenteIA.
/// </summary>
public class AsistenteIADbContext : DbContext
{
    public AsistenteIADbContext(
        DbContextOptions<AsistenteIADbContext> options)
        : base(options)
    {
    }

    public DbSet<Conversacion> Conversaciones => Set<Conversacion>();

    public DbSet<Mensaje> Mensajes => Set<Mensaje>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();

    public DbSet<UsuarioRol> UsuariosRoles => Set<UsuarioRol>();

    public DbSet<AuditoriaSesion> AuditoriasSesion
        => Set<AuditoriaSesion>();

    public DbSet<AuditoriaActividad> AuditoriasActividad
        => Set<AuditoriaActividad>();


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

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");

            entity.HasKey(x => x.IdUsuario);

            entity.Property(x => x.IdUsuario)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.NombreUsuario)
                .HasColumnName("Usuario")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.NombreUsuario)
                .IsUnique();

            entity.Property(x => x.Nombres)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Apellidos)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Correo)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(x => x.Correo)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.Activo)
                .IsRequired();

            entity.Property(x => x.FechaCreacion)
                .IsRequired();

            entity.HasMany(x => x.UsuarioRoles)
                .WithOne(x => x.Usuario)
                .HasForeignKey(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Sesiones)
                .WithOne(x => x.Usuario)
                .HasForeignKey(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Actividades)
                .WithOne(x => x.Usuario)
                .HasForeignKey(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Rol");

            entity.HasKey(x => x.IdRol);

            entity.Property(x => x.IdRol)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.Nombre)
                .IsUnique();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.Activo)
                .IsRequired();

            entity.HasMany(x => x.UsuarioRoles)
                .WithOne(x => x.Rol)
                .HasForeignKey(x => x.IdRol)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.ToTable("UsuarioRol");

            entity.HasKey(x => new { x.IdUsuario, x.IdRol });
        });

        modelBuilder.Entity<AuditoriaSesion>(entity =>
        {
            entity.ToTable("AuditoriaSesion");

            entity.HasKey(x => x.IdSesion);

            entity.Property(x => x.IdSesion)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.FechaInicio)
                .IsRequired();

            entity.Property(x => x.DireccionIP)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Navegador)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        });

        modelBuilder.Entity<AuditoriaActividad>(entity =>
        {
            entity.ToTable("AuditoriaActividad");

            entity.HasKey(x => x.IdActividad);

            entity.Property(x => x.IdActividad)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.FechaHora)
                .IsRequired();

            entity.Property(x => x.Modulo)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Accion)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.DireccionIP)
                .HasMaxLength(50)
                .IsRequired();
        });
    }
}