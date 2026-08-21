# Asistente Inteligente Empresarial

Aplicación empresarial desarrollada en .NET 8 que permite conversar con una inteligencia artificial local mediante Ollama. El proyecto aplica Clean Architecture, persistencia con SQL Server, autenticación por cookies, autorización por roles, auditoría, validaciones, registros estructurados y pruebas unitarias.

## Características principales

- Chat web conectado a una API REST.
- Integración local con Ollama y el modelo `deepseek-r1:7b`.
- Persistencia de conversaciones y mensajes en SQL Server.
- Arquitectura por capas con separación de responsabilidades.
- DTOs para comunicación entre API y Application.
- Validaciones mediante FluentValidation.
- Manejo global de excepciones con respuestas JSON controladas.
- Registro técnico de solicitudes, errores y tiempos mediante Serilog.
- Documentación interactiva de la API con Swagger/OpenAPI.
- Pruebas unitarias con xUnit y Moq.
- Autenticación con usuario y contraseña.
- Gestión de sesión mediante cookies.
- Autorización por roles: Administrador, Operador y Supervisor.
- Administración de usuarios y roles.
- Auditoría de sesiones, accesos y acciones administrativas.
- Contraseñas protegidas mediante hash y salt con `PasswordHasher`.

## Arquitectura

```text
Asistente.Web
      │ HTTP / JSON + Cookie
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

| Proyecto | Responsabilidad |
|---|---|
| `Asistente.Web` | Interfaz MVC, inicio y cierre de sesión, menú por roles y pantallas administrativas. |
| `Asistente.API` | Endpoints REST, Swagger, CORS, autenticación, autorización, middleware y Serilog. |
| `Asistente.Application` | Casos de uso, DTOs, validadores e interfaces. |
| `Asistente.Domain` | Entidades, reglas de negocio, enums y contratos de repositorio. |
| `Asistente.Infrastructure` | EF Core, SQL Server, repositorios, contraseñas y proveedor Ollama. |
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
- Cookie Authentication
- xUnit, Moq y Coverlet

## Seguridad implementada

### Autenticación

El sistema valida usuario, contraseña y estado activo de la cuenta.

- Un usuario inactivo no puede iniciar sesión.
- Los mensajes de error no revelan si el usuario existe.
- Cada inicio de sesión exitoso crea una auditoría de sesión.
- Cada intento fallido queda registrado en auditoría.

### Contraseñas

Las contraseñas no se guardan en texto plano.

- Se utiliza `PasswordHasher` de ASP.NET Core.
- El valor almacenado es `PasswordHash`.
- El hash incluye salt y mecanismos de derivación seguros.
- Los DTOs de respuesta nunca devuelven contraseñas ni hashes.
- Solo un Administrador puede cambiar contraseñas.

### Autorización por roles

| Rol | Accesos |
|---|---|
| Administrador | Chat, usuarios, roles, auditoría y operaciones administrativas. |
| Operador | Chat con la IA local. |
| Supervisor | Consulta de auditoría. |

La Web y la API aplican restricciones por roles. Una ruta no autorizada muestra la página de acceso denegado.

### Auditoría

Se registran los siguientes eventos:

- Inicio de sesión correcto.
- Inicio de sesión fallido.
- Cierre de sesión.
- Creación, edición, activación y desactivación de usuarios.
- Cambio de contraseña.
- Asignación de roles.
- Creación, edición, activación y desactivación de roles.
- Consultas de sesiones y actividades.

Las tablas de auditoría almacenan usuario, fecha, dirección IP, navegador, módulo, acción y descripción.

## Modelo de datos de seguridad

La Etapa 3 agrega las siguientes tablas:

| Tabla | Finalidad |
|---|---|
| `Usuario` | Datos de usuarios, estado, contraseña protegida y último acceso. |
| `Rol` | Perfiles de acceso configurables. |
| `UsuarioRol` | Relación muchos a muchos entre usuarios y roles. |
| `AuditoriaSesion` | Inicio, cierre, IP, navegador y estado de sesiones. |
| `AuditoriaActividad` | Registro de accesos y acciones del sistema. |

La migración de seguridad es:

```text
20260821153849_CrearSeguridadYAuditoria
```

El script SQL entregable se encuentra en:

```text
Scripts/Etapa3_CrearSeguridadYAuditoria.sql
```

## Requisitos previos

- Visual Studio 2022 con desarrollo de ASP.NET y .NET 8.
- SQL Server Express.
- Ollama instalado.
- Modelo local descargado:

```powershell
ollama pull deepseek-r1:7b
```

## Configuración

La cadena de conexión, Ollama, CORS y Serilog se encuentran en:

```text
Asistente.API/appsettings.json
```

La configuración por ambientes está en:

```text
Asistente.API/appsettings.Development.json
Asistente.API/appsettings.Testing.json
Asistente.API/appsettings.Production.json
```

Las claves de Data Protection se guardan localmente en:

```text
Asistente.DataProtectionKeys
```

Esta carpeta está ignorada por Git porque permite proteger las cookies compartidas entre la Web y la API.

## Ejecución local

1. Verificar que Ollama tenga instalado el modelo:

```powershell
ollama list
```

2. Si Ollama no está iniciado, ejecutar:

```powershell
ollama serve
```

3. Aplicar migraciones, si se está configurando una base nueva:

```powershell
dotnet ef database update --project Asistente.Infrastructure --startup-project Asistente.API
```

4. En Visual Studio, iniciar simultáneamente:

```text
Asistente.API
Asistente.Web
```

5. Abrir la aplicación Web:

```text
http://localhost:5201
```

6. Consultar Swagger:

```text
http://localhost:5148/swagger
```

## Cuentas de demostración local

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin123*` | Administrador |
| `operador1` | `Operador123*` | Operador |
| `supervisor1` | `Supervisor123*` | Supervisor |

