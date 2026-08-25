const configuracionIAPage = document.getElementById("configuracionIAPage");
const apiBaseUrlConfiguracionIA =
    configuracionIAPage.dataset.apiBaseUrl.replace(/\/$/, "");

const mensajeConfiguracionIA = document.getElementById(
    "mensajeConfiguracionIA");

const formConfiguracionAsistente = document.getElementById(
    "formConfiguracionAsistente");

const formNuevaVersionPrompt = document.getElementById(
    "formNuevaVersionPrompt");

const formProbarPrompt = document.getElementById(
    "formProbarPrompt");

const historialPromptBody = document.getElementById(
    "historialPromptBody");

const resultadoPruebaPrompt = document.getElementById(
    "resultadoPruebaPrompt");

let asistenteActivo = null;
let promptActivo = null;

function mostrarMensajeConfiguracionIA(mensaje, esError = false) {
    mensajeConfiguracionIA.textContent = mensaje;
    mensajeConfiguracionIA.className =
        `alert ${esError ? "alert-danger" : "alert-success"}`;
}

function ocultarMensajeConfiguracionIA() {
    mensajeConfiguracionIA.className = "alert d-none";
    mensajeConfiguracionIA.textContent = "";
}

async function solicitarConfiguracionIA(url, opciones = {}) {
    const response = await fetch(
        `${apiBaseUrlConfiguracionIA}${url}`,
        {
            credentials: "include",
            ...opciones,
            headers: {
                "Content-Type": "application/json",
                ...(opciones.headers || {})
            }
        });

    if (!response.ok) {
        let mensaje = "No fue posible procesar la solicitud.";

        try {
            const error = await response.json();
            mensaje = error.mensaje || mensaje;
        } catch {
            // La respuesta no contiene JSON.
        }

        throw new Error(mensaje);
    }

    if (response.status === 204) {
        return null;
    }

    return response.json();
}

