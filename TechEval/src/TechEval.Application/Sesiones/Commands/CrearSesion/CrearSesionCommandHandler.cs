using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones.Repositorios;
using TechEval.Domain.Sesiones;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Sesiones.Commands.CrearSesion;

public sealed class CrearSesionCommandHandler : IRequestHandler<CrearSesionCommand, CrearSesionResponse>
{
    private readonly IRepositorioSesionEvaluacion _repoSesion;
    private readonly IRepositorioEvaluacion _repoEvaluacion;
    private readonly IContextoUsuario _contextoUsuario;

    public CrearSesionCommandHandler(
        IRepositorioSesionEvaluacion repoSesion,
        IRepositorioEvaluacion repoEvaluacion,
        IContextoUsuario contextoUsuario)
    {
        _repoSesion = repoSesion;
        _repoEvaluacion = repoEvaluacion;
        _contextoUsuario = contextoUsuario;
    }

    public async Task<CrearSesionResponse> Handle(
        CrearSesionCommand command,
        CancellationToken cancellationToken)
    {
        // Verificar que el evaluador tenga permiso (lo hace el autorizer en controller)
        var evaluacion = await _repoEvaluacion.ObtenerConPreguntasAsync(command.EvaluacionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Evaluaciones.Evaluacion), command.EvaluacionId);

        // Auto-activar evaluación si está en estado Borrador (Épica 9.1)
        if (evaluacion.Estado == EstadoEvaluacion.Borrador)
        {
            evaluacion.CambiarEstado(EstadoEvaluacion.Activa);
            await _repoEvaluacion.ActualizarAsync(evaluacion, cancellationToken);
        }

        // Preparar preguntas ordenadas según configuración
        var preguntasOrdenadas = evaluacion.Preguntas
            .AsEnumerable()
            .OrderBy(p => p.Orden)
            .ToList();

        if (evaluacion.OrdenAleatorio)
        {
            preguntasOrdenadas = preguntasOrdenadas
                .OrderBy(_ => Guid.NewGuid())
                .ToList();
        }
        else if (evaluacion.OrdenPorDificultad)
        {
            preguntasOrdenadas = preguntasOrdenadas
                .OrderBy(p => (int)p.NivelDificultad)
                .ToList();
        }

        var preguntasParaSesion = preguntasOrdenadas
            .Select((p, idx) => (
                PreguntaId: p.Id,
                Texto: p.Texto,
                Orden: idx + 1,
                LimiteTiempo: p.LimiteTiempoSegundos
            ))
            .ToList();

        var sesion = SesionEvaluacion.Crear(
            command.EvaluacionId,
            command.CandidatoId,
            preguntasParaSesion
        );

        await _repoSesion.AgregarAsync(sesion, cancellationToken);

        return new CrearSesionResponse(
            SesionId: sesion.Id,
            CodigoAcceso: sesion.CodigoAcceso!,
            UrlSesion: $"/evaluacion/{sesion.CodigoAcceso}"
        );
    }
}
