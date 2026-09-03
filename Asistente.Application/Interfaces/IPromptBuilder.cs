using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IPromptBuilder
{
    string ConstruirPromptSistema(
        AsistenteDto asistente,
        PromptSistemaDto prompt);

    ChatRequestDto ConstruirSolicitudChat(
        AsistenteDto asistente,
        PromptSistemaDto prompt,
        IReadOnlyCollection<MensajeDto> mensajes,
        string? resumenContexto);

    ChatRequestDto ConstruirSolicitudChat(
        AsistenteDto asistente,
        PromptSistemaDto prompt,
        IReadOnlyCollection<MensajeDto> mensajes,
        string? resumenContexto,
        string? contextoDocumental);
}