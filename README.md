# Asistente Inteligente Empresarial

Aplicación web empresarial desarrollada en .NET 8 que permite mantener conversaciones con una inteligencia artificial local mediante Ollama. El sistema utiliza SQL Server, Entity Framework Core, Clean Architecture, autenticación por roles, auditoría y un motor configurable de asistentes y prompts.

## Características principales

* Chat web conectado a una API REST.
* Integración local con Ollama y el modelo `deepseek-r1:7b`.
* Persistencia de conversaciones y mensajes en SQL Server.
* Arquitectura por capas con separación de responsabilidades.
* Autenticación mediante cookies y control de acceso por roles.
* Administración de usuarios, roles y permisos.
* Auditoría de inicios y cierres de sesión, errores de acceso y acciones administrativas.
* Configuración dinámica del asistente desde base de datos.
* Prompts del sistema versionados, auditables y configurables.
* Prueba de prompts mostrando el prompt generado y la respuesta de Ollama.
* DTOs para la comunicación entre capas.
* Validaciones mediante FluentValidation.
* Manejo global de excepciones con respuestas JSON controladas.
* Registro de errores, solicitudes y tiempos mediante Serilog.
* Documentación interactiva mediante Swagger/OpenAPI.
* Pruebas unitarias con xUnit y Moq.

## Arquitectura

```text
Asistente.Web
      │ Interfaz MVC, cookies y navegación por roles
      ▼
Asistente.API
      │ Endpoints REST, Swagger, CORS, middleware y Serilog
      ▼
Asistente.Application
      │ Casos de uso, DTOs, validadores, servicios e interfaces
      ▼
Asistente.Domain
      ▲
      │ Implementaciones
Asistente.Infrastructure
      │
      ├── SQL Server / Entity Framework Core
      ├── Repositorios
      └── Ollama
```

## Proyectos de la solución

| Proyecto                   | Responsabilidad                                                       |
| -------------------------- | --------------------------------------------------------------------- |
| `Asistente.Web`            | Interfaz web MVC del chat, autenticación y administración.            |
| `Asistente.API`            | Endpoints REST, Swagger, CORS, middleware y Serilog.                  |
| `Asistente.Application`    | Casos de uso, DTOs, validadores, interfaces y lógica de aplicación.   |
| `Asistente.Domain`         | Entidades, reglas de dominio, enums y contratos de repositorio.       |
| `Asistente.Infrastructure` | EF Core, SQL Server, repositorios, inicialización y proveedor Ollama. |
| `Asistente.Shared`         | Modelos compartidos, principalmente respuestas de error.              |
| `Asistente.Tests`          | Pruebas unitarias con xUnit y Moq.                                    |

## Tecnologías

* .NET 8.
* ASP.NET Core MVC y ASP.NET Core Web API.
* Entity Framework Core.
* SQL Server.
* Ollama.
* Modelo local `deepseek-r1:7b`.
* FluentValidation.
* Serilog.
* Swagger/OpenAPI.
* xUnit.
* Moq.
* ReportGenerator.
* Bootstrap.

## Etapa 1: chat local con Ollama

La primera etapa implementó el chat empresarial básico.

* Interfaz web para enviar preguntas.
* API REST para procesar mensajes.
* Comunicación con Ollama mediante `HttpClient`.
* Uso del modelo local `deepseek-r1:7b`.
* Registro de conversaciones y mensajes.

## Etapa 2: arquitectura, calidad y persistencia

La segunda etapa fortaleció la estructura técnica del sistema.

* Aplicación de Clean Architecture.
* Persistencia con SQL Server y Entity Framework Core.
* Separación mediante Domain, Application, Infrastructure, API y Web.
* Uso de DTOs entre las capas.
* Validación de solicitudes con FluentValidation.
* Middleware global para excepciones.
* Respuestas de error en formato JSON.
* Registros estructurados con Serilog.
* Documentación de endpoints mediante Swagger.
* Pruebas unitarias de servicios y validadores.

## Etapa 3: seguridad, usuarios, roles y auditoría

La tercera etapa incorporó control de acceso y trazabilidad.

### Roles del sistema

| Rol             | Acceso                                                  |
| --------------- | ------------------------------------------------------- |
| `Administrador` | Chat, usuarios, roles, auditoría y configuración de IA. |
| `Operador`      | Acceso al chat empresarial.                             |
| `Supervisor`    | Consulta de registros de auditoría.                     |

### Funcionalidades de seguridad

* Inicio y cierre de sesión.
* Contraseñas protegidas mediante hash.
* Autenticación con cookies.
* Autorización basada en roles.
* Administración de usuarios.
* Administración de roles.
* Asignación de roles a usuarios.
* Activación y desactivación de usuarios y roles.
* Cambio de contraseña.
* Auditoría de sesiones y actividades.
* Registro de intentos fallidos de inicio de sesión.
* Navegación web según el perfil autorizado.

### Entidades de seguridad