Estas cuentas son únicamente para desarrollo local. No deben utilizarse en un ambiente de producción.

## Endpoints principales

| Método | Ruta | Descripción | Acceso |
|---|---|---|---|
| POST | `/api/autenticacion/iniciar-sesion` | Inicia sesión y crea la cookie. | Anónimo |
| POST | `/api/autenticacion/cerrar-sesion` | Cierra sesión y registra fecha de cierre. | Autenticado |
| GET/POST/PUT/PATCH | `/api/usuarios` | Gestión de usuarios, roles, contraseña y estado. | Administrador |
| GET/POST/PUT/PATCH | `/api/roles` | Gestión de roles y estado. | Administrador |
| GET | `/api/auditoria/sesiones` | Consulta sesiones registradas. | Administrador o Supervisor |
| GET | `/api/auditoria/actividades` | Consulta actividades registradas. | Administrador o Supervisor |
| POST | `/api/conversaciones/mensajes` | Envía mensajes a Ollama. | Administrador u Operador |

## Ejemplo de solicitud al asistente

```http
POST /api/conversaciones/mensajes
```

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

## Pruebas y cobertura

Ejecutar pruebas unitarias:

```powershell
dotnet test
```

Generar cobertura:

```powershell
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults

reportgenerator -reports:"TestResults\**\coverage.cobertura.xml" -targetdir:"TestCoverage" -reporttypes:"Html"

start TestCoverage\index.html
```

Resultado comprobado de la Etapa 3:

- 29 pruebas unitarias aprobadas.
- 92.3 % de cobertura de líneas en `Asistente.Application`.

## Registros

Los registros técnicos de Serilog se generan diariamente en:

```text
Asistente.API/Logs/log-AAAA-MM-DD.txt
```

Los logs y resultados temporales de cobertura no se versionan en Git.

## Control de versiones

- Repositorio: <https://github.com/Shephard07/AsistenteIA>
- Rama de Etapa 1: `main`
- Etiqueta de respaldo de Etapa 1: `etapa1-final`
- Etiqueta de respaldo de Etapa 2: `etapa2-final`
- Rama de desarrollo de Etapa 3: `etapa3-seguridad`