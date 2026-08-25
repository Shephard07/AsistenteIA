using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface IHistorialPromptRepository
{
    Task AgregarAsync(
        HistorialPrompt historial,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<HistorialPrompt>> ListarPorPromptAsync(
        int idPrompt,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<HistorialPrompt>> ListarPorAsistenteAsync(
    int idAsistente,
    CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}