using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Shared.Models;

public class EnviarMensajeResponse
{
    public int IdConversacion { get; set; }

    public string Respuesta { get; set; } = string.Empty;

    public int TiempoRespuestaMs { get; set; }
}