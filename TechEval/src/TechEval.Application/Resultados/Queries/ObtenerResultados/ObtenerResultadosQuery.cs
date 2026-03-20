using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Application.Sesiones.Commands.RegistrarRespuesta;
using TechEval.Domain.Evaluaciones.Repositorios;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Resultados.Queries.ObtenerResultados;

public sealed record ObtenerResultadosQuery(
    Guid SesionId
) : IRequest<ObtenerResultadosResponse>;

public sealed record ObtenerResultadosResponse(
    Guid ResultadoId,
    Guid SesionId,
    string NombreCandidato,
    string TituloEvaluacion,
    decimal PuntuacionTotal,
    decimal PuntuacionMaxima,
    decimal PorcentajeObtenido,
    string EstadoGeneral,
    int ViolacionesPestana,
    int TiempoTotalSegundos,
    string EstadoRevision,
    List<PuntuacionPreguntaDto> Puntuaciones,
    List<TranscripcionDto> Transcripciones
);

public sealed record PuntuacionPreguntaDto(
    int NumeroPregunta,
    string TipoPregunta,
    string Respuesta,
    decimal? PuntuacionAutomatica,
    decimal? PuntuacionManual,
    decimal? PuntuacionIASugerida,
    string? JustificacionIA,
    string? Observaciones,
    bool FueExpirada,
    string? UrlAdjunto,
    int TiempoEmpleadoSegundos,
    Guid PreguntaId
);

public sealed record TranscripcionDto(
    Guid Id,
    string Tipo,
    string? Contenido,
    string? UrlArchivo,
    decimal? PuntajeIA,
    string? JustificacionIA,
    string Estado
);

public sealed class ObtenerResultadosQueryHandler : IRequestHandler<ObtenerResultadosQuery, ObtenerResultadosResponse>
{
    private readonly IRepositorioResultadoEvaluacion _repositorio;
    private readonly IRepositorioSesionEvaluacion _repoSesion;
    private readonly IRepositorioEvaluacion _repoEvaluacion;
    private readonly IServicioAuth _servicioAuth;

    public ObtenerResultadosQueryHandler(
        IRepositorioResultadoEvaluacion repositorio,
        IRepositorioSesionEvaluacion repoSesion,
        IRepositorioEvaluacion repoEvaluacion,
        IServicioAuth servicioAuth)
    {
        _repositorio = repositorio;
        _repoSesion = repoSesion;
        _repoEvaluacion = repoEvaluacion;
        _servicioAuth = servicioAuth;
    }

    public async Task<ObtenerResultadosResponse> Handle(ObtenerResultadosQuery request, CancellationToken cancellationToken)
    {
        var resultado = await _repositorio.ObtenerPorSesionAsync(request.SesionId, cancellationToken);

        // Si no existe el resultado pero la sesión está completada, crearlo automáticamente (repair)
        if (resultado == null)
        {
            resultado = await IntentarCrearResultadoAsync(request.SesionId, cancellationToken);
        }

        // Si aún no hay resultado (sesión no completada), devolver respuesta parcial vacía
        if (resultado == null)
        {
            return await ConstruirRespuestaParcialAsync(request.SesionId, cancellationToken);
        }

        // Recalcular puntuación máxima con datos reales de la evaluación (corrige resultados legacy con max=100)
        var evaluacion = await _repoEvaluacion.ObtenerConPreguntasAsync(resultado.EvaluacionId, cancellationToken);
        var puntuacionMaximaReal = RegistrarRespuestaCommandHandler.CalcularPuntuacionMaximaEvaluacion(evaluacion);
        if (puntuacionMaximaReal > 0 && resultado.PuntuacionMaxima != puntuacionMaximaReal)
        {
            resultado.CalcularPuntuacionTotal(puntuacionMaximaReal);
            await _repositorio.ActualizarAsync(resultado, cancellationToken);
        }

        var puntuaciones = resultado.Puntuaciones.Select(p => new PuntuacionPreguntaDto(
            NumeroPregunta: p.NumeroPregunta,
            TipoPregunta: p.TipoPregunta,
            Respuesta: p.Respuesta,
            PuntuacionAutomatica: p.PuntuacionAutomatica,
            PuntuacionManual: p.PuntuacionManual,
            PuntuacionIASugerida: p.PuntuacionIASugerida,
            JustificacionIA: p.JustificacionIA,
            Observaciones: p.Observaciones,
            FueExpirada: p.FueExpirada,
            UrlAdjunto: p.UrlAdjunto,
            TiempoEmpleadoSegundos: p.TiempoEmpleadoSegundos,
            PreguntaId: p.PreguntaSesionId
        )).ToList();

        var transcripciones = resultado.Transcripciones.Select(t => new TranscripcionDto(
            Id: t.Id,
            Tipo: t.Tipo.ToString(),
            Contenido: t.Contenido,
            UrlArchivo: t.UrlArchivo,
            PuntajeIA: t.PuntajeIA,
            JustificacionIA: t.JustificacionIA,
            Estado: t.Estado.ToString()
        )).ToList();

        return new ObtenerResultadosResponse(
            ResultadoId: resultado.Id,
            SesionId: resultado.SesionId,
            NombreCandidato: resultado.NombreCandidato,
            TituloEvaluacion: resultado.TituloEvaluacion,
            PuntuacionTotal: resultado.PuntuacionTotal,
            PuntuacionMaxima: resultado.PuntuacionMaxima,
            PorcentajeObtenido: resultado.PorcentajeObtenido,
            EstadoGeneral: resultado.ObtenerEstadoGeneral(),
            ViolacionesPestana: resultado.ViolacionesPestana,
            TiempoTotalSegundos: resultado.TiempoTotalSegundos,
            EstadoRevision: resultado.EstadoRevision.ToString(),
            Puntuaciones: puntuaciones,
            Transcripciones: transcripciones
        );
    }

