const paginaConfiguracionMemoria = document.getElementById(
    "configuracionMemoriaPage");

const apiBaseUrlConfiguracionMemoria =
    paginaConfiguracionMemoria.dataset.apiBaseUrl
        .replace(/\/$/, "");

const formularioConfiguracionMemoria = document.getElementById(
    "formConfiguracionMemoria");

const btnGuardarConfiguracionMemoria = document.getElementById(
    "btnGuardarConfiguracionMemoria");

const mensajeConfiguracionMemoria = document.getElementById(
    "mensajeConfiguracionMemoria");

const estadoConfiguracionMemoria = document.getElementById(
    "estadoConfiguracionMemoria");

function mostrarMensaje(mensaje, esError = false) {
    mensajeConfiguracionMemoria.textContent = mensaje;
    mensajeConfiguracionMemoria.className =
        `alert ${esError ? "alert-danger" : "alert-success"}`;
}

function ocultarMensaje() {
    mensajeConfiguracionMemoria.textContent = "";
    mensajeConfiguracionMemoria.className = "alert d-none";
}

async function solicitar(url, opciones = {}) {
    const response = await fetch(
        `${apiBaseUrlConfiguracionMemoria}${url}`,
        {
            credentials: "include",
            ...opciones,
            headers: {
                ...(opciones.body
                    ? { "Content-Type": "application/json" }
                    : {}),
                ...(opciones.headers || {})
            }
        });

    if (!response.ok) {
        let mensaje = "No fue posible procesar la solicitud.";

        try {
            const error = await response.json();

            if (Array.isArray(error.errores)) {
                mensaje = error.errores.join(" ");
            } else {
                mensaje = error.mensaje || mensaje;
            }
        } catch {
            // La respuesta no contiene JSON.
        }

        throw new Error(mensaje);
    }

    return response.json();
}

function llenarFormulario(configuracion) {
    document.getElementById("maximoMensajesContexto").value =
        configuracion.maximoMensajesContexto;

    document.getElementById("maximoTokensContexto").value =
        configuracion.maximoTokensContexto;

    document.getElementById("longitudResumen").value =
        configuracion.longitudResumen;

    document.getElementById("cantidadConversacionesVisibles").value =
        configuracion.cantidadConversacionesVisibles;

    estadoConfiguracionMemoria.textContent = configuracion.activo
        ? "Configuración activa"
        : "Configuración inactiva";

    estadoConfiguracionMemoria.className = configuracion.activo
        ? "badge text-bg-success"
        : "badge text-bg-secondary";
}

async function cargarConfiguracion() {
    ocultarMensaje();

    try {
        const configuracion = await solicitar(
            "/api/configuracion-memoria");

        llenarFormulario(configuracion);
    } catch (error) {
        mostrarMensaje(error.message, true);
    }
}

formularioConfiguracionMemoria.addEventListener(
    "submit",
    async event => {
        event.preventDefault();
        ocultarMensaje();

        const solicitud = {
            maximoMensajesContexto: Number(
                document.getElementById(
                    "maximoMensajesContexto").value),

            maximoTokensContexto: Number(
                document.getElementById(
                    "maximoTokensContexto").value),

            longitudResumen: Number(
                document.getElementById(
                    "longitudResumen").value),

            cantidadConversacionesVisibles: Number(
                document.getElementById(
                    "cantidadConversacionesVisibles").value)
        };

        btnGuardarConfiguracionMemoria.disabled = true;
        btnGuardarConfiguracionMemoria.textContent = "Guardando...";

        try {
            const configuracion = await solicitar(
                "/api/configuracion-memoria",
                {
                    method: "PUT",
                    body: JSON.stringify(solicitud)
                });

            llenarFormulario(configuracion);

            mostrarMensaje(
                "Configuración de memoria actualizada correctamente.");
        } catch (error) {
            mostrarMensaje(error.message, true);
        } finally {
            btnGuardarConfiguracionMemoria.disabled = false;
            btnGuardarConfiguracionMemoria.textContent =
                "Guardar configuración";
        }
    });

cargarConfiguracion();