# Asistente Inteligente Empresarial

Aplicación web que permite mantener conversaciones con un asistente de inteligencia artificial ejecutado localmente mediante Ollama. El proyecto registra las conversaciones y mensajes en SQL Server y está desarrollado siguiendo principios de Clean Architecture.

## Tecnologías utilizadas

| Tecnología                        | Uso en el proyecto                          |
| --------------------------------- | ------------------------------------------- |
| ASP.NET Core 8                    | API REST y aplicación web MVC               |
| C#                                | Lógica de la aplicación                     |
| Entity Framework Core             | Acceso a datos y migraciones                |
| SQL Server Express                | Almacenamiento de conversaciones y mensajes |
| Ollama                            | Ejecución local del modelo de IA            |
| DeepSeek-R1 7B                    | Modelo de lenguaje utilizado                |
| HTML, CSS, Bootstrap y JavaScript | Interfaz de chat                            |
| Git y GitHub                      | Control de versiones                        |

## Arquitectura

La solución está organizada en seis proyectos para respetar Clean Architecture:

```text
Asistente.Web            -> Interfaz MVC para el usuario
Asistente.API            -> Endpoints HTTP y configuración de la API
Asistente.Application    -> Casos de uso y servicios de aplicación
Asistente.Domain         -> Entidades, reglas y contratos del dominio
Asistente.Infrastructure -> EF Core, SQL Server y conexión con Ollama
Asistente.Shared         -> Modelos compartidos de solicitud y respuesta
```

Los controladores no contienen lógica de negocio. Su función es recibir la solicitud HTTP y delegarla a los servicios de la capa Application.

## Funcionalidades

* Envío de mensajes a un asistente IA local.
* Registro de conversaciones y mensajes en SQL Server.
* Visualización del historial durante la conversación.
* Medición del tiempo de respuesta de la IA.
* Limpieza de conversación desde la interfaz web.
* Manejo centralizado de errores.
* Configuración del modelo Ollama desde `appsettings.json`.
* Uso de CORS para permitir la comunicación entre la Web MVC y la API.

## Requisitos previos

Antes de ejecutar el proyecto se debe tener instalado:

* Visual Studio 2022 con desarrollo de ASP.NET y web.
* .NET 8 SDK.
* SQL Server Express o SQL Server 2022.
* SQL Server Management Studio (opcional, para revisar la base de datos).
* Ollama.
* Git.

## Configuración de Ollama

Instalar Ollama desde:

```text
https://ollama.com/
```

Luego, en PowerShell, descargar el modelo:

```powershell
ollama pull deepseek-r1:7b
```

Verificar los modelos disponibles:

```powershell
ollama list
```

Si el servicio de Ollama no está iniciado, ejecutar:

```powershell
ollama serve
```

Para liberar la memoria utilizada por el modelo después de una prueba:

```powershell
ollama stop deepseek-r1:7b
```

## Configuración de base de datos

La aplicación utiliza SQL Server Express en la instancia:

```text
localhost\SQLEXPRESS
```

La cadena de conexión se encuentra en `Asistente.API/appsettings.json`:

```json
"ConnectionStrings": {
  "AsistenteIA": "Server=localhost\\SQLEXPRESS;Database=AsistenteIA;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
}
```

La base de datos se denomina:

```text
AsistenteIA
```

Para crearla mediante las migraciones de Entity Framework Core, abrir la Consola del Administrador de paquetes de Visual Studio y ejecutar:

```powershell
Update-Database -Project Asistente.Infrastructure -StartupProject Asistente.API
```

## Configuración de la IA

La configuración de Ollama está en `Asistente.API/appsettings.json`:

```json
"Ollama": {
  "BaseUrl": "http://localhost:11434",
  "Model": "deepseek-r1:7b",
  "TimeoutSeconds": 120,
  "KeepAlive": "0"
}
```

Para usar otro modelo, se modifica únicamente el valor de `Model` y se reinicia la API. No es necesario recompilar el código.

## Ejecución del proyecto

1. Verificar que SQL Server esté disponible.
2. Verificar que Ollama esté instalado y que el modelo exista con:

```powershell
ollama list
```

3. Ejecutar primero el proyecto `Asistente.API`.

La API se ejecuta normalmente en:

```text
http://localhost:5148
```

4. Ejecutar después el proyecto `Asistente.Web` usando el perfil HTTP.

La Web se ejecuta normalmente en:

```text
http://localhost:5201
```

5. Abrir la URL de la Web en el navegador y enviar un mensaje.

> Si los puertos cambian en tu equipo, actualizar `Api:BaseUrl` en `Asistente.Web/appsettings.json` y los orígenes permitidos en la configuración CORS de la API.

## Endpoint principal

```text
POST /api/conversaciones/mensajes
```

Ejemplo de solicitud:

```json
{
  "idConversacion": null,
  "mensaje": "Hola, ¿cómo puedes ayudarme?"
}
```

Ejemplo de respuesta:

```json
{
  "idConversacion": 1,
  "respuesta": "Hola, soy tu asistente inteligente empresarial.",
  "tiempoRespuestaMs": 21000
}
```

Prueba desde PowerShell:

```powershell
$body = @{
    mensaje = "Prueba de conexión con Ollama."
} | ConvertTo-Json

Invoke-RestMethod `
  -Uri "http://localhost:5148/api/conversaciones/mensajes" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

## Base de datos

La solución maneja las siguientes tablas:

| Tabla                   | Descripción                                                                             |
| ----------------------- | --------------------------------------------------------------------------------------- |
| `Conversacion`          | Almacena el estado y fechas de cada conversación.                                       |
| `Mensaje`               | Almacena cada mensaje del usuario y de la IA, junto con el tiempo de respuesta.         |
| `__EFMigrationsHistory` | Tabla interna utilizada por Entity Framework Core para controlar migraciones aplicadas. |

## Control de versiones

El repositorio utiliza Git para registrar los cambios del proyecto.

```text
Repositorio: https://github.com/Shephard07/AsistenteIA
```

Comandos básicos utilizados:

```powershell
git status
git add .
git commit -m "Descripción del cambio"
git push
```

## Autoría

Proyecto académico individual: Asistente Inteligente Empresarial.
