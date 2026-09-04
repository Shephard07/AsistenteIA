const paginaRag = document.getElementById("ragPage");

const apiBaseUrlRag = paginaRag.dataset.apiBaseUrl.replace(/\/$/, "");
const mensajeRag = document.getElementById("mensajeRag");
const contenidoRag = document.getElementById("contenidoRag");
const btnActualizarRag = document.getElementById("btnActualizarRag");
const formularioConfiguracionRag = document.getElementById(
    "formConfiguracionRag");

const btnGuardarConfiguracionRag = document.getElementById(
    "btnGuardarConfiguracionRag");
function mostrarMensaje(mensaje, esError = false) {
    mensajeRag.textContent = mensaje;
    mensajeRag.className =
        `alert ${esError ? "alert-danger" : "alert-success"}`;
}

function ocultarMensaje() {
    mensajeRag.textContent = "";
    mensajeRag.className = "alert d-none";
}

async function solicitar(url, opciones = {}) {
    const response = await fetch(
        `${apiBaseUrlRag}${url}`,
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
        let mensaje = "No fue posible completar la operación.";

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

    return response.status === 204
        ? null
        : response.json();
}

function formatearFecha(fecha) {
    if (!fecha) {
        return "Sin registro";
    }

    return new Intl.DateTimeFormat("es-PE", {
        dateStyle: "short",
        timeStyle: "short"
    }).format(new Date(fecha));
}

function obtenerClaseEstado(estado) {
    switch (estado) {
        case "Indexado":
            return "bg-success";
        case "EnProceso":
            return "bg-warning text-dark";
        case "Error":
            return "bg-danger";
        default:
            return "bg-secondary";
    }
}

function crearCelda(texto) {
    const celda = document.createElement("td");
    celda.textContent = texto;
    return celda;
}

function crearFilaDocumento(documento) {
    const fila = document.createElement("tr");

    const documentoCelda = document.createElement("td");

    const codigo = document.createElement("div");
    codigo.className = "fw-semibold";
    codigo.textContent = documento.codigoDocumento;

    const nombre = document.createElement("small");
    nombre.className = "text-muted";
    nombre.textContent = documento.nombreDocumento;

    documentoCelda.appendChild(codigo);
    documentoCelda.appendChild(nombre);

    fila.appendChild(documentoCelda);
    fila.appendChild(crearCelda(`v${documento.numeroVersion}`));

    const estadoCelda = document.createElement("td");
    const estado = document.createElement("span");
    estado.className =
        `badge ${obtenerClaseEstado(documento.estado)}`;
    estado.textContent = documento.estado;
    estadoCelda.appendChild(estado);

    if (documento.observaciones) {
        const observacion = document.createElement("small");
        observacion.className = "d-block text-danger mt-1";
        observacion.textContent = documento.observaciones;
        estadoCelda.appendChild(observacion);
    }

    fila.appendChild(estadoCelda);
    fila.appendChild(crearCelda(documento.totalChunks));
    fila.appendChild(crearCelda(documento.totalEmbeddings));
    fila.appendChild(crearCelda(
        formatearFecha(documento.fechaIndexacion)));

    const accionCelda = document.createElement("td");
    accionCelda.className = "text-end";

    const boton = document.createElement("button");
    boton.type = "button";
    boton.className = "btn btn-sm btn-outline-primary";
    boton.textContent = "Reindexar";

    const puedeReindexar =
        documento.estado === "Indexado" ||
        documento.estado === "Error";

    boton.disabled = !puedeReindexar;

    if (!puedeReindexar) {
        boton.title = "El documento ya está pendiente o en proceso.";
    }

    boton.addEventListener("click", () =>
        solicitarReindexacion(documento, boton));

    accionCelda.appendChild(boton);
    fila.appendChild(accionCelda);

    return fila;
}

function mostrarEstado(estado) {
    document.getElementById("totalDocumentosRag").textContent =
        estado.totalDocumentos;

    document.getElementById("totalIndexadosRag").textContent =
        estado.totalIndexados;

    document.getElementById("totalPendientesRag").textContent =
        estado.totalPendientes + estado.totalEnProceso;

    document.getElementById("totalErroresRag").textContent =
        estado.totalConError;

    document.getElementById("totalChunksRag").textContent =
        estado.totalChunks;

    document.getElementById("totalEmbeddingsRag").textContent =
        estado.totalEmbeddings;

    document.getElementById("tiempoPromedioRag").textContent =
        `${estado.tiempoPromedioIndexacionSegundos} s`;

    const estadoBaseVectorial = document.getElementById(
        "estadoBaseVectorialRag");

    estadoBaseVectorial.textContent =
        estado.baseVectorialDisponible
            ? "Disponible"
            : "No disponible";

    estadoBaseVectorial.className = estado.baseVectorialDisponible
        ? "h5 mb-0 text-success"
        : "h5 mb-0 text-danger";

    document.getElementById("proveedorRag").textContent =
        estado.configuracion.proveedor;

    document.getElementById("modeloEmbeddingsRag").value =
        estado.configuracion.modeloEmbeddings;

    document.getElementById("baseVectorialRag").textContent =
        estado.configuracion.baseVectorial;

    document.getElementById("cantidadResultadosRag").value =
        estado.configuracion.cantidadResultados;

    document.getElementById("puntajeMinimoRag").value =
        estado.configuracion.puntajeMinimo;

    document.getElementById("longitudContextoRag").value =
        estado.configuracion.longitudMaximaContexto;

    const tabla = document.getElementById("tablaDocumentosRag");
    tabla.innerHTML = "";

    if (estado.documentos.length === 0) {
        const fila = document.createElement("tr");
        const celda = document.createElement("td");

        celda.colSpan = 7;
        celda.className = "text-center text-muted py-4";
        celda.textContent =
            "No existen documentos procesados para indexar.";

        fila.appendChild(celda);
        tabla.appendChild(fila);
    } else {
        estado.documentos.forEach(documento => {
            tabla.appendChild(crearFilaDocumento(documento));
        });
    }

    contenidoRag.classList.remove("d-none");
}

async function cargarEstado(mensajeExito = null) {
    ocultarMensaje();
    btnActualizarRag.disabled = true;
    btnActualizarRag.textContent = "Actualizando...";

    try {
        const estado = await solicitar("/api/rag/estado");
        mostrarEstado(estado);

        if (mensajeExito) {
            mostrarMensaje(mensajeExito);
        }
    } catch (error) {
        contenidoRag.classList.add("d-none");
        mostrarMensaje(error.message, true);
    } finally {
        btnActualizarRag.disabled = false;
        btnActualizarRag.textContent = "Actualizar estado";
    }
}

async function solicitarReindexacion(documento, boton) {
    const confirmado = confirm(
        `¿Deseas reindexar "${documento.nombreDocumento}"? ` +
        "Se reemplazarán sus vectores actuales.");

    if (!confirmado) {
        return;
    }

    ocultarMensaje();
    boton.disabled = true;
    boton.textContent = "Solicitando...";

    try {
        await solicitar(
            `/api/rag/documentos/${documento.idDocumento}/reindexar`,
            { method: "POST" });

        await cargarEstado(
            "Solicitud de reindexación registrada. " +
            "El estado se actualizará automáticamente.");

        window.setTimeout(() => {
            cargarEstado("Estado de indexación actualizado.");
        }, 25000);
    } catch (error) {
        boton.disabled = false;
        boton.textContent = "Reindexar";
        mostrarMensaje(error.message, true);
    }
}

formularioConfiguracionRag.addEventListener(
    "submit",
    async event => {
        event.preventDefault();
        ocultarMensaje();

        const solicitud = {
            modeloEmbeddings: document.getElementById(
                "modeloEmbeddingsRag").value.trim(),

            cantidadResultados: Number(document.getElementById(
                "cantidadResultadosRag").value),

            puntajeMinimo: Number(document.getElementById(
                "puntajeMinimoRag").value),

            longitudMaximaContexto: Number(document.getElementById(
                "longitudContextoRag").value)
        };

        btnGuardarConfiguracionRag.disabled = true;
        btnGuardarConfiguracionRag.textContent = "Guardando...";

        try {
            await solicitar("/api/rag/configuracion", {
                method: "PUT",
                body: JSON.stringify(solicitud)
            });

            await cargarEstado(
                "Configuración RAG actualizada correctamente.");
        } catch (error) {
            mostrarMensaje(error.message, true);
        } finally {
            btnGuardarConfiguracionRag.disabled = false;
            btnGuardarConfiguracionRag.textContent =
                "Guardar configuración RAG";
        }
    });

btnActualizarRag.addEventListener("click", () => cargarEstado());

cargarEstado();