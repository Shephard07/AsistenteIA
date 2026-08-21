document.addEventListener("DOMContentLoaded", () => {
    const chatPage = document.querySelector(".chat-page");
    const apiBaseUrl = chatPage.dataset.apiBaseUrl.replace(/\/$/, "");

    const messagesContainer = document.getElementById("chatMessages");
    const messageInput = document.getElementById("messageInput");
    const sendButton = document.getElementById("sendButton");
    const clearButton = document.getElementById("clearButton");
    const loadingIndicator = document.getElementById("loadingIndicator");
    const validationMessage = document.getElementById("validationMessage");

    let idConversacion = null;
    let enviandoMensaje = false;

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

    function mostrarCarga(mostrar) {
        loadingIndicator.classList.toggle("d-none", !mostrar);
        sendButton.disabled = mostrar;
        messageInput.disabled = mostrar;
        enviandoMensaje = mostrar;
    }

    async function enviarMensaje() {
        const mensaje = messageInput.value.trim();

        if (!mensaje) {
            validationMessage.textContent =
                "Escribe un mensaje antes de enviarlo.";
            messageInput.focus();
            return;
        }

        if (enviandoMensaje) {
            return;
        }

        validationMessage.textContent = "";
        agregarMensaje("user", mensaje, "Tú");

        messageInput.value = "";
        mostrarCarga(true);

        try {
            const response = await fetch(
                `${apiBaseUrl}/api/conversaciones/mensajes`,
                {
                    method: "POST",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        idConversacion: idConversacion,
                        mensaje: mensaje
                    })
                });

            const data = await response.json();

            if (!response.ok) {
                throw new Error(
                    data.mensaje ||
                    "No fue posible procesar tu mensaje."
                );
            }

            idConversacion = data.idConversacion;

            agregarMensaje(
                "assistant",
                data.respuesta,
                `Asistente · ${data.tiempoRespuestaMs} ms`
            );
        } catch (error) {
            agregarMensaje(
                "error",
                error.message,
                "Error"
            );
        } finally {
            mostrarCarga(false);
            messageInput.focus();
        }
    }

    sendButton.addEventListener("click", enviarMensaje);

    messageInput.addEventListener("keydown", (event) => {
        if (event.key === "Enter" && !event.shiftKey) {
            event.preventDefault();
            enviarMensaje();
        }
    });

    clearButton.addEventListener("click", () => {
        idConversacion = null;
        validationMessage.textContent = "";

        messagesContainer.innerHTML = "";

        agregarMensaje(
            "assistant",
            "¡Hola! Soy tu asistente inteligente empresarial. ¿En qué puedo ayudarte hoy?",
            "Asistente"
        );

        messageInput.focus();
    });
});