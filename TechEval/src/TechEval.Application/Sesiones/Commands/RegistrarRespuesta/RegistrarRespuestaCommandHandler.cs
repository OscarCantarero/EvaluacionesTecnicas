using MediatR;
using TechEval.Application.Common.DTOs;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Application.Sesiones.Commands.IniciarSesion;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;
using TechEval.Domain.Sesiones;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Sesiones.Commands.RegistrarRespuesta;

public sealed class RegistrarRespuestaCommandHandler : IRequestHandler<RegistrarRespuestaCommand, RegistrarRespuestaResponse>
{
    private readonly IRepositorioSesionEvaluacion _repo;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IRepositorioEvaluacion _repoEvaluacion;
    private readonly IRepositorioResultadoEvaluacion _repoResultado;

    public RegistrarRespuestaCommandHandler(
        IRepositorioSesionEvaluacion repo,
        IContextoUsuario contextoUsuario,
        IRepositorioEvaluacion repoEvaluacion,
        IRepositorioResultadoEvaluacion repoResultado)
    {
        _repo = repo;
        _contextoUsuario = contextoUsuario;
        _repoEvaluacion = repoEvaluacion;
        _repoResultado = repoResultado;
    }

    public async Task<RegistrarRespuestaResponse> Handle(
        RegistrarRespuestaCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _repo.ObtenerConPreguntasAsync(command.SesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(SesionEvaluacion), command.SesionId);

        if (sesion.CandidatoId != _contextoUsuario.UsuarioId)
            throw new UnauthorizedException("No tienes acceso a esta sesión");

        sesion.RegistrarRespuesta(
            command.PreguntaId,
            command.Texto,
            command.TiempoEmpleadoSegundos,
            command.FueExpirado
        );

        var progreso = sesion.ObtenerProgreso();
        var (respondidas, total) = progreso;

        if (respondidas == total)
            sesion.Completar();

        await _repo.ActualizarAsync(sesion, cancellationToken);

        // Cargar evaluacion para DTOs con tipo+opciones y para auto-scoring
        var evaluacion = await _repoEvaluacion.ObtenerConPreguntasAsync(sesion.EvaluacionId, cancellationToken);
        var preguntaMap = evaluacion?.Preguntas.ToDictionary(p => p.Id)
            ?? new Dictionary<Guid, Pregunta>();

        if (sesion.Estado == EstadoSesion.Completada)
            await CrearResultadoSiNoExisteAsync(sesion, evaluacion, preguntaMap, cancellationToken);

        var preguntaSiguiente = sesion.ObtenerPreguntaActual();
        var dto = preguntaSiguiente == null
            ? null
            : IniciarSesionCommandHandler.BuildPreguntaDto(preguntaSiguiente, preguntaMap);

        return new RegistrarRespuestaResponse(
            SesionId: sesion.Id,
            PreguntaSiguiente: dto,
            PreguntasRespondidas: respondidas,
            TotalPreguntas: total,
            SesionCompletada: sesion.Estado == EstadoSesion.Completada
        );
    }

