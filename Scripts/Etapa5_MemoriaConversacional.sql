IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260818181822_CrearEstructuraInicial'
)
BEGIN
    CREATE TABLE [Conversacion] (
        [IdConversacion] int NOT NULL IDENTITY,
        [FechaInicio] datetime2 NOT NULL,
        [FechaFin] datetime2 NULL,
        [Estado] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_Conversacion] PRIMARY KEY ([IdConversacion])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260818181822_CrearEstructuraInicial'
)
BEGIN
    CREATE TABLE [Mensaje] (
        [IdMensaje] int NOT NULL IDENTITY,
        [IdConversacion] int NOT NULL,
        [Rol] nvarchar(20) NOT NULL,
        [Contenido] nvarchar(max) NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        [TiempoRespuestaMs] int NULL,
        CONSTRAINT [PK_Mensaje] PRIMARY KEY ([IdMensaje]),
        CONSTRAINT [FK_Mensaje_Conversacion_IdConversacion] FOREIGN KEY ([IdConversacion]) REFERENCES [Conversacion] ([IdConversacion]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260818181822_CrearEstructuraInicial'
)
BEGIN
    CREATE INDEX [IX_Mensaje_IdConversacion] ON [Mensaje] ([IdConversacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260818181822_CrearEstructuraInicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260818181822_CrearEstructuraInicial', N'8.0.1');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821153849_CrearSeguridadYAuditoria'
)
BEGIN
    CREATE TABLE [Rol] (
        [IdRol] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        [Descripcion] nvarchar(250) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Rol] PRIMARY KEY ([IdRol])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821153849_CrearSeguridadYAuditoria'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821153849_CrearSeguridadYAuditoria'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821153849_CrearSeguridadYAuditoria'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821153849_CrearSeguridadYAuditoria'
)
BEGIN
    CREATE TABLE [UsuarioRol] (
        [IdUsuario] int NOT NULL,
        [IdRol] int NOT NULL,
        CONSTRAINT [PK_UsuarioRol] PRIMARY KEY ([IdUsuario], [IdRol]),
        CONSTRAINT [FK_UsuarioRol_Rol_IdRol] FOREIGN KEY ([IdRol]) REFERENCES [Rol] ([IdRol]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UsuarioRol_Usuario_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuario] ([IdUsuario]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821153849_CrearSeguridadYAuditoria'
)
BEGIN
    CREATE INDEX [IX_AuditoriaActividad_IdUsuario] ON [AuditoriaActividad] ([IdUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821153849_CrearSeguridadYAuditoria'
)
BEGIN
    CREATE INDEX [IX_AuditoriaSesion_IdUsuario] ON [AuditoriaSesion] ([IdUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821153849_CrearSeguridadYAuditoria'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Rol_Nombre] ON [Rol] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821153849_CrearSeguridadYAuditoria'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuario_Correo] ON [Usuario] ([Correo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821153849_CrearSeguridadYAuditoria'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuario_Usuario] ON [Usuario] ([Usuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821153849_CrearSeguridadYAuditoria'
)
BEGIN
    CREATE INDEX [IX_UsuarioRol_IdRol] ON [UsuarioRol] ([IdRol]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821153849_CrearSeguridadYAuditoria'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821153849_CrearSeguridadYAuditoria', N'8.0.1');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825164342_CrearMotorConfiguracionAsistente'
)
BEGIN
    ALTER TABLE [Conversacion] ADD [IdAsistente] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825164342_CrearMotorConfiguracionAsistente'
)
BEGIN
    CREATE TABLE [Asistente] (
        [IdAsistente] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(500) NOT NULL,
        [ModeloIA] nvarchar(100) NOT NULL,
        [Idioma] nvarchar(50) NOT NULL,
        [LongitudRespuesta] nvarchar(50) NOT NULL,
        [Formalidad] nvarchar(50) NOT NULL,
        [FormatoRespuesta] nvarchar(100) NOT NULL,
        [Restricciones] nvarchar(4000) NOT NULL,
        [MensajeBienvenida] nvarchar(1000) NOT NULL,
        [Temperatura] decimal(4,2) NOT NULL,
        [MaxTokens] int NOT NULL,
        [TimeoutSeconds] int NOT NULL,
        [Activo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Asistente] PRIMARY KEY ([IdAsistente])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825164342_CrearMotorConfiguracionAsistente'
)
BEGIN
    CREATE TABLE [PromptSistema] (
        [IdPrompt] int NOT NULL IDENTITY,
        [IdAsistente] int NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [Contenido] nvarchar(max) NOT NULL,
        [Version] int NOT NULL,
        [Activo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_PromptSistema] PRIMARY KEY ([IdPrompt]),
        CONSTRAINT [FK_PromptSistema_Asistente_IdAsistente] FOREIGN KEY ([IdAsistente]) REFERENCES [Asistente] ([IdAsistente]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825164342_CrearMotorConfiguracionAsistente'
)
BEGIN
    CREATE TABLE [HistorialPrompt] (
        [IdHistorial] int NOT NULL IDENTITY,
        [IdPrompt] int NOT NULL,
        [Version] int NOT NULL,
        [Contenido] nvarchar(max) NOT NULL,
        [FechaModificacion] datetime2 NOT NULL,
        [UsuarioModificacion] nvarchar(50) NOT NULL,
        [MotivoCambio] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_HistorialPrompt] PRIMARY KEY ([IdHistorial]),
        CONSTRAINT [FK_HistorialPrompt_PromptSistema_IdPrompt] FOREIGN KEY ([IdPrompt]) REFERENCES [PromptSistema] ([IdPrompt]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825164342_CrearMotorConfiguracionAsistente'
)
BEGIN
    CREATE INDEX [IX_Conversacion_IdAsistente] ON [Conversacion] ([IdAsistente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825164342_CrearMotorConfiguracionAsistente'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Asistente_Nombre] ON [Asistente] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825164342_CrearMotorConfiguracionAsistente'
)
BEGIN
    CREATE INDEX [IX_HistorialPrompt_IdPrompt] ON [HistorialPrompt] ([IdPrompt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825164342_CrearMotorConfiguracionAsistente'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PromptSistema_IdAsistente_Activo] ON [PromptSistema] ([IdAsistente], [Activo]) WHERE [Activo] = 1');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825164342_CrearMotorConfiguracionAsistente'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PromptSistema_IdAsistente_Version] ON [PromptSistema] ([IdAsistente], [Version]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825164342_CrearMotorConfiguracionAsistente'
)
BEGIN
    ALTER TABLE [Conversacion] ADD CONSTRAINT [FK_Conversacion_Asistente_IdAsistente] FOREIGN KEY ([IdAsistente]) REFERENCES [Asistente] ([IdAsistente]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825164342_CrearMotorConfiguracionAsistente'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260825164342_CrearMotorConfiguracionAsistente', N'8.0.1');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826145808_ImplementarMemoriaConversacional'
)
BEGIN
    ALTER TABLE [Conversacion] ADD [FechaUltimaActividad] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826145808_ImplementarMemoriaConversacional'
)
BEGIN
    ALTER TABLE [Conversacion] ADD [IdUsuario] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826145808_ImplementarMemoriaConversacional'
)
BEGIN
    ALTER TABLE [Conversacion] ADD [ResumenContexto] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826145808_ImplementarMemoriaConversacional'
)
BEGIN
    ALTER TABLE [Conversacion] ADD [Titulo] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826145808_ImplementarMemoriaConversacional'
)
BEGIN
    ALTER TABLE [Conversacion] ADD [TotalMensajes] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826145808_ImplementarMemoriaConversacional'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826145808_ImplementarMemoriaConversacional'
)
BEGIN
    CREATE TABLE [ConfiguracionMemoria] (
        [IdConfiguracion] int NOT NULL IDENTITY,
        [MaximoMensajesContexto] int NOT NULL,
        [MaximoTokensContexto] int NOT NULL,
        [LongitudResumen] int NOT NULL,
        [CantidadConversacionesVisibles] int NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_ConfiguracionMemoria] PRIMARY KEY ([IdConfiguracion])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826145808_ImplementarMemoriaConversacional'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdConfiguracion', N'Activo', N'CantidadConversacionesVisibles', N'LongitudResumen', N'MaximoMensajesContexto', N'MaximoTokensContexto') AND [object_id] = OBJECT_ID(N'[ConfiguracionMemoria]'))
        SET IDENTITY_INSERT [ConfiguracionMemoria] ON;
    EXEC(N'INSERT INTO [ConfiguracionMemoria] ([IdConfiguracion], [Activo], [CantidadConversacionesVisibles], [LongitudResumen], [MaximoMensajesContexto], [MaximoTokensContexto])
    VALUES (1, CAST(1 AS bit), 20, 800, 10, 3000)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdConfiguracion', N'Activo', N'CantidadConversacionesVisibles', N'LongitudResumen', N'MaximoMensajesContexto', N'MaximoTokensContexto') AND [object_id] = OBJECT_ID(N'[ConfiguracionMemoria]'))
        SET IDENTITY_INSERT [ConfiguracionMemoria] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826145808_ImplementarMemoriaConversacional'
)
BEGIN
    CREATE INDEX [IX_Conversacion_IdUsuario_Estado_FechaUltimaActividad] ON [Conversacion] ([IdUsuario], [Estado], [FechaUltimaActividad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826145808_ImplementarMemoriaConversacional'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ConfiguracionMemoria_Activo] ON [ConfiguracionMemoria] ([Activo]) WHERE [Activo] = 1');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826145808_ImplementarMemoriaConversacional'
)
BEGIN
    ALTER TABLE [Conversacion] ADD CONSTRAINT [FK_Conversacion_Usuario_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuario] ([IdUsuario]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826145808_ImplementarMemoriaConversacional'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260826145808_ImplementarMemoriaConversacional', N'8.0.1');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827172934_AgregarControlResumenConversacion'
)
BEGIN
    ALTER TABLE [Conversacion] ADD [TotalMensajesResumidos] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827172934_AgregarControlResumenConversacion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260827172934_AgregarControlResumenConversacion', N'8.0.1');
END;
GO

COMMIT;
GO

