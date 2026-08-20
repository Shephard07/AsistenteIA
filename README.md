# Asistente Inteligente Empresarial

Aplicación web empresarial desarrollada en .NET 8 que permite mantener conversaciones con una inteligencia artificial local mediante Ollama. El proyecto aplica Clean Architecture, persistencia con SQL Server y Entity Framework Core, validaciones, documentación de API, registros estructurados y pruebas unitarias.

## Características

- Chat web conectado a una API REST.
- Integración local con Ollama y el modelo `deepseek-r1:7b`.
- Persistencia de conversaciones y mensajes en SQL Server.
- Arquitectura por capas con separación de responsabilidades.
- DTOs para comunicación entre API y Application.
- Validaciones mediante FluentValidation.
- Manejo global de excepciones con respuestas JSON.
- Registro de solicitudes, tiempos y errores mediante Serilog.
- Documentación interactiva mediante Swagger/OpenAPI.
- Pruebas unitarias con xUnit y Moq.
- Cobertura de `Asistente.Application` superior al 70 %.

## Arquitectura

```text
Asistente.Web
      │ HTTP / JSON
      ▼
Asistente.API
      │ DTOs + servicios de Application
      ▼
Asistente.Application
      │ interfaces
      ▼
Asistente.Domain
      ▲
      │ implementaciones
Asistente.Infrastructure
      │
      ├── SQL Server / Entity Framework Core
      └── Ollama
```

## Proyectos

| Proyecto | Responsabilidad |
|---|---|
| `Asistente.Web` | Interfaz web MVC del chat. |
| `Asistente.API` | Endpoints REST, Swagger, CORS, middleware y Serilog. |
| `Asistente.Application` | Casos de uso, DTOs, validadores e interfaces. |
| `Asistente.Domain` | Entidades, reglas de dominio, enums y contratos de repositorio. |
| `Asistente.Infrastructure` | EF Core, SQL Server, repositorios y proveedor Ollama. |
| `Asistente.Shared` | Modelos compartidos de errores. |
| `Asistente.Tests` | Pruebas unitarias con xUnit y Moq. |

## Tecnologías

- .NET 8 y C#
- ASP.NET Core MVC y Web API
- SQL Server Express
- Entity Framework Core
- Ollama con `deepseek-r1:7b`
- FluentValidation
- Serilog
- Swagger / OpenAPI
- xUnit, Moq y Coverlet

## Requisitos previos

- Visual Studio 2022 con desarrollo de ASP.NET y .NET 8.
- SQL Server Express.
- Ollama instalado.
- Modelo local descargado:

```powershell
ollama pull deepseek-r1:7b
```

## Configuración

La cadena de conexión, configuración de Ollama, CORS y Serilog se encuentran en:

```text
Asistente.API/appsettings.json
```

La aplicación posee configuración por ambiente:

```text
appsettings.Development.json
appsettings.Testing.json
appsettings.Production.json
```

El archivo de producción usa valores de ejemplo que deben reemplazarse antes de publicar el sistema.

## Ejecución local

1. Verificar que Ollama tenga el modelo instalado:

```powershell
ollama list
```

2. Si Ollama no está iniciado, ejecutar:

```powershell
ollama serve
```

3. En Visual Studio, iniciar simultáneamente:

```text
Asistente.API
Asistente.Web
```

4. Abrir la aplicación web:

```text
http://localhost:5201
```

5. Consultar Swagger:

```text
http://localhost:5148/swagger
```

## Endpoint principal

```http
POST /api/conversaciones/mensajes
```

Ejemplo de solicitud:

```json
{
  "idConversacion": null,
  "mensaje": "Hola, ¿cómo puede ayudarme el asistente?"
}
```

Ejemplo de respuesta:

```json
{
  "idConversacion": 1,
  "respuesta": "Respuesta generada por el asistente.",
  "tiempoRespuestaMs": 1200
}
```

## Validaciones y errores

- El mensaje es obligatorio.
- El mensaje admite como máximo 2000 caracteres.
- Si se envía un identificador de conversación, debe ser mayor que cero.
- Los errores se devuelven en formato JSON sin detalles técnicos expuestos al cliente.
- Los errores de Ollama, tiempos de espera y excepciones se registran en Serilog.

## Registros

Los registros diarios se generan en:

```text
Asistente.API/Logs/log-AAAA-MM-DD.txt
```

No se versionan en Git.

## Pruebas y cobertura

Ejecutar pruebas:

```powershell
dotnet test
```

Generar cobertura:

```powershell
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
reportgenerator -reports:"TestResults\**\coverage.cobertura.xml" -targetdir:"TestCoverage" -reporttypes:"Html"
start TestCoverage\index.html
```

Resultado actual:

```text
13 pruebas aprobadas
88.2 % de cobertura de líneas en Asistente.Application
```

## Control de versiones

- Repositorio: https://github.com/Shephard07/AsistenteIA
- Rama de Etapa 1: `main`
- Rama de desarrollo de Etapa 2: `etapa2-arquitectura`
- Etiqueta de respaldo de Etapa 1: `etapa1-final`