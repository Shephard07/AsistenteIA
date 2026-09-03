using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;

namespace Asistente.Application.Services;

public class AdministracionRagService
    : IAdministracionRagService
{
    private readonly IEmbeddingConfiguracionRepository
        _configuracionRepository;

    private readonly IDocumentoIndexadoRepository
        _documentoIndexadoRepository;

    public AdministracionRagService(
        IEmbeddingConfiguracionRepository configuracionRepository,
        IDocumentoIndexadoRepository documentoIndexadoRepository)
    {
        _configuracionRepository = configuracionRepository;
        _documentoIndexadoRepository = documentoIndexadoRepository;
    }

    public async Task<EstadoRagDto> ObtenerEstadoAsync(
        CancellationToken cancellationToken = default)
    {
        var configuracion = await _configuracionRepository
            .ObtenerActivaAsync(cancellationToken);

        var procesamientos = await _documentoIndexadoRepository
            .ListarProcesamientosAsync(cancellationToken);

        var documentos = procesamientos
            .Select(MapearDocumento)
            .ToArray();

        return new EstadoRagDto
        {
            Configuracion = new ConfiguracionRagDto
            {
                Proveedor = configuracion.Proveedor,
                ModeloEmbeddings = configuracion.ModeloEmbeddings,
                BaseVectorial = configuracion.BaseVectorial,
                CantidadResultados = configuracion.CantidadResultados,
                PuntajeMinimo = configuracion.PuntajeMinimo,
                LongitudMaximaContexto =
                    configuracion.LongitudMaximaContexto
            },
            TotalDocumentos = documentos.Length,
            TotalPendientes = documentos.Count(documento =>
                documento.Estado ==
                EstadoIndexacionDocumento.Pendiente),
            TotalEnProceso = documentos.Count(documento =>
                documento.Estado ==
                EstadoIndexacionDocumento.EnProceso),
            TotalIndexados = documentos.Count(documento =>
                documento.Estado ==
                EstadoIndexacionDocumento.Indexado),
            TotalConError = documentos.Count(documento =>
                documento.Estado ==
                EstadoIndexacionDocumento.Error),
            Documentos = documentos
        };
    }

    private static DocumentoIndexadoEstadoDto MapearDocumento(
        DocumentoProcesado procesamiento)
    {
        var version = procesamiento.VersionDocumento
            ?? throw new InvalidOperationException(
                "No se encontró la versión del documento procesado.");

        var documento = version.Documento
            ?? throw new InvalidOperationException(
                "No se encontró el documento asociado a la versión.");

        var indexacion = procesamiento.Indexacion;

        return new DocumentoIndexadoEstadoDto
        {
            IdDocumento = documento.IdDocumento,
            CodigoDocumento = documento.Codigo,
            NombreDocumento = documento.Nombre,
            IdVersionDocumento = version.IdVersion,
            NumeroVersion = version.NumeroVersion,
            IdDocumentoProcesado =
                procesamiento.IdDocumentoProcesado,
            IdDocumentoIndexado =
                indexacion?.IdDocumentoIndexado,
            Estado = indexacion?.Estado ??
                EstadoIndexacionDocumento.Pendiente,
            TotalChunks = indexacion?.TotalChunks ??
                procesamiento.TotalChunks,
            TotalEmbeddings = indexacion?.TotalEmbeddings ?? 0,
            FechaInicio = indexacion?.FechaInicio,
            FechaIndexacion = indexacion?.FechaIndexacion,
            Observaciones = indexacion?.Observaciones ?? string.Empty
        };
    }
}