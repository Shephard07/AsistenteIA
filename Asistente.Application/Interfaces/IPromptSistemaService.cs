using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IPromptSistemaService
{
    Task<IReadOnlyCollection<PromptSistemaDto>> ListarPorAsistenteAsync(
        int idAsistente,
        CancellationToken cancellationToken = default);

    Task<PromptSistemaDto> ObtenerActivoPorAsistenteAsync(
        int idAsistente,
        CancellationToken cancellationToken = default);

    Task<PromptSistemaDto> CrearAsync(
        int idAsistente,
        CrearPromptSistemaRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task<PromptSistemaDto> CrearNuevaVersionAsync(
        int idPromptOrigen,
        CrearVersionPromptRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task CambiarEstadoAsync(
        int idPrompt,
        bool activo,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<HistorialPromptDto>> ListarHistorialPorAsistenteAsync(
        int idAsistente,
        CancellationToken cancellationToken = default);
}