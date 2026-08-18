using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asistente.Domain.Enums;

namespace Asistente.Domain.Entities;

/// Representa una conversación registrada entre el usuario y el asistente
public class Conversacion
{
    public int IdConversacion { get; private set; }

    public DateTime FechaInicio { get; private set; }

    public DateTime? FechaFin { get; private set; }

    public EstadoConversacion Estado { get; private set; }

    public ICollection<Mensaje> Mensajes { get; private set; } =
        new List<Mensaje>();

    public Conversacion()
    {
        FechaInicio = DateTime.UtcNow;
        Estado = EstadoConversacion.Activa;
    }

    public void AgregarMensaje(Mensaje mensaje)
    {
        if (Estado != EstadoConversacion.Activa)
        {
            throw new InvalidOperationException(
                "No se pueden agregar mensajes a una conversación finalizada.");
        }

        ArgumentNullException.ThrowIfNull(mensaje);

        Mensajes.Add(mensaje);
    }

    public void Finalizar()
    {
        if (Estado == EstadoConversacion.Finalizada)
        {
            return;
        }

        Estado = EstadoConversacion.Finalizada;
        FechaFin = DateTime.UtcNow;
    }
}