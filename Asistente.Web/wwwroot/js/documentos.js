const paginaDocumentos = document.getElementById("documentosPage");

const apiBaseUrlDocumentos = paginaDocumentos.dataset.apiBaseUrl
    .replace(/\/$/, "");

const puedeAdministrarDocumentos =
    paginaDocumentos.dataset.puedeAdministrar === "true";

const mensajeDocumentos = document.getElementById("mensajeDocumentos");
const formFiltrosDocumentos = document.getElementById(
    "formFiltrosDocumentos");

const terminoBusquedaDocumento = document.getElementById(
    "terminoBusquedaDocumento");

const filtroCategoriaDocumento = document.getElementById(
    "filtroCategoriaDocumento");

const filtroEstadoDocumento = document.getElementById(
    "filtroEstadoDocumento");

const fechaDesdeDocumento = document.getElementById(
    "fechaDesdeDocumento");

const fechaHastaDocumento = document.getElementById(
    "fechaHastaDocumento");

const btnLimpiarFiltrosDocumentos = document.getElementById(
    "btnLimpiarFiltrosDocumentos");

const btnActualizarDocumentos = document.getElementById(
    "btnActualizarDocumentos");

const documentosBody = document.getElementById("documentosBody");

const panelDetalleDocumento = document.getElementById(
    "panelDetalleDocumento");

const codigoDetalleDocumento = document.getElementById(
    "codigoDetalleDocumento");

const nombreDetalleDocumento = document.getElementById(
    "nombreDetalleDocumento");

const metadatosDetalleDocumento = document.getElementById(
    "metadatosDetalleDocumento");

const descripcionDetalleDocumento = document.getElementById(
    "descripcionDetalleDocumento");

const estadoProcesamientoDetalleDocumento = document.getElementById(
    "estadoProcesamientoDetalleDocumento");

const totalPaginasProcesamiento = document.getElementById(
    "totalPaginasProcesamiento");

const totalCaracteresProcesamiento = document.getElementById(
    "totalCaracteresProcesamiento");

const totalChunksProcesamiento = document.getElementById(
    "totalChunksProcesamiento");

const fechasProcesamiento = document.getElementById(
    "fechasProcesamiento");

const observacionesProcesamiento = document.getElementById(
    "observacionesProcesamiento");

const versionesDocumentoBody = document.getElementById(
    "versionesDocumentoBody");

const btnCerrarDetalleDocumento = document.getElementById(
    "btnCerrarDetalleDocumento");

const panelFormularioDocumento = document.getElementById(
    "panelFormularioDocumento");

const formDocumento = document.getElementById("formDocumento");

const idDocumentoEdicion = document.getElementById(
    "idDocumentoEdicion");

const codigoDocumento = document.getElementById("codigoDocumento");

const nombreDocumento = document.getElementById("nombreDocumento");

const descripcionDocumento = document.getElementById(
    "descripcionDocumento");

const categoriaDocumento = document.getElementById(
    "categoriaDocumento");

const archivoDocumento = document.getElementById("archivoDocumento");

const tituloFormularioDocumento = document.getElementById(
    "tituloFormularioDocumento");

const btnNuevoDocumento = document.getElementById(
    "btnNuevoDocumento");

const btnCancelarDocumento = document.getElementById(
    "btnCancelarDocumento");

const btnGuardarDocumento = document.getElementById(
    "btnGuardarDocumento");

const formNuevaVersionDocumento = document.getElementById(
    "formNuevaVersionDocumento");

const archivoNuevaVersion = document.getElementById(
    "archivoNuevaVersion");

const btnGuardarNuevaVersion = document.getElementById(
    "btnGuardarNuevaVersion");

const auditoriaDocumentoBody = document.getElementById(
    "auditoriaDocumentoBody");

let categorias = [];
let documentos = [];
let documentoSeleccionado = null;

function mostrarMensaje(mensaje, esError = false) {
    mensajeDocumentos.textContent = mensaje;
    mensajeDocumentos.className =
        `alert ${esError ? "alert-danger" : "alert-success"}`;
}

