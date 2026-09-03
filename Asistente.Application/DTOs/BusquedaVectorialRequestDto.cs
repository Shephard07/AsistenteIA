using System;

namespace Asistente.Application.DTOs;

public class BusquedaVectorialRequestDto
{
    public float[] EmbeddingConsulta { get; init; }
        = Array.Empty<float>();

    public int CantidadResultados { get; init; }

    public decimal PuntajeMinimo { get; init; }
}