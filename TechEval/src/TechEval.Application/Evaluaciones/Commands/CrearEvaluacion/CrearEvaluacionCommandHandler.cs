using MediatR;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Evaluaciones.Commands.CrearEvaluacion;

public sealed class CrearEvaluacionCommandHandler(
    IRepositorioEvaluacion repositorio,
    IContextoUsuario contextoUsuario)
    : IRequestHandler<CrearEvaluacionCommand, Guid>
{
    public async Task<Guid> Handle(CrearEvaluacionCommand command, CancellationToken cancellationToken)
    {
        var evaluacion = Evaluacion.Crear(command.Nombre, command.Descripcion, contextoUsuario.UsuarioId);

        if (command.OrdenAleatorio)
            evaluacion.ActivarOrdenAleatorio();
        else if (command.OrdenPorDificultad)
            evaluacion.ActivarOrdenPorDificultad();

        await repositorio.AgregarAsync(evaluacion, cancellationToken);
        return evaluacion.Id;
    }
}