| Entidad              | Responsabilidad                             |
| -------------------- | ------------------------------------------- |
| `Usuario`            | Representa una cuenta de acceso al sistema. |
| `Rol`                | Define un perfil de permisos.               |
| `UsuarioRol`         | Relaciona usuarios con roles.               |
| `AuditoriaSesion`    | Registra inicios y cierres de sesión.       |
| `AuditoriaActividad` | Registra actividades e intentos fallidos.   |

## Etapa 4: motor configurable de asistentes y prompts

La cuarta etapa incorpora un motor configurable para modificar el comportamiento del asistente sin cambiar el código fuente. La configuración y el historial se guardan en SQL Server.

### Funcionalidades incorporadas

* Administración de asistentes configurables.
* Configuración de modelo, idioma, formalidad y restricciones.
* Configuración de temperatura, máximo de tokens y tiempo máximo de espera.
* Prompts del sistema almacenados en base de datos.
* Versionado de prompts.
* Desactivación automática de la versión activa anterior al crear una nueva.
* Historial de versiones con contenido, motivo del cambio, fecha y usuario.
* Construcción dinámica del prompt del sistema.
* Uso de la configuración activa en las conversaciones normales.
* Panel web para administrar la configuración de IA.
* Prueba de prompts sin registrar una conversación.
* Visualización del prompt generado, respuesta de Ollama y tiempo de respuesta.
* Auditoría de acciones relacionadas con asistentes y prompts.
* Acceso exclusivo del rol `Administrador` a la configuración de IA.

### Entidades del motor de IA

| Entidad           | Responsabilidad                                                                   |
| ----------------- | --------------------------------------------------------------------------------- |
| `Asistente`       | Almacena modelo, comportamiento, límites y configuración funcional del asistente. |
| `PromptSistema`   | Representa una versión de prompt asociada a un asistente.                         |
| `HistorialPrompt` | Registra versiones, contenido, motivo, fecha y usuario responsable.               |

### Configuración recomendada para Ollama

Para usar `deepseek-r1:7b` se recomienda:

```text
Temperatura: 0.4
Máximo de tokens: 1024
Tiempo máximo: 180 segundos
```

El modelo `deepseek-r1:7b` realiza razonamiento interno; un límite bajo de tokens puede ocasionar respuestas incompletas.

## Base de datos

La aplicación utiliza SQL Server y contiene, entre otras, las siguientes tablas:

```text
Conversacion
Mensaje
Usuario
Rol
UsuarioRol
AuditoriaSesion
AuditoriaActividad
Asistente
PromptSistema
HistorialPrompt
```

La migración de Etapa 4 se denomina:

```text
CrearMotorConfiguracionAsistente
```

Esta migración crea las tablas `Asistente`, `PromptSistema` e `HistorialPrompt`, además de la relación opcional entre `Conversacion` y `Asistente`.

## Requisitos previos

* .NET SDK 8.
* SQL Server.
* SQL Server Management Studio, opcional para revisar la base de datos.
* Ollama instalado.
* Modelo `deepseek-r1:7b` descargado.
* Visual Studio 2022 o Visual Studio Code, opcional.

## Instalación del modelo Ollama

Verificar los modelos disponibles:

```powershell
ollama list
```

Descargar el modelo si aún no está instalado:

```powershell
ollama pull deepseek-r1:7b
```

Iniciar Ollama si no está ejecutándose:

```powershell
ollama serve
```

## Configuración

La cadena de conexión, configuración de Ollama, CORS y Serilog se encuentran principalmente en:

```text
Asistente.API/appsettings.json
```

La solución también contiene configuraciones por ambiente:

```text
Asistente.API/appsettings.Development.json
Asistente.API/appsettings.Testing.json
Asistente.API/appsettings.Production.json
```

Antes de publicar el sistema, los valores de producción deben ser reemplazados por valores seguros del entorno correspondiente.

## Aplicar migraciones

Desde la raíz de la solución, ejecutar:

```powershell
dotnet ef database update --project Asistente.Infrastructure --startup-project Asistente.API
```

Las migraciones disponibles son:

```text
CrearEstructuraInicial
CrearSeguridadYAuditoria
CrearMotorConfiguracionAsistente
```

## Ejecución local

1. Verificar que SQL Server esté disponible.
2. Iniciar Ollama con `ollama serve`.
3. Aplicar migraciones si es necesario.
4. Iniciar simultáneamente los proyectos:

```text
Asistente.API
Asistente.Web
```

5. Abrir la aplicación web:

```text
http://localhost:5201
```

6. Consultar Swagger:

```text
http://localhost:5148/swagger
```

## Uso de la configuración de IA

1. Iniciar sesión con un usuario del rol `Administrador`.
2. Abrir la opción **Configuración IA**.
3. Crear o actualizar la configuración del asistente.
4. Crear el prompt inicial.
5. Crear nuevas versiones cuando se requiera modificar el comportamiento.
6. Registrar el motivo de cada cambio.
7. Consultar el historial de versiones.
8. Usar **Probar prompt** para ver el prompt generado, la respuesta de Ollama y el tiempo de respuesta.
9. Abrir el módulo **Asistente** y enviar una consulta para comprobar que el chat usa la configuración activa.

## Endpoints principales

### Autenticación

