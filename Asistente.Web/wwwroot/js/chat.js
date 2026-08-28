document.addEventListener("DOMContentLoaded", () => {
    const chatPage = document.querySelector(".chat-page");
    const apiBaseUrl = chatPage.dataset.apiBaseUrl.replace(/\/$/, "");

    const messagesContainer = document.getElementById("chatMessages");
    const messageInput = document.getElementById("messageInput");
    const sendButton = document.getElementById("sendButton");
    const newConversationButton = document.getElementById(
        "newConversationButton");

    const renameButton = document.getElementById("renameButton");
    const archiveButton = document.getElementById("archiveButton");
    const deleteButton = document.getElementById("deleteButton");

    const conversationTitle = document.getElementById("conversationTitle");
    const conversationSubtitle = document.getElementById(
        "conversationSubtitle");

    const conversationList = document.getElementById("conversationList");
    const searchConversationInput = document.getElementById(
        "searchConversationInput");

    const showArchivedInput = document.getElementById(
        "showArchivedInput");

    const loadingIndicator = document.getElementById("loadingIndicator");
    const validationMessage = document.getElementById("validationMessage");

    let idConversacion = null;
    let estadoConversacion = null;
    let enviandoMensaje = false;
    let temporizadorBusqueda = null;

    function agregarMensaje(tipo, contenido, rol) {
        const message = document.createElement("article");
        message.className = `message message-${tipo}`;

        const role = document.createElement("span");
        role.className = "message-role";
        role.textContent = rol;

        const text = document.createElement("p");
        text.textContent = contenido;

        message.appendChild(role);
        message.appendChild(text);
        messagesContainer.appendChild(message);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }

    function limpiarMensajes() {
        messagesContainer.replaceChildren();
    }

    function mostrarMensajeBienvenida() {
        limpiarMensajes();

        agregarMensaje(
            "assistant",
            "¡Hola! Soy tu asistente inteligente empresarial. ¿En qué puedo ayudarte hoy?",
            "Asistente");
    }

    function formatearFecha(fecha) {
        return new Date(fecha).toLocaleString("es-PE", {
            dateStyle: "short",
            timeStyle: "short"
        });
    }

    function obtenerTipoMensaje(rol) {
        return rol.toLowerCase() === "usuario"
            ? "user"
            : "assistant";
    }

    function actualizarCabecera() {
        const existeConversacion = idConversacion !== null;
        const archivada = estadoConversacion === "Archivada";

        renameButton.disabled = !existeConversacion || archivada;
        deleteButton.disabled = !existeConversacion;
        archiveButton.disabled = !existeConversacion;

        archiveButton.textContent = archivada
            ? "Reactivar"
            : "Archivar";
    }

    function mostrarCarga(mostrar) {
        loadingIndicator.classList.toggle("d-none", !mostrar);
        sendButton.disabled = mostrar;
        messageInput.disabled = mostrar;
        enviandoMensaje = mostrar;
    }

    async function solicitarApi(url, opciones = {}) {
        const response = await fetch(`${apiBaseUrl}${url}`, {
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

    function crearElementoConversacion(conversacion) {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "conversation-item";

        if (conversacion.idConversacion === idConversacion) {
            button.classList.add("active");
        }

        const title = document.createElement("strong");
        title.textContent = conversacion.titulo;

        const detail = document.createElement("span");
        detail.textContent =
            `${conversacion.totalMensajes} mensajes · ` +
            formatearFecha(conversacion.fechaUltimaActividad);

        button.appendChild(title);
        button.appendChild(detail);

        if (conversacion.estado === "Archivada") {
            const badge = document.createElement("small");
            badge.className = "conversation-status";
            badge.textContent = "Archivada";
            button.appendChild(badge);
        }

        button.addEventListener("click", async () => {
            try {
                await cargarConversacion(conversacion.idConversacion);
            } catch (error) {
                validationMessage.textContent = error.message;
            }
        });

        return button;
    }

    async function cargarConversaciones() {
        const terminoBusqueda = searchConversationInput.value.trim();
        const incluirArchivadas = showArchivedInput.checked;

        const parametros = new URLSearchParams({
            incluirArchivadas,
            cantidadMaxima: 50
        });

        if (terminoBusqueda) {
            parametros.append("terminoBusqueda", terminoBusqueda);
        }

        const conversaciones = await solicitarApi(
            `/api/conversaciones?${parametros.toString()}`);

        conversationList.replaceChildren();

        if (conversaciones.length === 0) {
            const empty = document.createElement("p");
            empty.className = "text-muted small p-3 mb-0";
            empty.textContent = terminoBusqueda
                ? "No se encontraron conversaciones."
                : "Aún no tienes conversaciones.";
            conversationList.appendChild(empty);
            return;
        }

        conversaciones.forEach(conversacion => {
            conversationList.appendChild(
                crearElementoConversacion(conversacion));
        });
    }

    async function cargarConversacion(id) {
        const conversacion = await solicitarApi(
            `/api/conversaciones/${id}`);

        idConversacion = conversacion.idConversacion;
        estadoConversacion = conversacion.estado;

        conversationTitle.textContent = conversacion.titulo;
        conversationSubtitle.textContent =
            `${conversacion.totalMensajes} mensajes · ` +
            `Última actividad: ${formatearFecha(
                conversacion.fechaUltimaActividad)}`;

        limpiarMensajes();

        conversacion.mensajes.forEach(mensaje => {
            const tipo = obtenerTipoMensaje(mensaje.rol);
            const rol = tipo === "user"
                ? "Tú"
                : `Asistente${mensaje.tiempoRespuestaMs
                    ? ` · ${mensaje.tiempoRespuestaMs} ms`
                    : ""}`;

            agregarMensaje(tipo, mensaje.contenido, rol);
        });

        actualizarCabecera();
        await cargarConversaciones();
        messageInput.focus();
    }

    function iniciarNuevaConversacion() {
        idConversacion = null;
        estadoConversacion = null;

        conversationTitle.textContent = "Nueva conversación";
        conversationSubtitle.textContent =
            "Inicia una consulta para crear una conversación.";

        validationMessage.textContent = "";
        mostrarMensajeBienvenida();
        actualizarCabecera();
        cargarConversaciones();
        messageInput.focus();
    }

    async function enviarMensaje() {
        const mensaje = messageInput.value.trim();

        if (!mensaje) {
            validationMessage.textContent =
                "Escribe un mensaje antes de enviarlo.";
            messageInput.focus();
            return;
        }

        if (enviandoMensaje || estadoConversacion === "Archivada") {
            return;
        }

        validationMessage.textContent = "";
        agregarMensaje("user", mensaje, "Tú");

        messageInput.value = "";
        mostrarCarga(true);

        try {
            const data = await solicitarApi(
                "/api/conversaciones/mensajes",
                {
                    method: "POST",
                    body: JSON.stringify({
                        idConversacion,
                        mensaje
                    })
                });

            idConversacion = data.idConversacion;
            estadoConversacion = "Activa";

            agregarMensaje(
                "assistant",
                data.respuesta,
                `Asistente · ${data.tiempoRespuestaMs} ms`);

            conversationSubtitle.textContent =
                "Conversación activa. Actualizando historial...";

            actualizarCabecera();
            await cargarConversacion(idConversacion);

        } catch (error) {
            agregarMensaje("error", error.message, "Error");
        } finally {
            mostrarCarga(false);
            messageInput.focus();
        }
    }

    async function renombrarConversacion() {
        if (!idConversacion) {
            return;
        }

        const titulo = window.prompt(
            "Escribe el nuevo título:",
            conversationTitle.textContent);

        if (!titulo || !titulo.trim()) {
            return;
        }

        try {
            await solicitarApi(
                `/api/conversaciones/${idConversacion}/titulo`,
                {
                    method: "PATCH",
                    body: JSON.stringify({
                        titulo: titulo.trim()
                    })
                });

            conversationTitle.textContent = titulo.trim();
            await cargarConversaciones();
        } catch (error) {
            validationMessage.textContent = error.message;
        }
    }

    async function cambiarEstadoConversacion() {
        if (!idConversacion) {
            return;
        }

        const archivada = estadoConversacion === "Archivada";
        const accion = archivada ? "reactivar" : "archivar";

        try {
            await solicitarApi(
                `/api/conversaciones/${idConversacion}/${accion}`,
                { method: "PATCH" });

            estadoConversacion = archivada
                ? "Activa"
                : "Archivada";

            conversationSubtitle.textContent = archivada
                ? "Conversación activa."
                : "Conversación archivada. Puedes reactivarla cuando quieras.";

            actualizarCabecera();
            await cargarConversaciones();
        } catch (error) {
            validationMessage.textContent = error.message;
        }
    }

    async function eliminarConversacion() {
        if (!idConversacion) {
            return;
        }

        const confirmar = window.confirm(
            "¿Deseas eliminar esta conversación? Esta acción la ocultará del historial.");

        if (!confirmar) {
            return;
        }

        try {
            await solicitarApi(
                `/api/conversaciones/${idConversacion}`,
                { method: "DELETE" });

            iniciarNuevaConversacion();
        } catch (error) {
            validationMessage.textContent = error.message;
        }
    }

    sendButton.addEventListener("click", enviarMensaje);

    messageInput.addEventListener("keydown", event => {
        if (event.key === "Enter" && !event.shiftKey) {
            event.preventDefault();
            enviarMensaje();
        }
    });

    newConversationButton.addEventListener(
        "click",
        iniciarNuevaConversacion);

    renameButton.addEventListener(
        "click",
        renombrarConversacion);

    archiveButton.addEventListener(
        "click",
        cambiarEstadoConversacion);

    deleteButton.addEventListener(
        "click",
        eliminarConversacion);

    searchConversationInput.addEventListener("input", () => {
        clearTimeout(temporizadorBusqueda);

        temporizadorBusqueda = setTimeout(() => {
            cargarConversaciones().catch(error => {
                validationMessage.textContent = error.message;
            });
        }, 300);
    });

    showArchivedInput.addEventListener("change", () => {
        cargarConversaciones().catch(error => {
            validationMessage.textContent = error.message;
        });
    });

    actualizarCabecera();

    cargarConversaciones().catch(error => {
        validationMessage.textContent = error.message;
    });
});