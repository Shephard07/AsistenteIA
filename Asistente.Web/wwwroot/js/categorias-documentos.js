const paginaCategoriasDocumentos = document.getElementById(
    "categoriasDocumentosPage");

const apiBaseUrlCategoriasDocumentos =
    paginaCategoriasDocumentos.dataset.apiBaseUrl
        .replace(/\/$/, "");

const formCategoriaDocumento = document.getElementById(
    "formCategoriaDocumento");

const idCategoriaDocumento = document.getElementById(
    "idCategoriaDocumento");

const nombreCategoriaDocumento = document.getElementById(
    "nombreCategoriaDocumento");

const descripcionCategoriaDocumento = document.getElementById(
    "descripcionCategoriaDocumento");

const tituloFormularioCategoria = document.getElementById(
    "tituloFormularioCategoria");

const btnGuardarCategoria = document.getElementById(
    "btnGuardarCategoria");

const btnCancelarCategoria = document.getElementById(
    "btnCancelarCategoria");

const btnActualizarCategorias = document.getElementById(
    "btnActualizarCategorias");

const categoriasDocumentosBody = document.getElementById(
    "categoriasDocumentosBody");

const mensajeCategoriasDocumentos = document.getElementById(
    "mensajeCategoriasDocumentos");

let categorias = [];

function mostrarMensaje(mensaje, esError = false) {
    mensajeCategoriasDocumentos.textContent = mensaje;
    mensajeCategoriasDocumentos.className =
        `alert ${esError ? "alert-danger" : "alert-success"}`;
}

function ocultarMensaje() {
    mensajeCategoriasDocumentos.textContent = "";
    mensajeCategoriasDocumentos.className = "alert d-none";
}

function escaparHtml(valor) {
    const elemento = document.createElement("div");
    elemento.textContent = valor ?? "";
    return elemento.innerHTML;
}

function formatearFecha(fecha) {
    return new Intl.DateTimeFormat("es-PE", {
        dateStyle: "short",
        timeStyle: "short"
    }).format(new Date(fecha));
}

async function solicitar(url, opciones = {}) {
    const tieneCuerpoJson =
        opciones.body &&
        !(opciones.body instanceof FormData);

    const response = await fetch(
        `${apiBaseUrlCategoriasDocumentos}${url}`,
        {
            credentials: "include",
            ...opciones,
            headers: {
                ...(tieneCuerpoJson
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

    if (response.status === 204) {
        return null;
    }

    const tipoContenido = response.headers.get("content-type") || "";

    return tipoContenido.includes("application/json")
        ? response.json()
        : null;
}

function crearFilaCategoria(categoria) {
    const estado = categoria.activo
        ? "<span class='badge text-bg-success'>Activa</span>"
        : "<span class='badge text-bg-secondary'>Inactiva</span>";

    const textoAccion = categoria.activo
        ? "Desactivar"
        : "Activar";

    const claseAccion = categoria.activo
        ? "btn-outline-danger"
        : "btn-outline-success";

    return `
        <tr>
            <td class="fw-semibold">
                ${escaparHtml(categoria.nombre)}
            </td>
            <td>${escaparHtml(categoria.descripcion)}</td>
            <td>${estado}</td>
            <td>${formatearFecha(categoria.fechaCreacion)}</td>
            <td class="text-end text-nowrap">
                <button type="button"
                        class="btn btn-outline-primary btn-sm me-1"
                        data-accion="editar"
                        data-id="${categoria.idCategoria}">
                    Editar
                </button>

                <button type="button"
                        class="btn ${claseAccion} btn-sm"
                        data-accion="estado"
                        data-id="${categoria.idCategoria}">
                    ${textoAccion}
                </button>
            </td>
        </tr>`;
}

function renderizarCategorias() {
    categoriasDocumentosBody.innerHTML = categorias.length === 0
        ? `
            <tr>
                <td colspan="5" class="text-center text-muted py-4">
                    No hay categorías registradas.
                </td>
            </tr>`
        : categorias.map(crearFilaCategoria).join("");
}

function prepararNuevaCategoria() {
    formCategoriaDocumento.reset();
    idCategoriaDocumento.value = "";
    tituloFormularioCategoria.textContent = "Nueva categoría";
    btnGuardarCategoria.textContent = "Crear categoría";
    btnCancelarCategoria.classList.add("d-none");
    nombreCategoriaDocumento.focus();
}

function prepararEdicion(categoria) {
    idCategoriaDocumento.value = categoria.idCategoria;
    nombreCategoriaDocumento.value = categoria.nombre;
    descripcionCategoriaDocumento.value = categoria.descripcion;
    tituloFormularioCategoria.textContent = "Editar categoría";
    btnGuardarCategoria.textContent = "Guardar cambios";
    btnCancelarCategoria.classList.remove("d-none");
    nombreCategoriaDocumento.focus();
}

async function cargarCategorias() {
    ocultarMensaje();

    try {
        categorias = await solicitar(
            "/api/categorias-documentos?soloActivas=false");

        renderizarCategorias();
    } catch (error) {
        mostrarMensaje(error.message, true);
    }
}

formCategoriaDocumento.addEventListener("submit", async event => {
    event.preventDefault();
    ocultarMensaje();

    const solicitud = {
        nombre: nombreCategoriaDocumento.value.trim(),
        descripcion: descripcionCategoriaDocumento.value.trim()
    };

    const esEdicion = idCategoriaDocumento.value !== "";

    btnGuardarCategoria.disabled = true;
    btnGuardarCategoria.textContent = esEdicion
        ? "Guardando..."
        : "Creando...";

    try {
        if (esEdicion) {
            await solicitar(
                `/api/categorias-documentos/${idCategoriaDocumento.value}`,
                {
                    method: "PUT",
                    body: JSON.stringify(solicitud)
                });

            mostrarMensaje(
                "Categoría actualizada correctamente.");
        } else {
            await solicitar("/api/categorias-documentos", {
                method: "POST",
                body: JSON.stringify(solicitud)
            });

            mostrarMensaje(
                "Categoría creada correctamente.");
        }

        prepararNuevaCategoria();
        await cargarCategorias();
    } catch (error) {
        mostrarMensaje(error.message, true);
    } finally {
        btnGuardarCategoria.disabled = false;
        btnGuardarCategoria.textContent =
            idCategoriaDocumento.value !== ""
                ? "Guardar cambios"
                : "Crear categoría";
    }
});

categoriasDocumentosBody.addEventListener("click", async event => {
    const boton = event.target.closest("button[data-accion]");

    if (!boton) {
        return;
    }

    const idCategoria = Number(boton.dataset.id);

    const categoria = categorias.find(item =>
        item.idCategoria === idCategoria);

    if (!categoria) {
        return;
    }

    if (boton.dataset.accion === "editar") {
        ocultarMensaje();
        prepararEdicion(categoria);
        return;
    }

    try {
        const nuevoEstado = !categoria.activo;

        boton.disabled = true;

        await solicitar(
            `/api/categorias-documentos/${idCategoria}/estado?activo=${nuevoEstado}`,
            {
                method: "PATCH"
            });

        mostrarMensaje(
            nuevoEstado
                ? "Categoría activada correctamente."
                : "Categoría desactivada correctamente.");

        await cargarCategorias();
    } catch (error) {
        mostrarMensaje(error.message, true);
    } finally {
        boton.disabled = false;
    }
});

btnCancelarCategoria.addEventListener("click", () => {
    ocultarMensaje();
    prepararNuevaCategoria();
});

btnActualizarCategorias.addEventListener("click", cargarCategorias);

prepararNuevaCategoria();
cargarCategorias();