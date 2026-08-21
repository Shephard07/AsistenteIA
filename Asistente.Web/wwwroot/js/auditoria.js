const auditoriaPage = document.getElementById("auditoriaPage");
const apiBaseUrlAuditoria = auditoriaPage.dataset.apiBaseUrl.replace(/\/$/, "");
const sesionesBody = document.getElementById("sesionesBody");
const actividadesBody = document.getElementById("actividadesBody");
const mensajeAuditoria = document.getElementById("mensajeAuditoria");
const actualizarAuditoria = document.getElementById("actualizarAuditoria");

function formatearFecha(fecha) {
    if (!fecha) return "—";

    return new Date(fecha).toLocaleString("es-PE");
}

function mostrarMensajeAuditoria(mensaje, esError = false) {
    mensajeAuditoria.textContent = mensaje;
    mensajeAuditoria.className =
        `alert ${esError ? "alert-danger" : "alert-success"}`;
}

async function solicitarAuditoria(url) {
    const response = await fetch(`${apiBaseUrlAuditoria}${url}`, {
        credentials: "include",
        headers: {
            "Content-Type": "application/json"
        }
    });

    if (!response.ok) {
        let mensaje = "No fue posible consultar la auditoría.";

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

function renderizarSesiones(sesiones) {
    sesionesBody.innerHTML = sesiones.length === 0
        ? `<tr><td colspan="5" class="text-center text-muted">
               No hay sesiones registradas.
           </td></tr>`
        : sesiones.map(sesion => `
            <tr>
                <td>${sesion.usuario}</td>
                <td>${formatearFecha(sesion.fechaInicio)}</td>
                <td>${formatearFecha(sesion.fechaFin)}</td>
                <td>${sesion.direccionIP}</td>
                <td>
                    <span class="badge ${sesion.estado === "Activa"
                ? "text-bg-success"
                : "text-bg-secondary"}">
                        ${sesion.estado}
                    </span>
                </td>
            </tr>`).join("");
}

function renderizarActividades(actividades) {
    actividadesBody.innerHTML = actividades.length === 0
        ? `<tr><td colspan="6" class="text-center text-muted">
               No hay actividades registradas.
           </td></tr>`
        : actividades.map(actividad => `
            <tr>
                <td>${formatearFecha(actividad.fechaHora)}</td>
                <td>${actividad.usuario}</td>
                <td>${actividad.modulo}</td>
                <td>${actividad.accion}</td>
                <td>${actividad.descripcion}</td>
                <td>${actividad.direccionIP}</td>
            </tr>`).join("");
}

async function cargarAuditoria() {
    try {
        const [sesiones, actividades] = await Promise.all([
            solicitarAuditoria("/api/auditoria/sesiones"),
            solicitarAuditoria("/api/auditoria/actividades")
        ]);

        renderizarSesiones(sesiones);
        renderizarActividades(actividades);
    } catch (error) {
        mostrarMensajeAuditoria(error.message, true);
    }
}

actualizarAuditoria.addEventListener("click", async () => {
    await cargarAuditoria();
    mostrarMensajeAuditoria("Información de auditoría actualizada.");
});

cargarAuditoria();