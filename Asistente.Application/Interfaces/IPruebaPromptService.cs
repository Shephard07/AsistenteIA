using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

/// <summary>
/// Permite probar una versión de prompt sin guardar una conversación.
/// </summary>
public interface IPruebaPromptService
{
    Task<ProbarPromptResponseDto> ProbarAsync(
        ProbarPromptRequestDto request,
        CancellationToken cancellationToken = default);
}