const usuariosPage = document.getElementById("usuariosPage");
const apiBaseUrlUsuarios = usuariosPage.dataset.apiBaseUrl.replace(/\/$/, "");
const usuariosBody = document.getElementById("usuariosBody");
const rolesCrearUsuario = document.getElementById("rolesCrearUsuario");
const formCrearUsuario = document.getElementById("formCrearUsuario");
const mensajeUsuarios = document.getElementById("mensajeUsuarios");

let usuariosActuales = [];
let rolesActuales = [];

function mostrarMensajeUsuarios(mensaje, esError = false) {
    mensajeUsuarios.textContent = mensaje;
    mensajeUsuarios.className =
        `alert ${esError ? "alert-danger" : "alert-success"}`;
}

async function solicitarUsuarios(url, opciones = {}) {
    const response = await fetch(`${apiBaseUrlUsuarios}${url}`, {
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

    return response.status === 204 ? null : response.json();
}

function renderizarRolesCrear() {
    const rolesActivos = rolesActuales.filter(rol => rol.activo);

    rolesCrearUsuario.innerHTML = rolesActivos.length === 0
        ? "<span class='text-danger'>No existen roles activos disponibles.</span>"
        : rolesActivos.map(rol => `
            <div class="form-check">
                <input class="form-check-input"
                       type="checkbox"
                       value="${rol.idRol}"
                       id="rol-${rol.idRol}">
                <label class="form-check-label" for="rol-${rol.idRol}">
                    ${rol.nombre}
                </label>
            </div>`).join("");
}

function crearFilaUsuario(usuario) {
    const roles = usuario.roles.length === 0
        ? "<span class='text-muted'>Sin roles</span>"
        : usuario.roles.map(rol =>
            `<span class="badge text-bg-info me-1">${rol.nombre}</span>`)
            .join("");

    const estado = usuario.activo
        ? "<span class='badge text-bg-success'>Activo</span>"
        : "<span class='badge text-bg-secondary'>Inactivo</span>";

    const accionEstado = usuario.activo ? "Desactivar" : "Activar";
    const claseEstado = usuario.activo
        ? "btn-outline-danger"
        : "btn-outline-success";

    return `
        <tr>
            <td>${usuario.usuario}</td>
            <td>${usuario.nombres} ${usuario.apellidos}</td>
            <td>${usuario.correo}</td>
            <td>${roles}</td>
            <td>${estado}</td>
            <td class="text-end text-nowrap">
                <button class="btn btn-sm btn-outline-primary me-1"
                        data-editar="${usuario.idUsuario}">
                    Editar
                </button>
                <button class="btn btn-sm btn-outline-secondary me-1"
                        data-roles="${usuario.idUsuario}">
                    Roles
                </button>
                <button class="btn btn-sm btn-outline-warning me-1"
                        data-password="${usuario.idUsuario}">
                    Contraseña
                </button>
                <button class="btn btn-sm ${claseEstado}"
                        data-estado="${usuario.idUsuario}">
                    ${accionEstado}
                </button>
            </td>
        </tr>`;
}

function renderizarUsuarios() {
    usuariosBody.innerHTML = usuariosActuales.length === 0
        ? `<tr><td colspan="6" class="text-center text-muted">
               No hay usuarios registrados.
           </td></tr>`
        : usuariosActuales.map(crearFilaUsuario).join("");
}

async function cargarDatos() {
    try {
        const [roles, usuarios] = await Promise.all([
            solicitarUsuarios("/api/roles"),
            solicitarUsuarios("/api/usuarios")
        ]);

        rolesActuales = roles;
        usuariosActuales = usuarios;

        renderizarRolesCrear();
        renderizarUsuarios();
    } catch (error) {
        usuariosBody.innerHTML = `<tr><td colspan="6" class="text-center text-danger">
            ${error.message}
        </td></tr>`;
    }
}

function obtenerRolesSeleccionados() {
    return [...rolesCrearUsuario.querySelectorAll(
        "input[type='checkbox']:checked")]
        .map(input => Number(input.value));
}

formCrearUsuario.addEventListener("submit", async event => {
    event.preventDefault();

    try {
        await solicitarUsuarios("/api/usuarios", {
            method: "POST",
            body: JSON.stringify({
                usuario: document.getElementById("usuario").value,
                nombres: document.getElementById("nombres").value,
                apellidos: document.getElementById("apellidos").value,
                correo: document.getElementById("correo").value,
                password: document.getElementById("password").value,
                idsRoles: obtenerRolesSeleccionados()
            })
        });

        formCrearUsuario.reset();
        mostrarMensajeUsuarios("Usuario creado correctamente.");
        await cargarDatos();
    } catch (error) {
        mostrarMensajeUsuarios(error.message, true);
    }
});

usuariosBody.addEventListener("click", async event => {
    const boton = event.target.closest("button");

    if (!boton) return;

    const idUsuario = Number(
        boton.dataset.editar ||
        boton.dataset.roles ||
        boton.dataset.password ||
        boton.dataset.estado);

    const usuario = usuariosActuales.find(
        item => item.idUsuario === idUsuario);

    if (!usuario) return;

    try {
        if (boton.dataset.editar) {
            const nuevoUsuario = prompt("Usuario:", usuario.usuario);
            if (nuevoUsuario === null) return;

            const nombres = prompt("Nombres:", usuario.nombres);
            if (nombres === null) return;

            const apellidos = prompt("Apellidos:", usuario.apellidos);
            if (apellidos === null) return;

            const correo = prompt("Correo:", usuario.correo);
            if (correo === null) return;

            await solicitarUsuarios(`/api/usuarios/${idUsuario}`, {
                method: "PUT",
                body: JSON.stringify({
                    usuario: nuevoUsuario,
                    nombres,
                    apellidos,
                    correo
                })
            });

            mostrarMensajeUsuarios("Usuario actualizado correctamente.");
        }

        if (boton.dataset.roles) {
            const rolesActualesUsuario = usuario.roles
                .map(rol => rol.idRol)
                .join(",");

            const textoRoles = prompt(
                "IDs de roles activos separados por coma:",
                rolesActualesUsuario);

            if (textoRoles === null) return;

            const idsRoles = textoRoles
                .split(",")
                .map(valor => Number(valor.trim()))
                .filter(valor => Number.isInteger(valor) && valor > 0);

            await solicitarUsuarios(`/api/usuarios/${idUsuario}/roles`, {
                method: "PUT",
                body: JSON.stringify({ idsRoles })
            });

            mostrarMensajeUsuarios(
                "Roles del usuario actualizados correctamente.");
        }

        if (boton.dataset.password) {
            const nuevaPassword = prompt(
                "Nueva contraseña (8 caracteres o más, con complejidad):");

            if (nuevaPassword === null) return;

            await solicitarUsuarios(`/api/usuarios/${idUsuario}/password`, {
                method: "PUT",
                body: JSON.stringify({ nuevaPassword })
            });

            mostrarMensajeUsuarios("Contraseña actualizada correctamente.");
        }

        if (boton.dataset.estado) {
            const activar = !usuario.activo;

            await solicitarUsuarios(
                `/api/usuarios/${idUsuario}/estado?activar=${activar}`,
                { method: "PATCH" });

            mostrarMensajeUsuarios(
                activar
                    ? "Usuario activado correctamente."
                    : "Usuario desactivado correctamente.");
        }

        await cargarDatos();
    } catch (error) {
        mostrarMensajeUsuarios(error.message, true);
    }
});

cargarDatos();