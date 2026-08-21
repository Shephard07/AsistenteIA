const rolesPage = document.getElementById("rolesPage");
const apiBaseUrlRoles = rolesPage.dataset.apiBaseUrl.replace(/\/$/, "");
const rolesBody = document.getElementById("rolesBody");
const mensajeRoles = document.getElementById("mensajeRoles");
const formCrearRol = document.getElementById("formCrearRol");

function mostrarMensajeRoles(mensaje, esError = false) {
    mensajeRoles.textContent = mensaje;
    mensajeRoles.className = `alert ${esError ? "alert-danger" : "alert-success"}`;
}

async function solicitarRoles(url, opciones = {}) {
    const response = await fetch(`${apiBaseUrlRoles}${url}`, {
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

function crearFilaRol(rol) {
    const estado = rol.activo
        ? "<span class='badge text-bg-success'>Activo</span>"
        : "<span class='badge text-bg-secondary'>Inactivo</span>";

    const textoEstado = rol.activo ? "Desactivar" : "Activar";
    const claseEstado = rol.activo ? "btn-outline-danger" : "btn-outline-success";

    return `
        <tr>
            <td>${rol.idRol}</td>
            <td>${rol.nombre}</td>
            <td>${rol.descripcion}</td>
            <td>${estado}</td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-primary me-1"
                        data-editar="${rol.idRol}"
                        data-nombre="${rol.nombre}"
                        data-descripcion="${rol.descripcion}">
                    Editar
                </button>
                <button class="btn btn-sm ${claseEstado}"
                        data-estado="${rol.idRol}"
                        data-activo="${rol.activo}">
                    ${textoEstado}
                </button>
            </td>
        </tr>`;
}

async function cargarRoles() {
    try {
        const roles = await solicitarRoles("/api/roles");

        rolesBody.innerHTML = roles.length === 0
            ? `<tr><td colspan="5" class="text-center text-muted">
                   No hay roles registrados.
               </td></tr>`
            : roles.map(crearFilaRol).join("");
    } catch (error) {
        rolesBody.innerHTML = `<tr><td colspan="5" class="text-center text-danger">
            ${error.message}
        </td></tr>`;
    }
}

formCrearRol.addEventListener("submit", async event => {
    event.preventDefault();

    try {
        await solicitarRoles("/api/roles", {
            method: "POST",
            body: JSON.stringify({
                nombre: document.getElementById("nombreRol").value,
                descripcion: document.getElementById("descripcionRol").value
            })
        });

        formCrearRol.reset();
        mostrarMensajeRoles("Rol creado correctamente.");
        await cargarRoles();
    } catch (error) {
        mostrarMensajeRoles(error.message, true);
    }
});

rolesBody.addEventListener("click", async event => {
    const botonEditar = event.target.closest("[data-editar]");
    const botonEstado = event.target.closest("[data-estado]");

    try {
        if (botonEditar) {
            const idRol = botonEditar.dataset.editar;
            const nombre = prompt(
                "Nombre del rol:",
                botonEditar.dataset.nombre);

            if (nombre === null) return;

            const descripcion = prompt(
                "Descripción del rol:",
                botonEditar.dataset.descripcion);

            if (descripcion === null) return;

            await solicitarRoles(`/api/roles/${idRol}`, {
                method: "PUT",
                body: JSON.stringify({ nombre, descripcion })
            });

            mostrarMensajeRoles("Rol actualizado correctamente.");
            await cargarRoles();
        }

        if (botonEstado) {
            const idRol = botonEstado.dataset.estado;
            const activoActual = botonEstado.dataset.activo === "true";
            const activar = !activoActual;

            await solicitarRoles(
                `/api/roles/${idRol}/estado?activar=${activar}`,
                { method: "PATCH" });

            mostrarMensajeRoles(
                activar
                    ? "Rol activado correctamente."
                    : "Rol desactivado correctamente.");

            await cargarRoles();
        }
    } catch (error) {
        mostrarMensajeRoles(error.message, true);
    }
});

cargarRoles();