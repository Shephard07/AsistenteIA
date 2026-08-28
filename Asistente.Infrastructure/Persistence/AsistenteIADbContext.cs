//AsistenteIADbContext.cs
using Asistente.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AsistenteEntity = Asistente.Domain.Entities.Asistente;

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

    public DbSet<AsistenteEntity> Asistentes => Set<AsistenteEntity>();

    public DbSet<PromptSistema> PromptsSistema => Set<PromptSistema>();

    public DbSet<HistorialPrompt> HistorialesPrompt
        => Set<HistorialPrompt>();

    public DbSet<CategoriaDocumento> CategoriasDocumento
    => Set<CategoriaDocumento>();

    public DbSet<Documento> Documentos
        => Set<Documento>();

    public DbSet<DocumentoVersion> DocumentosVersiones
        => Set<DocumentoVersion>();

    public DbSet<ConfiguracionMemoria> ConfiguracionesMemoria
    => Set<ConfiguracionMemoria>();



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

            entity.HasOne(x => x.Asistente)
                .WithMany()
                .HasForeignKey(x => x.IdAsistente)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.Titulo)
                .HasMaxLength(200);

            entity.Property(x => x.FechaUltimaActividad)
                .IsRequired();

            entity.Property(x => x.ResumenContexto)
                .HasMaxLength(8000);

            entity.Property(x => x.TotalMensajes)
                .IsRequired();

            entity.Property(x => x.TotalMensajesResumidos)
                .IsRequired()
                .HasDefaultValue(0);

            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.Conversaciones)
                .HasForeignKey(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.IdUsuario,
                x.Estado,
                x.FechaUltimaActividad
            });
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
            entity.HasMany(x => x.Conversaciones)
                .WithOne(x => x.Usuario)
                .HasForeignKey(x => x.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
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

            entity.HasOne(x => x.Documento)
                .WithMany(x => x.ActividadesAuditoria)
                .HasForeignKey(x => x.IdDocumento)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => new
            {
                x.IdDocumento,
                x.FechaHora
            });
        });

        modelBuilder.Entity<AsistenteEntity>(entity =>
        {
            entity.ToTable("Asistente");

            entity.HasKey(x => x.IdAsistente);

            entity.Property(x => x.IdAsistente)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.Nombre)
                .IsUnique();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.ModeloIA)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Idioma)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.LongitudRespuesta)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Formalidad)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.FormatoRespuesta)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Restricciones)
                .HasMaxLength(4000)
                .IsRequired();

            entity.Property(x => x.MensajeBienvenida)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.Temperatura)
                .HasColumnType("decimal(4,2)")
                .IsRequired();

            entity.Property(x => x.MaxTokens)
                .IsRequired();

            entity.Property(x => x.TimeoutSeconds)
                .IsRequired();

            entity.Property(x => x.Activo)
                .IsRequired();

            entity.Property(x => x.FechaCreacion)
                .IsRequired();

            entity.HasMany(x => x.Prompts)
                .WithOne(x => x.Asistente)
                .HasForeignKey(x => x.IdAsistente)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PromptSistema>(entity =>
        {
            entity.ToTable("PromptSistema");

            entity.HasKey(x => x.IdPrompt);

            entity.Property(x => x.IdPrompt)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Contenido)
                .IsRequired();

            entity.Property(x => x.Version)
                .IsRequired();

            entity.Property(x => x.Activo)
                .IsRequired();

            entity.Property(x => x.FechaCreacion)
                .IsRequired();

            entity.Property(x => x.UsuarioCreacion)
                .HasMaxLength(50)
                .IsRequired();

            // Un asistente no puede repetir números de versión.
            entity.HasIndex(x => new { x.IdAsistente, x.Version })
                .IsUnique();

            // Un asistente solo puede tener un Prompt activo.
            entity.HasIndex(x => new { x.IdAsistente, x.Activo })
                .HasFilter("[Activo] = 1")
                .IsUnique();

            entity.HasMany(x => x.Historiales)
                .WithOne(x => x.PromptSistema)
                .HasForeignKey(x => x.IdPrompt)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HistorialPrompt>(entity =>
        {
            entity.ToTable("HistorialPrompt");

            entity.HasKey(x => x.IdHistorial);

            entity.Property(x => x.IdHistorial)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Version)
                .IsRequired();

            entity.Property(x => x.Contenido)
                .IsRequired();

            entity.Property(x => x.FechaModificacion)
                .IsRequired();

            entity.Property(x => x.UsuarioModificacion)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.MotivoCambio)
                .HasMaxLength(500)
                .IsRequired();
        });

        modelBuilder.Entity<CategoriaDocumento>(entity =>
        {
            entity.ToTable("CategoriaDocumento");

            entity.HasKey(x => x.IdCategoria);

            entity.Property(x => x.IdCategoria)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.Nombre)
                .IsUnique();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.Activo)
                .IsRequired();

            entity.Property(x => x.FechaCreacion)
                .IsRequired();

            entity.HasMany(x => x.Documentos)
                .WithOne(x => x.Categoria)
                .HasForeignKey(x => x.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.ToTable("Documento");

            entity.HasKey(x => x.IdDocumento);

            entity.Property(x => x.IdDocumento)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.Codigo)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.Codigo)
                .IsUnique();

            entity.Property(x => x.Nombre)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.VersionActual)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(x => x.Estado)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.EstadoProcesamiento)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.FechaRegistro)
                .IsRequired();

            entity.Property(x => x.UsuarioRegistro)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasMany(x => x.Versiones)
                .WithOne(x => x.Documento)
                .HasForeignKey(x => x.IdDocumento)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.IdCategoria,
                x.Estado,
                x.FechaRegistro
            });
        });

        modelBuilder.Entity<DocumentoVersion>(entity =>
        {
            entity.ToTable("DocumentoVersion");

            entity.HasKey(x => x.IdVersion);

            entity.Property(x => x.IdVersion)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.NumeroVersion)
                .IsRequired();

            entity.Property(x => x.NombreArchivo)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(x => x.RutaArchivo)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.TamanoArchivo)
                .IsRequired();

            entity.Property(x => x.HashArchivo)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.FechaCarga)
                .IsRequired();

            entity.Property(x => x.UsuarioCarga)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Activo)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.IdDocumento,
                x.NumeroVersion
            })
            .IsUnique();

            entity.HasIndex(x => new
            {
                x.IdDocumento,
                x.Activo
            })
            .HasFilter("[Activo] = 1")
            .IsUnique();
        });

        modelBuilder.Entity<ConfiguracionMemoria>(entity =>
        {
            entity.ToTable("ConfiguracionMemoria");

            entity.HasKey(x => x.IdConfiguracion);

            entity.Property(x => x.IdConfiguracion)
                .ValueGeneratedOnAdd();

            entity.Property(x => x.MaximoMensajesContexto)
                .IsRequired();

            entity.Property(x => x.MaximoTokensContexto)
                .IsRequired();

            entity.Property(x => x.LongitudResumen)
                .IsRequired();

            entity.Property(x => x.CantidadConversacionesVisibles)
                .IsRequired();

            entity.Property(x => x.Activo)
                .IsRequired();

            // Solo puede existir una configuración de memoria activa.
            entity.HasIndex(x => x.Activo)
                .HasFilter("[Activo] = 1")
                .IsUnique();

            entity.HasData(new
            {
                IdConfiguracion = 1,
                MaximoMensajesContexto = 10,
                MaximoTokensContexto = 3000,
                LongitudResumen = 800,
                CantidadConversacionesVisibles = 20,
                Activo = true
            });
        });
    }
}