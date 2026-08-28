const paginaDepuracion = document.getElementById(
    "depuracionContextoPage");

const apiBaseUrlDepuracion = paginaDepuracion.dataset.apiBaseUrl
    .replace(/\/$/, "");

const selectorConversacion = document.getElementById(
    "selectorConversacionDepuracion");

const btnActualizar = document.getElementById(
    "btnActualizarDepuracion");

const mensajeDepuracion = document.getElementById(
    "mensajeDepuracion");

const contenidoDepuracion = document.getElementById(
    "contenidoDepuracion");

function mostrarMensaje(mensaje, esError = false) {
    mensajeDepuracion.textContent = mensaje;
    mensajeDepuracion.className =
        `alert ${esError ? "alert-danger" : "alert-success"}`;
}

function ocultarMensaje() {
    mensajeDepuracion.textContent = "";
    mensajeDepuracion.className = "alert d-none";
}

async function solicitar(url) {
    const response = await fetch(
        `${apiBaseUrlDepuracion}${url}`,
        {
            credentials: "include"
        });

    if (!response.ok) {
        let mensaje = "No fue posible obtener la información.";

        try {
            const error = await response.json();
            mensaje = error.mensaje || mensaje;
        } catch {
            // La respuesta no contiene JSON.
        }

        throw new Error(mensaje);
    }

    return response.json();
}

function crearMensajeContexto(mensaje) {
    const contenedor = document.createElement("article");
    contenedor.className = "border rounded p-3 bg-light";

    const rol = document.createElement("p");
    rol.className = "fw-semibold text-primary mb-2";
    rol.textContent = mensaje.rol;

    const contenido = document.createElement("p");
    contenido.className = "mb-0";
    contenido.style.whiteSpace = "pre-wrap";
    contenido.textContent = mensaje.contenido;

    contenedor.appendChild(rol);
    contenedor.appendChild(contenido);

    return contenedor;
}

function mostrarContexto(contexto) {
    document.getElementById("cantidadMensajesContexto").textContent =
        contexto.cantidadMensajesContexto;

    document.getElementById("cantidadMensajesEnviados").textContent =
        contexto.cantidadMensajesEnviados;

    document.getElementById("tokensEstimados").textContent =
        contexto.tokensEstimados;

    document.getElementById("tiempoConstruccion").textContent =
        contexto.tiempoConstruccionMs;

    document.getElementById("tituloConversacionDepuracion").textContent =
        contexto.tituloConversacion;

    document.getElementById("modeloIADepuracion").textContent =
        contexto.modeloIA;

    document.getElementById("resumenContextoDepuracion").textContent =
        contexto.resumenContexto ||
        "La conversación todavía no tiene un resumen generado.";

    document.getElementById("promptFinalDepuracion").textContent =
        contexto.promptFinal;

    const mensajesContenedor = document.getElementById(
        "mensajesContextoDepuracion");

    mensajesContenedor.innerHTML = "";

    contexto.mensajesContexto.forEach(mensaje => {
        mensajesContenedor.appendChild(
            crearMensajeContexto(mensaje));
    });

    contenidoDepuracion.classList.remove("d-none");
}

async function cargarContextoSeleccionado() {
    const idConversacion = Number(selectorConversacion.value);

    if (!idConversacion) {
        contenidoDepuracion.classList.add("d-none");
        return;
    }

    ocultarMensaje();
    btnActualizar.disabled = true;
    btnActualizar.textContent = "Construyendo...";

    try {
        const contexto = await solicitar(
            `/api/depuracion-contexto/conversaciones/${idConversacion}`);

        mostrarContexto(contexto);
    } catch (error) {
        contenidoDepuracion.classList.add("d-none");
        mostrarMensaje(error.message, true);
    } finally {
        btnActualizar.disabled = false;
        btnActualizar.textContent = "Actualizar contexto";
    }
}

async function cargarConversaciones() {
    try {
        const conversaciones = await solicitar(
            "/api/conversaciones?incluirArchivadas=true&cantidadMaxima=100");

        if (conversaciones.length === 0) {
            selectorConversacion.innerHTML =
                "<option value=''>No hay conversaciones disponibles.</option>";

            selectorConversacion.disabled = true;
            btnActualizar.disabled = true;

            return;
        }

        selectorConversacion.innerHTML = conversaciones
            .map(conversacion => `
                <option value="${conversacion.idConversacion}">
                    #${conversacion.idConversacion} -
                    ${conversacion.titulo}
                    (${conversacion.totalMensajes} mensajes)
                </option>`)
            .join("");

        await cargarContextoSeleccionado();
    } catch (error) {
        selectorConversacion.innerHTML =
            "<option value=''>No fue posible cargar conversaciones.</option>";

        selectorConversacion.disabled = true;
        btnActualizar.disabled = true;

        mostrarMensaje(error.message, true);
    }
}

selectorConversacion.addEventListener(
    "change",
    cargarContextoSeleccionado);

btnActualizar.addEventListener(
    "click",
    cargarContextoSeleccionado);

cargarConversaciones();