function escaparHtml(valor) {
    return String(valor ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#039;");
}

function formatearFecha(fecha) {
    if (!fecha) {
        return "-";
    }

    return new Date(fecha).toLocaleString("es-PE");
}

function llenarFormularioAsistente(asistente) {
    document.getElementById("idAsistente").value = asistente.idAsistente;
    document.getElementById("nombreAsistente").value = asistente.nombre;
    document.getElementById("descripcionAsistente").value =
        asistente.descripcion;
    document.getElementById("modeloIA").value = asistente.modeloIA;
    document.getElementById("idiomaAsistente").value = asistente.idioma;
    document.getElementById("longitudRespuesta").value =
        asistente.longitudRespuesta;
    document.getElementById("formalidadAsistente").value =
        asistente.formalidad;
    document.getElementById("formatoRespuesta").value =
        asistente.formatoRespuesta;
    document.getElementById("restriccionesAsistente").value =
        asistente.restricciones;
    document.getElementById("mensajeBienvenida").value =
        asistente.mensajeBienvenida;
    document.getElementById("temperaturaAsistente").value =
        asistente.temperatura;
    document.getElementById("maxTokensAsistente").value =
        asistente.maxTokens;
    document.getElementById("timeoutAsistente").value =
        asistente.timeoutSeconds;
}

function llenarDatosPromptActivo(prompt) {
    document.getElementById("idPromptActivo").value = prompt.idPrompt;
    document.getElementById("contenidoPrompt").value = prompt.contenido;
    document.getElementById("nombrePrompt").value =
        `${prompt.nombre} - versión ${prompt.version + 1}`;
}

function crearFilaHistorial(item) {
    return `
        <tr>
            <td>${item.version}</td>
            <td>${escaparHtml(item.motivoCambio)}</td>
            <td>${escaparHtml(item.usuarioModificacion)}</td>
            <td>${formatearFecha(item.fechaModificacion)}</td>
            <td>
                <details>
                    <summary>Ver contenido</summary>
                    <div class="mt-2 text-break">
                        ${escaparHtml(item.contenido)}
                    </div>
                </details>
            </td>
        </tr>`;
}

async function cargarHistorialPrompt() {
    try {
        const historial = await solicitarConfiguracionIA(
            `/api/asistentes/${asistenteActivo.idAsistente}/prompts/historial`);

        historialPromptBody.innerHTML = historial.length === 0
            ? `<tr>
                   <td colspan="5" class="text-center text-muted">
                       No hay versiones registradas.
                   </td>
               </tr>`
            : historial.map(crearFilaHistorial).join("");
    } catch (error) {
        historialPromptBody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center text-danger">
                    ${escaparHtml(error.message)}
                </td>
            </tr>`;
    }
}

async function cargarPromptActivo() {
    const prompts = await solicitarConfiguracionIA(
        `/api/asistentes/${asistenteActivo.idAsistente}/prompts`);

    promptActivo = prompts.find(prompt => prompt.activo) ?? null;

    if (!promptActivo) {
        document.getElementById("idPromptActivo").value = "";
        document.getElementById("contenidoPrompt").value = "";
        mostrarMensajeConfiguracionIA(
            "El asistente activo no tiene un prompt activo.",
            true);

        return;
    }

    llenarDatosPromptActivo(promptActivo);
}

async function cargarConfiguracionIA() {
    ocultarMensajeConfiguracionIA();

    try {
        const asistentes = await solicitarConfiguracionIA("/api/asistentes");

        asistenteActivo = asistentes.find(asistente => asistente.activo) ?? null;

        if (!asistenteActivo) {
            throw new Error(
                "No existe un asistente activo. Créalo primero desde la API.");
        }

        llenarFormularioAsistente(asistenteActivo);

        await cargarPromptActivo();
        await cargarHistorialPrompt();
    } catch (error) {
        mostrarMensajeConfiguracionIA(error.message, true);

        historialPromptBody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center text-danger">
                    ${escaparHtml(error.message)}
                </td>
            </tr>`;
    }
}

formConfiguracionAsistente.addEventListener("submit", async event => {
    event.preventDefault();

    try {
        const idAsistente = document.getElementById("idAsistente").value;

        await solicitarConfiguracionIA(`/api/asistentes/${idAsistente}`, {
            method: "PUT",
            body: JSON.stringify({
                nombre: document.getElementById("nombreAsistente").value,
                descripcion: document.getElementById(
                    "descripcionAsistente").value,
                modeloIA: document.getElementById("modeloIA").value,
                idioma: document.getElementById("idiomaAsistente").value,
                longitudRespuesta: document.getElementById(
                    "longitudRespuesta").value,
                formalidad: document.getElementById(
                    "formalidadAsistente").value,
                formatoRespuesta: document.getElementById(
                    "formatoRespuesta").value,
                restricciones: document.getElementById(
                    "restriccionesAsistente").value,
                mensajeBienvenida: document.getElementById(
                    "mensajeBienvenida").value,
                temperatura: Number(document.getElementById(
                    "temperaturaAsistente").value),
                maxTokens: Number(document.getElementById(
                    "maxTokensAsistente").value),
                timeoutSeconds: Number(document.getElementById(
                    "timeoutAsistente").value)
            })
        });

        mostrarMensajeConfiguracionIA(
            "Configuración del asistente actualizada correctamente.");

        await cargarConfiguracionIA();
    } catch (error) {
        mostrarMensajeConfiguracionIA(error.message, true);
    }
});

formNuevaVersionPrompt.addEventListener("submit", async event => {
    event.preventDefault();

    try {
        const idPrompt = document.getElementById("idPromptActivo").value;

        if (!idPrompt) {
            throw new Error(
                "No existe un prompt activo para crear una nueva versión.");
        }

        await solicitarConfiguracionIA(
            `/api/prompts/${idPrompt}/versiones`,
            {
                method: "POST",
                body: JSON.stringify({
                    nombre: document.getElementById("nombrePrompt").value,
                    contenido: document.getElementById(
                        "contenidoPrompt").value,
                    motivoCambio: document.getElementById(
                        "motivoCambioPrompt").value
                })
            });

        document.getElementById("motivoCambioPrompt").value = "";

        mostrarMensajeConfiguracionIA(
            "Nueva versión de prompt creada y activada correctamente.");

        await cargarConfiguracionIA();
    } catch (error) {
        mostrarMensajeConfiguracionIA(error.message, true);
    }
});

formProbarPrompt.addEventListener("submit", async event => {
    event.preventDefault();

    try {
        if (!asistenteActivo || !promptActivo) {
            throw new Error(
                "No hay asistente o prompt activo disponible para la prueba.");
        }

        const boton = formProbarPrompt.querySelector("button[type='submit']");
        boton.disabled = true;
        boton.textContent = "Consultando Ollama...";

        try {
            const respuesta = await solicitarConfiguracionIA(
                "/api/pruebas-prompts",
                {
                    method: "POST",
                    body: JSON.stringify({
                        idAsistente: asistenteActivo.idAsistente,
                        idPrompt: promptActivo.idPrompt,
                        mensaje: document.getElementById(
                            "mensajePrueba").value
                    })
                });

            document.getElementById("tiempoPruebaPrompt").textContent =
                respuesta.tiempoRespuestaMs;

            document.getElementById("promptGeneradoPrueba").textContent =
                respuesta.promptGenerado;

            document.getElementById("respuestaPruebaPrompt").textContent =
                respuesta.respuesta;

            resultadoPruebaPrompt.classList.remove("d-none");
            mostrarMensajeConfiguracionIA(
                "Prueba de prompt completada correctamente.");
        } finally {
            boton.disabled = false;
            boton.textContent = "Probar con Ollama";
        }
    } catch (error) {
        mostrarMensajeConfiguracionIA(error.message, true);
    }
});

cargarConfiguracionIA();