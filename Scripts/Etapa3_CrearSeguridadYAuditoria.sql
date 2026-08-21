BEGIN TRANSACTION;
GO

CREATE TABLE [Rol] (
    [IdRol] int NOT NULL IDENTITY,
    [Nombre] nvarchar(50) NOT NULL,
    [Descripcion] nvarchar(250) NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_Rol] PRIMARY KEY ([IdRol])
);
GO

CREATE TABLE [Usuario] (
    [IdUsuario] int NOT NULL IDENTITY,
    [Usuario] nvarchar(50) NOT NULL,
    [Nombres] nvarchar(100) NOT NULL,
    [Apellidos] nvarchar(100) NOT NULL,
    [Correo] nvarchar(150) NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [Activo] bit NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    [FechaUltimoAcceso] datetime2 NULL,
    CONSTRAINT [PK_Usuario] PRIMARY KEY ([IdUsuario])
);
GO

CREATE TABLE [AuditoriaActividad] (
    [IdActividad] int NOT NULL IDENTITY,
    [IdUsuario] int NULL,
    [FechaHora] datetime2 NOT NULL,
    [Modulo] nvarchar(100) NOT NULL,
    [Accion] nvarchar(100) NOT NULL,
    [Descripcion] nvarchar(1000) NOT NULL,
    [DireccionIP] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_AuditoriaActividad] PRIMARY KEY ([IdActividad]),
    CONSTRAINT [FK_AuditoriaActividad_Usuario_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuario] ([IdUsuario]) ON DELETE SET NULL
);
GO

CREATE TABLE [AuditoriaSesion] (
    [IdSesion] int NOT NULL IDENTITY,
    [IdUsuario] int NOT NULL,
    [FechaInicio] datetime2 NOT NULL,
    [FechaFin] datetime2 NULL,
    [DireccionIP] nvarchar(50) NOT NULL,
    [Navegador] nvarchar(500) NOT NULL,
    [Estado] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_AuditoriaSesion] PRIMARY KEY ([IdSesion]),
    CONSTRAINT [FK_AuditoriaSesion_Usuario_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuario] ([IdUsuario]) ON DELETE NO ACTION
);
GO

CREATE TABLE [UsuarioRol] (
    [IdUsuario] int NOT NULL,
    [IdRol] int NOT NULL,
    CONSTRAINT [PK_UsuarioRol] PRIMARY KEY ([IdUsuario], [IdRol]),
    CONSTRAINT [FK_UsuarioRol_Rol_IdRol] FOREIGN KEY ([IdRol]) REFERENCES [Rol] ([IdRol]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UsuarioRol_Usuario_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuario] ([IdUsuario]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_AuditoriaActividad_IdUsuario] ON [AuditoriaActividad] ([IdUsuario]);
GO

CREATE INDEX [IX_AuditoriaSesion_IdUsuario] ON [AuditoriaSesion] ([IdUsuario]);
GO

CREATE UNIQUE INDEX [IX_Rol_Nombre] ON [Rol] ([Nombre]);
GO

CREATE UNIQUE INDEX [IX_Usuario_Correo] ON [Usuario] ([Correo]);
GO

CREATE UNIQUE INDEX [IX_Usuario_Usuario] ON [Usuario] ([Usuario]);
GO

CREATE INDEX [IX_UsuarioRol_IdRol] ON [UsuarioRol] ([IdRol]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260821153849_CrearSeguridadYAuditoria', N'8.0.1');
GO

COMMIT;
GO

