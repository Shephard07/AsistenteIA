    using Asistente.Application.Interfaces;
    using Asistente.Domain.Entities;
    using Asistente.Domain.Interfaces;

    namespace Asistente.Application.Services;

    /// <summary>
    /// Gestiona la obtención y creación de conversaciones.
    /// </summary>
    public class ConversacionService : IConversacionService
    {
        private readonly IConversacionRepository _conversacionRepository;

        public ConversacionService(
            IConversacionRepository conversacionRepository)
        {
            _conversacionRepository = conversacionRepository;
        }

        public async Task<Conversacion> ObtenerOCrearAsync(
            int? idConversacion,
            int idAsistente,
            int idUsuario,
            CancellationToken cancellationToken = default)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idUsuario),
                    "El identificador del usuario debe ser mayor que cero.");
            }

            if (idConversacion.HasValue)
            {
                return await _conversacionRepository
                    .ObtenerPorIdYUsuarioAsync(
                        idConversacion.Value,
                        idUsuario,
                        cancellationToken)
                    ?? throw new KeyNotFoundException(
                        "La conversación solicitada no existe.");
            }

            var conversacion = new Conversacion(
                idAsistente,
                idUsuario);

            await _conversacionRepository.AgregarAsync(
                conversacion,
                cancellationToken);

            return conversacion;
        }
    }