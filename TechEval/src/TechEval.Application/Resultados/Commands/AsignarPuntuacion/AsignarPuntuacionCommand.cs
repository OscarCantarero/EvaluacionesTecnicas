using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Application.Sesiones.Commands.RegistrarRespuesta;
using TechEval.Domain.Evaluaciones.Repositorios;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;

namespace TechEval.Application.Resultados.Commands.AsignarPuntuacion;

public sealed record AsignarPuntuacionCommand(
    Guid SesionId,
    Guid PreguntaId,
    decimal Puntaje,
    string? Observaciones = null
) : IRequest<AsignarPuntuacionResponse>;

public sealed record AsignarPuntuacionResponse(
    bool ExitoAsignacion,
    decimal PuntajeTotal,
    decimal PorcentajeObtenido,
    string Mensaje
);

public sealed class AsignarPuntuacionCommandHandler : IRequestHandler<AsignarPuntuacionCommand, AsignarPuntuacionResponse>
{
    private readonly IRepositorioResultadoEvaluacion _repositorio;
    private readonly IRepositorioEvaluacion _repoEvaluacion;

    public AsignarPuntuacionCommandHandler(
        IRepositorioResultadoEvaluacion repositorio,
        IRepositorioEvaluacion repoEvaluacion)
    {
        _repositorio = repositorio;
        _repoEvaluacion = repoEvaluacion;
    }

    public async Task<AsignarPuntuacionResponse> Handle(AsignarPuntuacionCommand request, CancellationToken cancellationToken)
    {
        var resultado = await _repositorio.ObtenerPorSesionAsync(request.SesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ResultadoEvaluacion), request.SesionId);

        var puntuacion = resultado.ObtenerPuntuacion(request.PreguntaId)
            ?? throw new NotFoundException(nameof(PuntuacionPregunta), request.PreguntaId);

        // Asignar puntuación manual
        puntuacion.AsignarPuntuacionManual(request.Puntaje, request.Observaciones);

        // Recalcular total con puntuación máxima real
        var evaluacion = await _repoEvaluacion.ObtenerConPreguntasAsync(resultado.EvaluacionId, cancellationToken);
        resultado.CalcularPuntuacionTotal(RegistrarRespuestaCommandHandler.CalcularPuntuacionMaximaEvaluacion(evaluacion));

        // Persistir cambios
        await _repositorio.ActualizarAsync(resultado, cancellationToken);

        return new AsignarPuntuacionResponse(
            ExitoAsignacion: true,
            PuntajeTotal: resultado.PuntuacionTotal,
            PorcentajeObtenido: resultado.PorcentajeObtenido,
            Mensaje: "Puntuación asignada exitosamente"
        );
    }
}
