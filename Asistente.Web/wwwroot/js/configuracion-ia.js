const configuracionIAPage = document.getElementById("configuracionIAPage");
const apiBaseUrlConfiguracionIA =
    configuracionIAPage.dataset.apiBaseUrl.replace(/\/$/, "");

const mensajeConfiguracionIA = document.getElementById(
    "mensajeConfiguracionIA");

const selectorAsistente = document.getElementById("selectorAsistente");
const btnNuevoAsistente = document.getElementById("btnNuevoAsistente");
const btnCambiarEstadoAsistente = document.getElementById(
    "btnCambiarEstadoAsistente");

const estadoAsistenteSeleccionado = document.getElementById(
    "estadoAsistenteSeleccionado");

const formConfiguracionAsistente = document.getElementById(
    "formConfiguracionAsistente");

const btnGuardarAsistente = document.getElementById(
    "btnGuardarAsistente");

const tituloFormularioAsistente = document.getElementById(
    "tituloFormularioAsistente");

const formNuevaVersionPrompt = document.getElementById(
    "formNuevaVersionPrompt");

const formProbarPrompt = document.getElementById(
    "formProbarPrompt");

const selectorPromptPrueba = document.getElementById(
    "selectorPromptPrueba");

const btnCambiarEstadoPrompt = document.getElementById(
    "btnCambiarEstadoPrompt");

const estadoPromptSeleccionado = document.getElementById(
    "estadoPromptSeleccionado");

const historialPromptBody = document.getElementById(
    "historialPromptBody");

const resultadoPruebaPrompt = document.getElementById(
    "resultadoPruebaPrompt");

let asistentes = [];
let prompts = [];
let asistenteSeleccionado = null;
let promptActivo = null;
let promptSeleccionadoParaPrueba = null;
let modoCreacionAsistente = false;

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
    return fecha
        ? new Date(fecha).toLocaleString("es-PE")
        : "-";
}

