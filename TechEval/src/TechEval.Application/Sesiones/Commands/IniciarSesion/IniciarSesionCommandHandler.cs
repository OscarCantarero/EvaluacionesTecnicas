using MediatR;
using TechEval.Application.Common.DTOs;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;
using TechEval.Domain.Sesiones;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Sesiones.Commands.IniciarSesion;

public sealed class IniciarSesionCommandHandler : IRequestHandler<IniciarSesionCommand, IniciarSesionResponse>
{
    private readonly IRepositorioSesionEvaluacion _repo;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IRepositorioEvaluacion _repoEvaluacion;

    public IniciarSesionCommandHandler(
        IRepositorioSesionEvaluacion repo,
        IContextoUsuario contextoUsuario,
        IRepositorioEvaluacion repoEvaluacion)
    {
        _repo = repo;
        _contextoUsuario = contextoUsuario;
        _repoEvaluacion = repoEvaluacion;
    }

    public async Task<IniciarSesionResponse> Handle(
        IniciarSesionCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _repo.ObtenerPorCodigoAccesoAsync(command.CodigoAcceso, cancellationToken)
            ?? throw new NotFoundException(nameof(SesionEvaluacion), command.CodigoAcceso);

        if (sesion.CandidatoId != _contextoUsuario.UsuarioId)
            throw new UnauthorizedException("No tienes acceso a esta sesión");

        // Idempotente: si ya está en progreso, devuelve el estado actual (p.ej. al recargar la página)
        if (sesion.Estado == EstadoSesion.NoIniciada)
        {
            sesion.Iniciar();
            await _repo.ActualizarAsync(sesion, cancellationToken);
        }

        var evaluacion = await _repoEvaluacion.ObtenerConPreguntasAsync(sesion.EvaluacionId, cancellationToken);
        var preguntaMap = evaluacion?.Preguntas.ToDictionary(p => p.Id)
            ?? new Dictionary<Guid, Pregunta>();

        var progreso = sesion.ObtenerProgreso();
        var (respondidas, total) = progreso;
        var preguntaActual = sesion.ObtenerPreguntaActual();

        var dto = preguntaActual == null ? null : BuildPreguntaDto(preguntaActual, preguntaMap);

        return new IniciarSesionResponse(
            SesionId: sesion.Id,
            PreguntaActual: dto,
            TotalPreguntas: total,
            PreguntasRespondidas: respondidas
        );
    }

    internal static PreguntaSesionDto BuildPreguntaDto(
        PreguntaSesion preguntaSesion,
        Dictionary<Guid, Pregunta> preguntaMap)
    {
        preguntaMap.TryGetValue(preguntaSesion.PreguntaId, out var pregunta);
        var tipo = pregunta?.TipoPregunta.ToString() ?? "TextoLibre";
        var opciones = pregunta is not null && pregunta.TipoPregunta != TipoPregunta.TextoLibre
            ? pregunta.Opciones.Select(o => new OpcionPreguntaDto(o.Id, o.Texto)).ToList()
            : new List<OpcionPreguntaDto>();

        return new PreguntaSesionDto(
            PreguntaId: preguntaSesion.PreguntaId,
            Texto: preguntaSesion.Texto,
            Orden: preguntaSesion.Orden,
            LimiteTiempoSegundos: preguntaSesion.LimiteTiempoSegundos,
            TipoPregunta: tipo,
            Opciones: opciones
        );
    }
}