    /// <summary>
    /// Intenta crear automáticamente un ResultadoEvaluacion si la sesión está completada
    /// pero nunca se generó el resultado (e.g. sesiones creadas antes de esta funcionalidad).
    /// </summary>
    private async Task<ResultadoEvaluacion?> IntentarCrearResultadoAsync(Guid sesionId, CancellationToken cancellationToken)
    {
        var sesion = await _repoSesion.ObtenerConPreguntasAsync(sesionId, cancellationToken);
        if (sesion == null || sesion.Estado != Domain.Sesiones.EstadoSesion.Completada)
            return null;

        var evaluacion = await _repoEvaluacion.ObtenerConPreguntasAsync(sesion.EvaluacionId, cancellationToken);

        // Obtener nombre del candidato
        var usuarios = await _servicioAuth.ListarUsuariosAsync(null, cancellationToken);
        var nombreCandidato = usuarios.FirstOrDefault(u => u.Id == sesion.CandidatoId)?.Nombre
            ?? sesion.CandidatoId;

        var tiempoTotal = sesion.CompletadaEn.HasValue && sesion.IniciadaEn.HasValue
            ? (int)(sesion.CompletadaEn.Value - sesion.IniciadaEn.Value).TotalSeconds
            : 0;

        var resultado = ResultadoEvaluacion.Crear(
            sesionId: sesion.Id,
            evaluacionId: sesion.EvaluacionId,
            candidatoId: sesion.CandidatoId,
            nombreCandidato: nombreCandidato,
            tituloEvaluacion: evaluacion?.Nombre ?? "Evaluación",
            violacionesPestana: sesion.ContadorViolacionesPestana,
            tiempoTotalSegundos: tiempoTotal
        );

        foreach (var preguntaSesion in sesion.Preguntas.Where(p => p.FueRespondida).OrderBy(p => p.Orden))
        {
            var preguntaOriginal = evaluacion?.Preguntas.FirstOrDefault(p => p.Id == preguntaSesion.PreguntaId);
            var tipoPregunta = preguntaOriginal?.TipoPregunta.ToString() ?? "TextoLibre";
            var autoScore = RegistrarRespuestaCommandHandler.CalcularPuntuacionAutomatica(
                preguntaOriginal, preguntaSesion.Respuesta?.Texto);

            var puntuacion = PuntuacionPregunta.Crear(
                preguntaSesionId: preguntaSesion.Id,
                numeroPregunta: preguntaSesion.Orden,
                tipoPregunta: tipoPregunta,
                respuesta: preguntaSesion.Respuesta?.Texto ?? string.Empty,
                puntuacionAutomatica: autoScore,
                fueExpirada: preguntaSesion.TiempoExpirado,
                urlAdjunto: preguntaSesion.Respuesta?.UrlAdjunto,
                tiempoEmpleadoSegundos: preguntaSesion.Respuesta?.TiempoEmpleadoSegundos ?? 0
            );

            resultado.AgregarPuntuacion(puntuacion);
        }

        resultado.CalcularPuntuacionTotal(RegistrarRespuestaCommandHandler.CalcularPuntuacionMaximaEvaluacion(evaluacion));
        await _repositorio.AgregarAsync(resultado, cancellationToken);

        return resultado;
    }

    /// <summary>
    /// Construye una respuesta parcial cuando la sesión aún no tiene ResultadoEvaluacion
    /// (por ejemplo, sesiones en progreso o no iniciadas).
    /// </summary>
    private async Task<ObtenerResultadosResponse> ConstruirRespuestaParcialAsync(Guid sesionId, CancellationToken cancellationToken)
    {
        var sesion = await _repoSesion.ObtenerConPreguntasAsync(sesionId, cancellationToken)
            ?? throw new NotFoundException("SesionEvaluacion", sesionId);

        var evaluacion = await _repoEvaluacion.ObtenerConPreguntasAsync(sesion.EvaluacionId, cancellationToken);

        var usuarios = await _servicioAuth.ListarUsuariosAsync(null, cancellationToken);
        var nombreCandidato = usuarios.FirstOrDefault(u => u.Id == sesion.CandidatoId)?.Nombre
            ?? sesion.CandidatoId;

        return new ObtenerResultadosResponse(
            ResultadoId: Guid.Empty,
            SesionId: sesion.Id,
            NombreCandidato: nombreCandidato,
            TituloEvaluacion: evaluacion?.Nombre ?? "Evaluación",
            PuntuacionTotal: 0,
            PuntuacionMaxima: 0,
            PorcentajeObtenido: 0,
            EstadoGeneral: sesion.Estado.ToString(),
            ViolacionesPestana: sesion.ContadorViolacionesPestana,
            TiempoTotalSegundos: 0,
            EstadoRevision: "Pendiente",
            Puntuaciones: [],
            Transcripciones: []
        );
    }
}
