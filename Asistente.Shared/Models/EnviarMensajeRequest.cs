using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Shared.Models;

public class EnviarMensajeRequest
{
    public int? IdConversacion { get; set; }

    public string Mensaje { get; set; } = string.Empty;
}