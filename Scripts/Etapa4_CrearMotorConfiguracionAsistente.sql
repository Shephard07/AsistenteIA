BEGIN TRANSACTION;
GO

ALTER TABLE [Conversacion] ADD [IdAsistente] int NULL;
GO

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
GO

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
GO

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
GO

CREATE INDEX [IX_Conversacion_IdAsistente] ON [Conversacion] ([IdAsistente]);
GO

CREATE UNIQUE INDEX [IX_Asistente_Nombre] ON [Asistente] ([Nombre]);
GO

CREATE INDEX [IX_HistorialPrompt_IdPrompt] ON [HistorialPrompt] ([IdPrompt]);
GO

CREATE UNIQUE INDEX [IX_PromptSistema_IdAsistente_Activo] ON [PromptSistema] ([IdAsistente], [Activo]) WHERE [Activo] = 1;
GO

CREATE UNIQUE INDEX [IX_PromptSistema_IdAsistente_Version] ON [PromptSistema] ([IdAsistente], [Version]);
GO

ALTER TABLE [Conversacion] ADD CONSTRAINT [FK_Conversacion_Asistente_IdAsistente] FOREIGN KEY ([IdAsistente]) REFERENCES [Asistente] ([IdAsistente]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260825164342_CrearMotorConfiguracionAsistente', N'8.0.1');
GO

COMMIT;
GO

