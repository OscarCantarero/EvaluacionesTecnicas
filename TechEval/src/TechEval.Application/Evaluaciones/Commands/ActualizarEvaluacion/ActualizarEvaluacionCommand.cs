using MediatR;
using TechEval.Domain.Evaluaciones;

namespace TechEval.Application.Evaluaciones.Commands.ActualizarEvaluacion;

public sealed record ActualizarEvaluacionCommand(
    Guid Id,
    string Nombre,
    string? Descripcion,
    bool OrdenAleatorio,
    bool OrdenPorDificultad,
    // Épica 8: Selección dinámica
    ModoSeleccionPreguntas ModoSeleccionPreguntas = ModoSeleccionPreguntas.Fijas,
    int? CantidadPreguntasSesion = null,
    int DistribucionFacil = 0,
    int DistribucionMedio = 0,
    int DistribucionDificil = 0
) : IRequest;
