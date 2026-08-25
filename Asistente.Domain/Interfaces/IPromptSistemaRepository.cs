using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface IPromptSistemaRepository
{
    Task AgregarAsync(
        PromptSistema prompt,
        CancellationToken cancellationToken = default);

    Task<PromptSistema?> ObtenerPorIdAsync(
        int idPrompt,
        CancellationToken cancellationToken = default);

    Task<PromptSistema?> ObtenerActivoPorAsistenteAsync(
        int idAsistente,
        CancellationToken cancellationToken = default);

    Task<int> ObtenerUltimaVersionAsync(
        int idAsistente,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PromptSistema>> ListarPorAsistenteAsync(
        int idAsistente,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}