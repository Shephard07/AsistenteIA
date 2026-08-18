using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Domain.ValueObjects;

public record RespuestaIA(
    string Contenido,
    int TiempoRespuestaMs);