    private async Task CrearResultadoSiNoExisteAsync(
        SesionEvaluacion sesion,
        Evaluacion? evaluacion,
        Dictionary<Guid, Pregunta> preguntaMap,
        CancellationToken cancellationToken)
    {
        var existente = await _repoResultado.ObtenerPorSesionAsync(sesion.Id, cancellationToken);
        if (existente != null) return;

        var tiempoTotal = sesion.CompletadaEn.HasValue && sesion.IniciadaEn.HasValue
            ? (int)(sesion.CompletadaEn.Value - sesion.IniciadaEn.Value).TotalSeconds
            : 0;

        var resultado = ResultadoEvaluacion.Crear(
            sesionId: sesion.Id,
            evaluacionId: sesion.EvaluacionId,
            candidatoId: sesion.CandidatoId,
            nombreCandidato: _contextoUsuario.Nombre,
            tituloEvaluacion: evaluacion?.Nombre ?? "Evaluación",
            violacionesPestana: sesion.ContadorViolacionesPestana,
            tiempoTotalSegundos: tiempoTotal
        );

        foreach (var preguntaSesion in sesion.Preguntas.Where(p => p.FueRespondida).OrderBy(p => p.Orden))
        {
            preguntaMap.TryGetValue(preguntaSesion.PreguntaId, out var pregunta);
            var tipoPregunta = pregunta?.TipoPregunta.ToString() ?? "TextoLibre";
            var autoScore = CalcularPuntuacionAutomatica(pregunta, preguntaSesion.Respuesta?.Texto);

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

        resultado.CalcularPuntuacionTotal(CalcularPuntuacionMaximaEvaluacion(evaluacion));
        await _repoResultado.AgregarAsync(resultado, cancellationToken);
    }

    /// <summary>
    /// Calcula la puntuación máxima real de la evaluación sumando la mejor opción de cada pregunta de selección.
    /// </summary>
    internal static decimal CalcularPuntuacionMaximaEvaluacion(Evaluacion? evaluacion)
    {
        if (evaluacion == null) return 0m;

        decimal maxTotal = 0m;
        foreach (var pregunta in evaluacion.Preguntas)
        {
            if (pregunta.TipoPregunta == TipoPregunta.SeleccionUnica)
            {
                // Para selección única: la opción con mayor puntuación
                var maxOpcion = pregunta.Opciones
                    .Where(o => o.Puntuacion.HasValue && !o.EsRevisionManual)
                    .Select(o => (decimal)o.Puntuacion!.Value)
                    .DefaultIfEmpty(0m)
                    .Max();
                maxTotal += maxOpcion;
            }
            else if (pregunta.TipoPregunta == TipoPregunta.SeleccionMultiple)
            {
                // Para selección múltiple: suma de todas las opciones con puntuación positiva
                var sumaOpciones = pregunta.Opciones
                    .Where(o => o.Puntuacion.HasValue && !o.EsRevisionManual && o.Puntuacion.Value > 0)
                    .Sum(o => (decimal)o.Puntuacion!.Value);
                maxTotal += sumaOpciones;
            }
            // TextoLibre: no tiene puntuación automática máxima definida
        }
        return maxTotal;
    }

    /// <summary>
    /// Calcula puntuación automática para preguntas de selección basándose en las opciones elegidas.
    /// TextoLibre siempre requiere revisión manual (retorna null).
    /// </summary>
    internal static decimal? CalcularPuntuacionAutomatica(Pregunta? pregunta, string? textoRespuesta)
    {
        if (pregunta == null || string.IsNullOrEmpty(textoRespuesta)) return null;

        if (pregunta.TipoPregunta == TipoPregunta.SeleccionUnica)
        {
            if (!Guid.TryParse(textoRespuesta, out var opcionId)) return null;
            var opcion = pregunta.Opciones.FirstOrDefault(o => o.Id == opcionId);
            if (opcion == null || opcion.EsRevisionManual || opcion.Puntuacion == null) return null;
            return (decimal)opcion.Puntuacion.Value;
        }

        if (pregunta.TipoPregunta == TipoPregunta.SeleccionMultiple)
        {
            var ids = textoRespuesta.Split(',', StringSplitOptions.RemoveEmptyEntries);
            decimal suma = 0;
            bool tieneAutoScore = false;
            foreach (var idStr in ids)
            {
                if (!Guid.TryParse(idStr.Trim(), out var opcionId)) continue;
                var opcion = pregunta.Opciones.FirstOrDefault(o => o.Id == opcionId);
                if (opcion == null || opcion.EsRevisionManual || opcion.Puntuacion == null) continue;
                suma += (decimal)opcion.Puntuacion.Value;
                tieneAutoScore = true;
            }
            return tieneAutoScore ? suma : null;
        }

        return null; // TextoLibre: siempre revisión manual
    }
}