| Método | Endpoint                            | Descripción                            |
| ------ | ----------------------------------- | -------------------------------------- |
| POST   | `/api/autenticacion/iniciar-sesion` | Inicia sesión y registra la auditoría. |
| POST   | `/api/autenticacion/cerrar-sesion`  | Cierra la sesión activa.               |

### Usuarios y roles

| Método | Endpoint                    | Descripción                    |
| ------ | --------------------------- | ------------------------------ |
| GET    | `/api/usuarios`             | Lista usuarios.                |
| POST   | `/api/usuarios`             | Crea un usuario.               |
| PUT    | `/api/usuarios/{id}`        | Actualiza datos de un usuario. |
| PATCH  | `/api/usuarios/{id}/estado` | Activa o desactiva un usuario. |
| GET    | `/api/roles`                | Lista roles.                   |
| POST   | `/api/roles`                | Crea un rol.                   |
| PUT    | `/api/roles/{id}`           | Actualiza un rol.              |
| PATCH  | `/api/roles/{id}/estado`    | Activa o desactiva un rol.     |

### Auditoría

| Método | Endpoint                     | Descripción                    |
| ------ | ---------------------------- | ------------------------------ |
| GET    | `/api/auditoria/sesiones`    | Lista sesiones registradas.    |
| GET    | `/api/auditoria/actividades` | Lista actividades registradas. |

### Asistentes y prompts

| Método | Endpoint                                 | Descripción                                    |
| ------ | ---------------------------------------- | ---------------------------------------------- |
| GET    | `/api/asistentes`                        | Lista asistentes configurados.                 |
| GET    | `/api/asistentes/{id}`                   | Obtiene un asistente por identificador.        |
| POST   | `/api/asistentes`                        | Crea un asistente.                             |
| PUT    | `/api/asistentes/{id}`                   | Actualiza la configuración del asistente.      |
| PATCH  | `/api/asistentes/{id}/estado`            | Activa o desactiva un asistente.               |
| GET    | `/api/asistentes/{id}/prompts`           | Lista los prompts de un asistente.             |
| POST   | `/api/asistentes/{id}/prompts`           | Crea un prompt inicial.                        |
| POST   | `/api/prompts/{idPrompt}/versiones`      | Crea una nueva versión del prompt.             |
| PATCH  | `/api/prompts/{idPrompt}/estado`         | Activa o desactiva un prompt.                  |
| GET    | `/api/asistentes/{id}/prompts/historial` | Consulta el historial de prompts.              |
| POST   | `/api/pruebas-prompts`                   | Prueba un prompt sin guardar una conversación. |

### Conversaciones

| Método | Endpoint                       | Descripción                                               |
| ------ | ------------------------------ | --------------------------------------------------------- |
| POST   | `/api/conversaciones/mensajes` | Envía un mensaje al asistente y registra la conversación. |

Ejemplo de solicitud:

```json
{
  "idConversacion": null,
  "mensaje": "Dame tres recomendaciones para mejorar la productividad."
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

## Validaciones y manejo de errores

* El mensaje es obligatorio.
* El mensaje admite como máximo 2000 caracteres.
* Los identificadores enviados deben ser válidos.
* Las contraseñas deben cumplir requisitos de seguridad.
* Los errores se devuelven en JSON sin exponer detalles técnicos.
* Los errores de Ollama, tiempos de espera y excepciones se registran mediante Serilog.
* Si Ollama no está disponible, el sistema devuelve una respuesta controlada.

## Registros

Los registros diarios se generan en:

```text
Asistente.API/Logs/log-AAAA-MM-DD.txt
```

Los archivos de log no se versionan en Git.

## Pruebas unitarias

Ejecutar las pruebas:

```powershell
dotnet test
```

Resultado actual:

```text
35 pruebas aprobadas.
0 pruebas con error.
```

Las pruebas cubren, entre otros:

* Servicios de conversación y mensajes.
* Envío de mensajes al proveedor de IA.
* Validadores.
* Autenticación.
* Usuarios y roles.
* Auditoría.
* Construcción dinámica de prompts.
* Creación y versionado de prompts.
* Prueba de prompts.
* Validación de configuración del asistente.

## Cobertura de código

Generar cobertura:

```powershell
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResultsEtapa4
reportgenerator -reports:"TestResultsEtapa4\**\coverage.cobertura.xml" -targetdir:"TestCoverageEtapa4" -reporttypes:"Html"
Start-Process .\TestCoverageEtapa4\index.html
```

Resultado actual:

```text
70.6 % de cobertura de líneas en Asistente.Application.
```

## Scripts SQL

Los scripts SQL de cada etapa se encuentran en:

```text
Scripts/
```

Para la Etapa 4:

```text
Scripts/Etapa4_CrearMotorConfiguracionAsistente.sql
```

## Control de versiones

Repositorio:

```text
https://github.com/Shephard07/AsistenteIA
```

Ramas principales:

```text
main
etapa2-arquitectura
etapa3-seguridad
etapa4-motor-prompts
```

Etiquetas de entrega:

```text
etapa1-final
etapa2-final
etapa3-final
```
