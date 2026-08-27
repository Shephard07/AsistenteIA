    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Asistente.Domain.Enums;

    namespace Asistente.Domain.Entities;


    /// Representa un mensaje perteneciente a una conversación.
    public class Mensaje
    {
        public int IdMensaje { get; private set; }

        public int IdConversacion { get; private set; }

        public RolMensaje Rol { get; private set; }

        public string Contenido { get; private set; } = string.Empty;

        public DateTime FechaHora { get; private set; }

        public int? TiempoRespuestaMs { get; private set; }

        public Conversacion? Conversacion { get; private set; }

        // Constructor requerido por Entity Framework Core.
        private Mensaje()
        {
        }

        public Mensaje(
            RolMensaje rol,
            string contenido,
            int? tiempoRespuestaMs = null)
        {
            if (string.IsNullOrWhiteSpace(contenido))
            {
                throw new ArgumentException(
                    "El contenido del mensaje no puede estar vacío.",
                    nameof(contenido));
            }

            Rol = rol;
            Contenido = contenido.Trim();
            FechaHora = DateTime.UtcNow;
            TiempoRespuestaMs = tiempoRespuestaMs;
        }
    }