function obtenerConfiguracionFormulario() {
    return {
        nombre: document.getElementById("nombreAsistente").value.trim(),
        descripcion: document.getElementById(
            "descripcionAsistente").value.trim(),
        modeloIA: document.getElementById("modeloIA").value.trim(),
        idioma: document.getElementById("idiomaAsistente").value.trim(),
        longitudRespuesta: document.getElementById(
            "longitudRespuesta").value.trim(),
        formalidad: document.getElementById(
            "formalidadAsistente").value.trim(),
        formatoRespuesta: document.getElementById(
            "formatoRespuesta").value.trim(),
        restricciones: document.getElementById(
            "restriccionesAsistente").value.trim(),
        mensajeBienvenida: document.getElementById(
            "mensajeBienvenida").value.trim(),
        temperatura: Number(document.getElementById(
            "temperaturaAsistente").value),
        maxTokens: Number(document.getElementById(
            "maxTokensAsistente").value),
        timeoutSeconds: Number(document.getElementById(
            "timeoutAsistente").value)
    };
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

function prepararNuevoAsistente() {
    modoCreacionAsistente = true;
    asistenteSeleccionado = null;
    promptActivo = null;
    prompts = [];

    document.getElementById("formConfiguracionAsistente").reset();
    document.getElementById("idAsistente").value = "";
    document.getElementById("modeloIA").value = "deepseek-r1:7b";
    document.getElementById("idiomaAsistente").value = "Español";
    document.getElementById("longitudRespuesta").value = "Breve y clara";
    document.getElementById("formalidadAsistente").value = "Profesional";
    document.getElementById("temperaturaAsistente").value = "0.4";
    document.getElementById("maxTokensAsistente").value = "1024";
    document.getElementById("timeoutAsistente").value = "180";

    tituloFormularioAsistente.textContent = "Nuevo asistente";
    btnGuardarAsistente.textContent = "Crear asistente";

    btnCambiarEstadoAsistente.disabled = true;
    btnCambiarEstadoAsistente.textContent = "Sin asistente";

    estadoAsistenteSeleccionado.textContent =
        "Completa los datos y guarda para crear el asistente.";

    document.getElementById("idPromptActivo").value = "";
    document.getElementById("nombrePrompt").value = "";
    document.getElementById("contenidoPrompt").value = "";
    document.getElementById("motivoCambioPrompt").value = "";

    selectorPromptPrueba.innerHTML =
        "<option>No hay prompts disponibles.</option>";

    btnCambiarEstadoPrompt.disabled = true;
    btnCambiarEstadoPrompt.textContent = "Sin prompt";
    estadoPromptSeleccionado.textContent = "";

    historialPromptBody.innerHTML = `
        <tr>
            <td colspan="5" class="text-center text-muted">
                Guarda el asistente para administrar sus prompts.
            </td>
        </tr>`;

    resultadoPruebaPrompt.classList.add("d-none");
}

function actualizarEstadoVisualAsistente() {
    if (!asistenteSeleccionado) {
        return;
    }

    const activo = asistenteSeleccionado.activo;

    btnCambiarEstadoAsistente.disabled = false;
    btnCambiarEstadoAsistente.textContent = activo
        ? "Desactivar asistente"
        : "Activar asistente";

    btnCambiarEstadoAsistente.className = activo
        ? "btn btn-outline-danger w-100"
        : "btn btn-outline-success w-100";

    estadoAsistenteSeleccionado.textContent = activo
        ? "Estado actual: Activo. Este asistente puede utilizarse en el chat."
        : "Estado actual: Inactivo.";
}

function llenarSelectorAsistentes() {
    selectorAsistente.innerHTML = asistentes.map(asistente => `
        <option value="${asistente.idAsistente}">
            ${escaparHtml(asistente.nombre)}
            ${asistente.activo ? " - Activo" : " - Inactivo"}
        </option>`).join("");
}

function llenarDatosPromptActivo() {
    if (!promptActivo) {
        document.getElementById("idPromptActivo").value = "";
        document.getElementById("nombrePrompt").value = "";
        document.getElementById("contenidoPrompt").value = "";
        return;
    }

    document.getElementById("idPromptActivo").value = promptActivo.idPrompt;
    document.getElementById("contenidoPrompt").value = promptActivo.contenido;
    document.getElementById("nombrePrompt").value =
        `${promptActivo.nombre} - versión ${promptActivo.version + 1}`;
}

function actualizarEstadoVisualPrompt() {
    if (!promptSeleccionadoParaPrueba) {
        btnCambiarEstadoPrompt.disabled = true;
        btnCambiarEstadoPrompt.textContent = "Sin prompt";
        estadoPromptSeleccionado.textContent = "";
        return;
    }

    const activo = promptSeleccionadoParaPrueba.activo;

    btnCambiarEstadoPrompt.disabled = false;
    btnCambiarEstadoPrompt.textContent = activo
        ? "Desactivar prompt"
        : "Activar prompt";

    btnCambiarEstadoPrompt.className = activo
        ? "btn btn-outline-danger w-100"
        : "btn btn-outline-success w-100";

    estadoPromptSeleccionado.textContent = activo
        ? "Prompt seleccionado: Activo"
        : "Prompt seleccionado: Inactivo";
}

function llenarSelectorPrompts() {
    selectorPromptPrueba.innerHTML = prompts.length === 0
        ? "<option value=''>No hay prompts disponibles.</option>"
        : prompts.map(prompt => `
            <option value="${prompt.idPrompt}">
                Versión ${prompt.version} - ${escaparHtml(prompt.nombre)}
                ${prompt.activo ? " - Activa" : " - Inactiva"}
            </option>`).join("");

    if (promptActivo) {
        selectorPromptPrueba.value = promptActivo.idPrompt;
        promptSeleccionadoParaPrueba = promptActivo;
    } else {
        promptSeleccionadoParaPrueba = null;
    }

    actualizarEstadoVisualPrompt();
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
    if (!asistenteSeleccionado) {
        return;
    }

    const historial = await solicitarConfiguracionIA(
        `/api/asistentes/${asistenteSeleccionado.idAsistente}/prompts/historial`);

    historialPromptBody.innerHTML = historial.length === 0
        ? `<tr>
               <td colspan="5" class="text-center text-muted">
                   No hay versiones registradas.
               </td>
           </tr>`
        : historial.map(crearFilaHistorial).join("");
}

async function cargarPrompts() {
    prompts = await solicitarConfiguracionIA(
        `/api/asistentes/${asistenteSeleccionado.idAsistente}/prompts`);

    promptActivo = prompts.find(prompt => prompt.activo) ?? null;

    llenarDatosPromptActivo();
    llenarSelectorPrompts();

    if (!promptActivo) {
        mostrarMensajeConfiguracionIA(
            "El asistente seleccionado no tiene un prompt activo. Puedes crear el prompt inicial.",
            true);
    }
}

async function seleccionarAsistente(idAsistente) {
    asistenteSeleccionado = asistentes.find(
        asistente => asistente.idAsistente === idAsistente) ?? null;

    if (!asistenteSeleccionado) {
        return;
    }

    modoCreacionAsistente = false;
    tituloFormularioAsistente.textContent =
        "Configuración del asistente";
    btnGuardarAsistente.textContent = "Guardar configuración";

    selectorAsistente.value = asistenteSeleccionado.idAsistente;
    llenarFormularioAsistente(asistenteSeleccionado);
    actualizarEstadoVisualAsistente();

    await cargarPrompts();
    await cargarHistorialPrompt();

    resultadoPruebaPrompt.classList.add("d-none");
}

async function cargarConfiguracionIA(idPreferido = null) {
    ocultarMensajeConfiguracionIA();

    try {
        asistentes = await solicitarConfiguracionIA("/api/asistentes");

        if (asistentes.length === 0) {
            prepararNuevoAsistente();
            return;
        }

        llenarSelectorAsistentes();

        const idSeleccionado = idPreferido
            ?? asistenteSeleccionado?.idAsistente
            ?? asistentes.find(asistente => asistente.activo)?.idAsistente
            ?? asistentes[0].idAsistente;

        await seleccionarAsistente(Number(idSeleccionado));
    } catch (error) {
        mostrarMensajeConfiguracionIA(error.message, true);
    }
}

selectorAsistente.addEventListener("change", async event => {
    try {
        await seleccionarAsistente(Number(event.target.value));
    } catch (error) {
        mostrarMensajeConfiguracionIA(error.message, true);
    }
});

btnNuevoAsistente.addEventListener("click", () => {
    ocultarMensajeConfiguracionIA();
    prepararNuevoAsistente();
});

formConfiguracionAsistente.addEventListener("submit", async event => {
    event.preventDefault();

    try {
        const configuracion = obtenerConfiguracionFormulario();

        if (modoCreacionAsistente) {
            const respuesta = await solicitarConfiguracionIA(
                "/api/asistentes",
                {
                    method: "POST",
                    body: JSON.stringify(configuracion)
                });

            mostrarMensajeConfiguracionIA(
                "Asistente creado correctamente. Ahora puedes crear su prompt inicial.");

            await cargarConfiguracionIA(respuesta.idAsistente);
            return;
        }

        if (!asistenteSeleccionado) {
            throw new Error("Selecciona un asistente antes de guardar.");
        }

        await solicitarConfiguracionIA(
            `/api/asistentes/${asistenteSeleccionado.idAsistente}`,
            {
                method: "PUT",
                body: JSON.stringify(configuracion)
            });

        mostrarMensajeConfiguracionIA(
            "Configuración del asistente actualizada correctamente.");

        await cargarConfiguracionIA(asistenteSeleccionado.idAsistente);
    } catch (error) {
        mostrarMensajeConfiguracionIA(error.message, true);
    }
});

btnCambiarEstadoAsistente.addEventListener("click", async () => {
    try {
        if (!asistenteSeleccionado) {
            return;
        }

        const nuevoEstado = !asistenteSeleccionado.activo;

        await solicitarConfiguracionIA(
            `/api/asistentes/${asistenteSeleccionado.idAsistente}/estado?activo=${nuevoEstado}`,
            { method: "PATCH" });

        mostrarMensajeConfiguracionIA(
            nuevoEstado
                ? "Asistente activado correctamente."
                : "Asistente desactivado correctamente.");

        await cargarConfiguracionIA(asistenteSeleccionado.idAsistente);
    } catch (error) {
        mostrarMensajeConfiguracionIA(error.message, true);
    }
});

formNuevaVersionPrompt.addEventListener("submit", async event => {
    event.preventDefault();

    try {
        if (!asistenteSeleccionado) {
            throw new Error("Primero selecciona o crea un asistente.");
        }

        const nombre = document.getElementById("nombrePrompt").value.trim();
        const contenido = document.getElementById(
            "contenidoPrompt").value.trim();

        const motivoCambio = document.getElementById(
            "motivoCambioPrompt").value.trim();

        if (!promptActivo) {
            await solicitarConfiguracionIA(
                `/api/asistentes/${asistenteSeleccionado.idAsistente}/prompts`,
                {
                    method: "POST",
                    body: JSON.stringify({ nombre, contenido })
                });

            mostrarMensajeConfiguracionIA(
                "Prompt inicial creado y activado correctamente.");
        } else {
            await solicitarConfiguracionIA(
                `/api/prompts/${promptActivo.idPrompt}/versiones`,
                {
                    method: "POST",
                    body: JSON.stringify({
                        nombre,
                        contenido,
                        motivoCambio
                    })
                });

            mostrarMensajeConfiguracionIA(
                "Nueva versión de prompt creada y activada correctamente.");
        }

        document.getElementById("motivoCambioPrompt").value = "";

        await seleccionarAsistente(asistenteSeleccionado.idAsistente);
    } catch (error) {
        mostrarMensajeConfiguracionIA(error.message, true);
    }
});

selectorPromptPrueba.addEventListener("change", event => {
    promptSeleccionadoParaPrueba = prompts.find(
        prompt => prompt.idPrompt === Number(event.target.value)) ?? null;

    actualizarEstadoVisualPrompt();
    resultadoPruebaPrompt.classList.add("d-none");
});

btnCambiarEstadoPrompt.addEventListener("click", async () => {
    try {
        if (!promptSeleccionadoParaPrueba) {
            return;
        }

        const nuevoEstado = !promptSeleccionadoParaPrueba.activo;

        await solicitarConfiguracionIA(
            `/api/prompts/${promptSeleccionadoParaPrueba.idPrompt}/estado?activo=${nuevoEstado}`,
            { method: "PATCH" });

        mostrarMensajeConfiguracionIA(
            nuevoEstado
                ? "Prompt activado correctamente."
                : "Prompt desactivado correctamente.");

        await seleccionarAsistente(asistenteSeleccionado.idAsistente);
    } catch (error) {
        mostrarMensajeConfiguracionIA(error.message, true);
    }
});

formProbarPrompt.addEventListener("submit", async event => {
    event.preventDefault();

    try {
        if (!asistenteSeleccionado || !promptSeleccionadoParaPrueba) {
            throw new Error(
                "Selecciona un asistente y una versión de prompt para la prueba.");
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
                        idAsistente: asistenteSeleccionado.idAsistente,
                        idPrompt: promptSeleccionadoParaPrueba.idPrompt,
                        mensaje: document.getElementById(
                            "mensajePrueba").value.trim()
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