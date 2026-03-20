using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Sesiones.Queries.ObtenerSesion;

public sealed class ObtenerSesionQueryHandler : IRequestHandler<ObtenerSesionQuery, ObtenerSesionResponse>
{
    private readonly IRepositorioSesionEvaluacion _repo;
    private readonly IContextoUsuario _contextoUsuario;

    public ObtenerSesionQueryHandler(
        IRepositorioSesionEvaluacion repo,
        IContextoUsuario contextoUsuario)
    {
        _repo = repo;
        _contextoUsuario = contextoUsuario;
    }

    public async Task<ObtenerSesionResponse> Handle(
        ObtenerSesionQuery query,
        CancellationToken cancellationToken)
    {
        var sesion = await _repo.ObtenerConPreguntasAsync(query.SesionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Sesiones.SesionEvaluacion), query.SesionId);

        if (sesion.CandidatoId != _contextoUsuario.UsuarioId)
            throw new UnauthorizedException("No tienes acceso a esta sesión");

        var progreso = sesion.ObtenerProgreso();
        var (respondidas, total) = progreso;

        var preguntas = sesion.Preguntas.Select(p => new PreguntaSesionConRespuestaDto(
            Id: p.Id,
            PreguntaId: p.PreguntaId,
            Texto: p.Texto,
            Orden: p.Orden,
            LimiteTiempoSegundos: p.LimiteTiempoSegundos,
            Respuesta: p.Respuesta == null ? null : new RespuestaCandidatoDto(
                Texto: p.Respuesta.Texto,
                UrlAdjunto: p.Respuesta.UrlAdjunto,
                TiempoEmpleadoSegundos: p.Respuesta.TiempoEmpleadoSegundos,
                BrindadaEn: p.Respuesta.BrindadaEn,
                FueExpirado: p.Respuesta.FueExpiratoElTiempo
            ),
            TiempoExpirado: p.TiempoExpirado
        )).ToList();

        return new ObtenerSesionResponse(
            SesionId: sesion.Id,
            EvaluacionId: sesion.EvaluacionId,
            Estado: sesion.Estado.ToString(),
            CreadaEn: sesion.CreadaEn,
            IniciadaEn: sesion.IniciadaEn,
            CompletadaEn: sesion.CompletadaEn,
            ContadorViolacionesPestana: sesion.ContadorViolacionesPestana,
            Preguntas: preguntas,
            ProgresoTotal: total,
            ProgresoRespondidas: respondidas
        );
    }
}
