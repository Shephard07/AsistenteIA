using Asistente.Application.DTOs;
using Asistente.Application.Validators;
using Xunit;

namespace Asistente.Tests.Validators;

public class EnviarMensajeRequestValidatorTests
{
    private readonly EnviarMensajeRequestValidator _validator = new();

    [Fact]
    public void Debe_Fallar_Cuando_El_Mensaje_Esta_Vacio()
    {
        var request = new EnviarMensajeRequestDto
        {
            Mensaje = string.Empty
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == "El mensaje es obligatorio.");
    }

    [Fact]
    public void Debe_Fallar_Cuando_El_IdConversacion_No_Es_Valido()
    {
        var request = new EnviarMensajeRequestDto
        {
            IdConversacion = 0,
            Mensaje = "Consulta válida"
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage.Contains(
                "identificador de conversación"));
    }

    [Fact]
    public void Debe_Ser_Valido_Cuando_La_Solicitud_Es_Correcta()
    {
        var request = new EnviarMensajeRequestDto
        {
            IdConversacion = 1,
            Mensaje = "¿Cómo mejoro la productividad?"
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }
}