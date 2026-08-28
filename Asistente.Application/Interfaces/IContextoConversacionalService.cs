using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IContextoConversacionalService
{
    ContextoConversacionalDto Construir(
        IReadOnlyCollection<MensajeDto> mensajes,
        string? resumenContexto,
        ConfiguracionMemoriaDto configuracion);
        int EstimarTokens(string contenido);
}