function ocultarMensaje() {
    mensajeDocumentos.textContent = "";
    mensajeDocumentos.className = "alert d-none";
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

function formatearTamano(tamanoArchivo) {
    if (tamanoArchivo < 1024 * 1024) {
        return `${Math.ceil(tamanoArchivo / 1024)} KB`;
    }

    return `${(tamanoArchivo / (1024 * 1024)).toFixed(2)} MB`;
}

function crearEtiquetaEstado(estado) {
    const clases = {
        Borrador: "bg-secondary text-white",
        Activo: "bg-success text-white",
        Archivado: "bg-warning text-dark",
        Eliminado: "bg-danger text-white"
    };

    return `
        <span class="badge ${clases[estado] ?? "text-bg-secondary"}">
            ${escaparHtml(estado)}
        </span>`;
}

function crearEtiquetaEstadoProcesamiento(estado) {
    const estilos = {
        PendienteProcesamiento: {
            clase: "bg-secondary text-white",
            texto: "Pendiente"
        },
        EnProceso: {
            clase: "bg-info text-dark",
            texto: "En proceso"
        },
        Procesado: {
            clase: "bg-success text-white",
            texto: "Procesado"
        },
        Error: {
            clase: "bg-danger text-white",
            texto: "Error"
        }
    };

    const opcion = estilos[estado] ?? {
        clase: "bg-secondary text-white",
        texto: estado || "No disponible"
    };

    return `
        <span class="badge ${opcion.clase}">
            ${escaparHtml(opcion.texto)}
        </span>`;
}

function formatearNumero(valor) {
    return new Intl.NumberFormat("es-PE").format(valor ?? 0);
}

function renderizarProcesamientoDocumento(procesamiento) {
    const datos = procesamiento ?? {};
    const estado = datos.estado || "PendienteProcesamiento";

    estadoProcesamientoDetalleDocumento.innerHTML =
        crearEtiquetaEstadoProcesamiento(estado);

    totalPaginasProcesamiento.textContent =
        formatearNumero(datos.totalPaginas);

    totalCaracteresProcesamiento.textContent =
        formatearNumero(datos.totalCaracteres);

    totalChunksProcesamiento.textContent =
        formatearNumero(datos.totalChunks);

    const inicio = datos.fechaInicio
        ? formatearFecha(datos.fechaInicio)
        : "No iniciado";

    const fin = datos.fechaFin
        ? formatearFecha(datos.fechaFin)
        : "Pendiente";

    fechasProcesamiento.textContent =
        `Inicio: ${inicio} · Fin: ${fin}`;

    observacionesProcesamiento.textContent =
        datos.observaciones || "Sin observaciones.";
}

async function obtenerMensajeError(response) {
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

    return mensaje;
}

async function solicitar(url, opciones = {}) {
    const tieneCuerpoJson =
        opciones.body &&
        !(opciones.body instanceof FormData);

    const response = await fetch(
        `${apiBaseUrlDocumentos}${url}`,
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
        throw new Error(await obtenerMensajeError(response));
    }

    if (response.status === 204) {
        return null;
    }

    const tipoContenido = response.headers.get("content-type") || "";

    return tipoContenido.includes("application/json")
        ? response.json()
        : null;
}

async function descargarVersion(idDocumento, version) {
    const response = await fetch(
        `${apiBaseUrlDocumentos}/api/documentos/${idDocumento}` +
        `/versiones/${version.idVersion}/descarga`,
        {
            credentials: "include"
        });

    if (!response.ok) {
        throw new Error(await obtenerMensajeError(response));
    }

    const archivo = await response.blob();
    const urlTemporal = URL.createObjectURL(archivo);
    const enlace = document.createElement("a");

    enlace.href = urlTemporal;
    enlace.download = version.nombreArchivo;

    document.body.appendChild(enlace);
    enlace.click();
    enlace.remove();

    URL.revokeObjectURL(urlTemporal);
}

function llenarSelectCategorias() {
    const valorFiltroActual = filtroCategoriaDocumento.value;
    const valorFormularioActual = categoriaDocumento?.value ?? "";

    const opciones = categorias.map(categoria => `
        <option value="${categoria.idCategoria}">
            ${escaparHtml(categoria.nombre)}
        </option>`).join("");

    filtroCategoriaDocumento.innerHTML = `
        <option value="">Todas las categorías</option>
        ${opciones}`;

    filtroCategoriaDocumento.value = valorFiltroActual;

    if (puedeAdministrarDocumentos) {
        categoriaDocumento.innerHTML = `
            <option value="">Selecciona una categoría</option>
            ${opciones}`;

        categoriaDocumento.value = valorFormularioActual;
    }
}

function crearFilaDocumento(documento) {
    let acciones = `
        <button type="button"
                class="btn btn-outline-primary btn-sm"
                data-accion="detalle"
                data-id="${documento.idDocumento}">
            Ver
        </button>`;

    if (puedeAdministrarDocumentos &&
        documento.estado !== "Eliminado") {
        acciones += `
            <button type="button"
                    class="btn btn-outline-secondary btn-sm ms-1"
                    data-accion="editar"
                    data-id="${documento.idDocumento}">
                Editar
            </button>`;
    }

    if (puedeAdministrarDocumentos &&
        (documento.estado === "Borrador" ||
            documento.estado === "Archivado")) {
        acciones += `
            <button type="button"
                    class="btn btn-outline-success btn-sm ms-1"
                    data-accion="activar"
                    data-id="${documento.idDocumento}">
                Activar
            </button>`;
    }

    if (puedeAdministrarDocumentos &&
        documento.estado === "Activo") {
        acciones += `
            <button type="button"
                    class="btn btn-outline-warning btn-sm ms-1"
                    data-accion="archivar"
                    data-id="${documento.idDocumento}">
                Archivar
            </button>`;
    }

    if (puedeAdministrarDocumentos &&
        documento.estado !== "Eliminado") {
        acciones += `
            <button type="button"
                    class="btn btn-outline-danger btn-sm ms-1"
                    data-accion="eliminar"
                    data-id="${documento.idDocumento}">
                Eliminar
            </button>`;
    }

    return `
        <tr>
            <td class="fw-semibold">
                ${escaparHtml(documento.codigo)}
            </td>
            <td>${escaparHtml(documento.nombre)}</td>
            <td>${escaparHtml(documento.categoria)}</td>
            <td>v${documento.versionActual}</td>
            <td>${crearEtiquetaEstado(documento.estado)}</td>
            <td>
                ${crearEtiquetaEstadoProcesamiento(
                    documento.estadoProcesamiento)}
            </td>
            <td>${formatearFecha(documento.fechaRegistro)}</td>
            <td class="text-end text-nowrap">${acciones}</td>
        </tr>`;
}

function renderizarDocumentos() {
    documentosBody.innerHTML = documentos.length === 0
        ? `
            <tr>
                <td colspan="7" class="text-center text-muted py-4">
                    No se encontraron documentos.
                </td>
            </tr>`
        : documentos.map(crearFilaDocumento).join("");
}

function construirConsultaDocumentos() {
    const parametros = new URLSearchParams();

    if (terminoBusquedaDocumento.value.trim()) {
        parametros.set(
            "terminoBusqueda",
            terminoBusquedaDocumento.value.trim());
    }

    if (filtroCategoriaDocumento.value) {
        parametros.set(
            "idCategoria",
            filtroCategoriaDocumento.value);
    }

    if (filtroEstadoDocumento.value) {
        parametros.set("estado", filtroEstadoDocumento.value);
    }

    if (fechaDesdeDocumento.value) {
        parametros.set("fechaDesde", fechaDesdeDocumento.value);
    }

    if (fechaHastaDocumento.value) {
        parametros.set("fechaHasta", fechaHastaDocumento.value);
    }

    const consulta = parametros.toString();

    return consulta
        ? `/api/documentos?${consulta}`
        : "/api/documentos";
}

async function cargarCategorias() {
    categorias = await solicitar(
        "/api/categorias-documentos?soloActivas=true");

    llenarSelectCategorias();
}

async function cargarDocumentos() {
    ocultarMensaje();

    try {
        documentos = await solicitar(construirConsultaDocumentos());
        renderizarDocumentos();
    } catch (error) {
        mostrarMensaje(error.message, true);
    }
}

function prepararNuevoDocumento() {
    formDocumento.reset();
    idDocumentoEdicion.value = "";
    codigoDocumento.disabled = false;
    archivoDocumento.disabled = false;
    archivoDocumento.required = true;

    tituloFormularioDocumento.textContent = "Nuevo documento";
    btnGuardarDocumento.textContent = "Registrar documento";

    panelFormularioDocumento.classList.remove("d-none");
    panelFormularioDocumento.scrollIntoView({
        behavior: "smooth",
        block: "start"
    });
}

function cerrarFormularioDocumento() {
    formDocumento.reset();
    idDocumentoEdicion.value = "";
    panelFormularioDocumento.classList.add("d-none");
}

function prepararEdicionDocumento(documento) {
    idDocumentoEdicion.value = documento.idDocumento;
    codigoDocumento.value = documento.codigo;
    codigoDocumento.disabled = true;

    nombreDocumento.value = documento.nombre;
    descripcionDocumento.value = documento.descripcion;
    categoriaDocumento.value = documento.idCategoria;

    archivoDocumento.value = "";
    archivoDocumento.required = false;
    archivoDocumento.disabled = true;

    tituloFormularioDocumento.textContent = "Editar documento";
    btnGuardarDocumento.textContent = "Guardar cambios";

    panelFormularioDocumento.classList.remove("d-none");
    panelFormularioDocumento.scrollIntoView({
        behavior: "smooth",
        block: "start"
    });
}

function renderizarVersiones(documento) {
    versionesDocumentoBody.innerHTML = documento.versiones.length === 0
        ? `
            <tr>
                <td colspan="7" class="text-center text-muted">
                    No hay versiones registradas.
                </td>
            </tr>`
        : documento.versiones.map(version => `
            <tr>
                <td>v${version.numeroVersion}</td>
                <td>${escaparHtml(version.nombreArchivo)}</td>
                <td>${formatearTamano(version.tamanoArchivo)}</td>
                <td>${formatearFecha(version.fechaCarga)}</td>
                <td>${escaparHtml(version.usuarioCarga)}</td>
                <td>
                    <span class="badge ${version.activo
                ? "bg-success text-white"
                : "bg-secondary text-white"}">
                        ${version.activo ? "Vigente" : "Histórica"}
                    </span>
                </td>
                <td class="text-end">
                    <button type="button"
                            class="btn btn-outline-primary btn-sm"
                            data-accion="descargar-version"
                            data-version="${version.idVersion}">
                        Descargar
                    </button>
                </td>
            </tr>`).join("");
}

async function cargarAuditoria(idDocumento) {
    if (!puedeAdministrarDocumentos) {
        return;
    }

    const actividades = await solicitar(
        `/api/documentos/${idDocumento}/auditoria`);

    auditoriaDocumentoBody.innerHTML = actividades.length === 0
        ? `
            <tr>
                <td colspan="5" class="text-center text-muted">
                    No hay actividades registradas.
                </td>
            </tr>`
        : actividades.map(actividad => `
            <tr>
                <td>${formatearFecha(actividad.fechaHora)}</td>
                <td>${escaparHtml(actividad.usuario)}</td>
                <td>${escaparHtml(actividad.accion)}</td>
                <td>${escaparHtml(actividad.descripcion)}</td>
                <td>${escaparHtml(actividad.direccionIP)}</td>
            </tr>`).join("");
}

async function cargarDetalleDocumento(idDocumento) {
    try {
        documentoSeleccionado = await solicitar(
            `/api/documentos/${idDocumento}`);

        codigoDetalleDocumento.textContent =
            documentoSeleccionado.codigo;

        nombreDetalleDocumento.textContent =
            documentoSeleccionado.nombre;

        metadatosDetalleDocumento.textContent =
            `Categoría: ${documentoSeleccionado.categoria} · ` +
            `Versión actual: ${documentoSeleccionado.versionActual} · ` +
            `Estado: ${documentoSeleccionado.estado}`;

        renderizarProcesamientoDocumento(
            documentoSeleccionado.procesamientoActual);

        descripcionDetalleDocumento.textContent =
            documentoSeleccionado.descripcion ||
            "Sin descripción registrada.";

        renderizarVersiones(documentoSeleccionado);

        panelDetalleDocumento.classList.remove("d-none");

        await cargarAuditoria(idDocumento);
    } catch (error) {
        mostrarMensaje(error.message, true);
    }
}

async function cambiarEstadoDocumento(
    documento,
    accion,
    mensajeExito) {
    await solicitar(
        `/api/documentos/${documento.idDocumento}/${accion}`,
        {
            method: "PATCH"
        });

    mostrarMensaje(mensajeExito);

    await cargarDocumentos();
    await cargarDetalleDocumento(documento.idDocumento);
}

async function eliminarDocumento(documento) {
    const confirmado = window.confirm(
        `¿Deseas eliminar lógicamente el documento ` +
        `'${documento.codigo}'?`);

    if (!confirmado) {
        return;
    }

    await solicitar(
        `/api/documentos/${documento.idDocumento}`,
        {
            method: "DELETE"
        });

    mostrarMensaje("Documento eliminado correctamente.");

    if (documentoSeleccionado?.idDocumento === documento.idDocumento) {
        panelDetalleDocumento.classList.add("d-none");
        documentoSeleccionado = null;
    }

    await cargarDocumentos();
}

formFiltrosDocumentos.addEventListener("submit", async event => {
    event.preventDefault();
    await cargarDocumentos();
});

btnLimpiarFiltrosDocumentos.addEventListener("click", async () => {
    formFiltrosDocumentos.reset();
    await cargarDocumentos();
});

btnActualizarDocumentos.addEventListener("click", async () => {
    await cargarDocumentos();

    if (documentoSeleccionado) {
        await cargarDetalleDocumento(
            documentoSeleccionado.idDocumento);
    }
});

documentosBody.addEventListener("click", async event => {
    const boton = event.target.closest("button[data-accion]");

    if (!boton) {
        return;
    }

    const idDocumento = Number(boton.dataset.id);

    const documento = documentos.find(item =>
        item.idDocumento === idDocumento);

    if (!documento) {
        return;
    }

    try {
        boton.disabled = true;

        switch (boton.dataset.accion) {
            case "detalle":
                await cargarDetalleDocumento(idDocumento);
                break;

            case "editar": {
                const detalle = await solicitar(
                    `/api/documentos/${idDocumento}`);

                prepararEdicionDocumento(detalle);
                break;
            }

            case "activar":
                await cambiarEstadoDocumento(
                    documento,
                    "activar",
                    "Documento activado correctamente.");
                break;

            case "archivar":
                await cambiarEstadoDocumento(
                    documento,
                    "archivar",
                    "Documento archivado correctamente.");
                break;

            case "eliminar":
                await eliminarDocumento(documento);
                break;
        }
    } catch (error) {
        mostrarMensaje(error.message, true);
    } finally {
        boton.disabled = false;
    }
});

versionesDocumentoBody.addEventListener("click", async event => {
    const boton = event.target.closest(
        "button[data-accion='descargar-version']");

    if (!boton || !documentoSeleccionado) {
        return;
    }

    const version = documentoSeleccionado.versiones.find(item =>
        item.idVersion === Number(boton.dataset.version));

    if (!version) {
        return;
    }

    try {
        boton.disabled = true;
        boton.textContent = "Descargando...";

        await descargarVersion(
            documentoSeleccionado.idDocumento,
            version);
    } catch (error) {
        mostrarMensaje(error.message, true);
    } finally {
        boton.disabled = false;
        boton.textContent = "Descargar";
    }
});

btnCerrarDetalleDocumento.addEventListener("click", () => {
    panelDetalleDocumento.classList.add("d-none");
    documentoSeleccionado = null;
});

if (puedeAdministrarDocumentos) {
    btnNuevoDocumento.addEventListener("click", () => {
        ocultarMensaje();
        prepararNuevoDocumento();
    });

    btnCancelarDocumento.addEventListener("click", () => {
        ocultarMensaje();
        cerrarFormularioDocumento();
    });

        formDocumento.addEventListener("submit", async event => {
            event.preventDefault();
            ocultarMensaje();

            const esEdicion = idDocumentoEdicion.value !== "";
            const idDocumentoActual = Number(idDocumentoEdicion.value);

            btnGuardarDocumento.disabled = true;
            btnGuardarDocumento.textContent = esEdicion
                ? "Guardando..."
                : "Registrando...";

            try {
                if (esEdicion) {
                    await solicitar(
                        `/api/documentos/${idDocumentoEdicion.value}`,
                        {
                            method: "PUT",
                            body: JSON.stringify({
                                nombre: nombreDocumento.value.trim(),
                                descripcion: descripcionDocumento.value.trim(),
                                idCategoria: Number(categoriaDocumento.value)
                            })
                        });

                    mostrarMensaje(
                        "Documento actualizado correctamente.");

                    cerrarFormularioDocumento();

                    await cargarDocumentos();
                    await cargarDetalleDocumento(idDocumentoActual);
                } else {
                    const archivo = archivoDocumento.files[0];

                    if (!archivo) {
                        throw new Error(
                            "Selecciona el archivo PDF del documento.");
                    }

                    const formulario = new FormData();

                    formulario.append(
                        "Codigo",
                        codigoDocumento.value.trim());

                    formulario.append(
                        "Nombre",
                        nombreDocumento.value.trim());

                    formulario.append(
                        "Descripcion",
                        descripcionDocumento.value.trim());

                    formulario.append(
                        "IdCategoria",
                        categoriaDocumento.value);

                    formulario.append("archivo", archivo);

                    const documento = await solicitar(
                        "/api/documentos",
                        {
                            method: "POST",
                            body: formulario
                        });

                    mostrarMensaje(
                        "Documento registrado correctamente. " +
                        "Puedes activarlo cuando esté listo.");

                    cerrarFormularioDocumento();

                    await cargarDocumentos();
                    await cargarDetalleDocumento(documento.idDocumento);
                }
            } catch (error) {
                mostrarMensaje(error.message, true);
            } finally {
                btnGuardarDocumento.disabled = false;

                btnGuardarDocumento.textContent =
                    idDocumentoEdicion.value !== ""
                        ? "Guardar cambios"
                        : "Registrar documento";
            }
        });

    formNuevaVersionDocumento.addEventListener(
        "submit",
        async event => {
            event.preventDefault();
            ocultarMensaje();

            if (!documentoSeleccionado) {
                return;
            }

            const archivo = archivoNuevaVersion.files[0];

            if (!archivo) {
                mostrarMensaje(
                    "Selecciona un PDF para registrar la nueva versión.",
                    true);

                return;
            }

            const formulario = new FormData();
            formulario.append("archivo", archivo);

            btnGuardarNuevaVersion.disabled = true;
            btnGuardarNuevaVersion.textContent = "Cargando...";

            try {
                await solicitar(
                    `/api/documentos/${documentoSeleccionado.idDocumento}` +
                    "/versiones",
                    {
                        method: "POST",
                        body: formulario
                    });

                formNuevaVersionDocumento.reset();

                mostrarMensaje(
                    "Nueva versión registrada correctamente.");

                await cargarDocumentos();
                await cargarDetalleDocumento(
                    documentoSeleccionado.idDocumento);
            } catch (error) {
                mostrarMensaje(error.message, true);
            } finally {
                btnGuardarNuevaVersion.disabled = false;
                btnGuardarNuevaVersion.textContent =
                    "Cargar versión";
            }
        });
}

async function iniciarPaginaDocumentos() {
    try {
        await cargarCategorias();
        await cargarDocumentos();
    } catch (error) {
        mostrarMensaje(error.message, true);
    }
}

iniciarPaginaDocumentos();