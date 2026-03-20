using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones.Repositorios;
using TechEval.Domain.Sesiones;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Sesiones.Commands.CrearSesionesMasivas;

public sealed record CrearSesionesMasivasCommand(
    Guid EvaluacionId,
    List<string> CandidatoIds
) : IRequest<CrearSesionesMasivasResponse>;

public sealed record CrearSesionesMasivasResponse(
    int TotalCreadas,
    List<SesionCreadaDto> Sesiones
);

public sealed record SesionCreadaDto(
    Guid SesionId,
    string CandidatoId,
    string CodigoAcceso
);

public sealed class CrearSesionesMasivasCommandHandler : IRequestHandler<CrearSesionesMasivasCommand, CrearSesionesMasivasResponse>
{
    private readonly IRepositorioEvaluacion _repoEvaluacion;
    private readonly IRepositorioSesionEvaluacion _repoSesion;

    public CrearSesionesMasivasCommandHandler(
        IRepositorioEvaluacion repoEvaluacion,
        IRepositorioSesionEvaluacion repoSesion)
    {
        _repoEvaluacion = repoEvaluacion;
        _repoSesion = repoSesion;
    }

    public async Task<CrearSesionesMasivasResponse> Handle(
        CrearSesionesMasivasCommand command,
        CancellationToken cancellationToken)
    {
        var evaluacion = await _repoEvaluacion.ObtenerConPreguntasAsync(command.EvaluacionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Evaluaciones.Evaluacion), command.EvaluacionId);

        if (!evaluacion.Preguntas.Any())
            throw new InvalidOperationException("La evaluación no tiene preguntas configuradas.");

        var sesionesCreadas = new List<SesionCreadaDto>();

        foreach (var candidatoId in command.CandidatoIds.Distinct())
        {
            var preguntas = evaluacion.Preguntas
                .Select(p => (p.Id, p.Texto, p.Orden, p.LimiteTiempoSegundos))
                .ToList();

            // Aplicar orden aleatorio o por dificultad
            if (evaluacion.OrdenAleatorio)
            {
                preguntas = preguntas.OrderBy(_ => Random.Shared.Next()).ToList();
                for (var i = 0; i < preguntas.Count; i++)
                    preguntas[i] = (preguntas[i].Id, preguntas[i].Texto, i + 1, preguntas[i].LimiteTiempoSegundos);
            }
            else if (evaluacion.OrdenPorDificultad)
            {
                var preguntasDominio = evaluacion.Preguntas.ToList();
                var ordenadas = preguntasDominio
                    .OrderBy(p => p.NivelDificultad)
                    .Select((p, i) => (p.Id, p.Texto, Orden: i + 1, p.LimiteTiempoSegundos))
                    .ToList();
                preguntas = ordenadas;
            }

            var sesion = SesionEvaluacion.Crear(command.EvaluacionId, candidatoId, preguntas);
            await _repoSesion.AgregarAsync(sesion, cancellationToken);

            sesionesCreadas.Add(new SesionCreadaDto(
                SesionId: sesion.Id,
                CandidatoId: candidatoId,
                CodigoAcceso: sesion.CodigoAcceso!
            ));
        }

        return new CrearSesionesMasivasResponse(
            TotalCreadas: sesionesCreadas.Count,
            Sesiones: sesionesCreadas
        );
    }
}
