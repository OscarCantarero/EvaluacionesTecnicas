using FluentAssertions;
using Moq;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Evaluaciones.Commands.AgregarPregunta;
using TechEval.Domain.Common.Errors;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Tests.Evaluaciones;

public class AgregarPreguntaCommandHandlerTests
{
    private readonly Mock<IRepositorioEvaluacion> _repositorioMock = new();
    private readonly AgregarPreguntaCommandHandler _handler;

    public AgregarPreguntaCommandHandlerTests()
    {
        _handler = new AgregarPreguntaCommandHandler(_repositorioMock.Object);
    }

    private static Evaluacion CrearEvaluacionValida() =>
        Evaluacion.Crear("Evaluación Test", null, "evaluador-1");

    [Fact(DisplayName = "Handle con evaluación válida debe retornar ID de la pregunta creada")]
    public async Task Handle_EvaluacionValida_DebeRetornarPreguntaId()
    {
        var evaluacion = CrearEvaluacionValida();
        var command = new AgregarPreguntaCommand(
            evaluacion.Id,
            "¿Qué es SOLID?",
            TipoPregunta.TextoLibre,
            NivelDificultad.Medio,
            null,
            false,
            true);

        _repositorioMock.Setup(r => r.ObtenerConPreguntasAsync(evaluacion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluacion);
        _repositorioMock.Setup(r => r.ActualizarAsync(It.IsAny<Evaluacion>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().NotBeEmpty();
        evaluacion.Preguntas.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handle con evaluación inexistente debe lanzar NotFoundException")]
    public async Task Handle_EvaluacionInexistente_DebeLanzarNotFoundException()
    {
        var idInexistente = Guid.NewGuid();
        _repositorioMock.Setup(r => r.ObtenerConPreguntasAsync(idInexistente, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Evaluacion?)null);

        var command = new AgregarPreguntaCommand(
            idInexistente,
            "¿Pregunta?",
            TipoPregunta.TextoLibre,
            NivelDificultad.Facil,
            null,
            false,
            false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Evaluacion*");
    }

    [Fact(DisplayName = "AgregarOpcion en pregunta de texto libre debe lanzar DomainException")]
    public async Task Handle_OpcionEnPreguntaTextoLibre_DebeLanzarDomainException()
    {
        var evaluacion = CrearEvaluacionValida();
        evaluacion.AgregarPregunta(
            "Explica el patrón Repository",
            TipoPregunta.TextoLibre,
            NivelDificultad.Medio,
            null,
            false,
            true);

        var preguntaId = evaluacion.Preguntas.First().Id;

        var act = () => evaluacion.AgregarOpcion(preguntaId, "Opción inválida", 1, false);

        act.Should().Throw<DomainException>()
            .Which.Error.Codigo.Should().Be("Evaluacion.OpcionEnPreguntaLibre");
    }

    [Fact(DisplayName = "Handle debe persistir múltiples preguntas secuencialmente sin error de concurrencia")]
    public async Task Handle_MultiplesPreguntas_DebePersistirSinConcurrencia()
    {
        var evaluacion = CrearEvaluacionValida();

        _repositorioMock.Setup(r => r.ObtenerConPreguntasAsync(evaluacion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluacion);
        _repositorioMock.Setup(r => r.ActualizarAsync(It.IsAny<Evaluacion>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var preguntas = new[]
        {
            new AgregarPreguntaCommand(evaluacion.Id, "Pregunta 1", TipoPregunta.TextoLibre, NivelDificultad.Facil, null, false, true),
            new AgregarPreguntaCommand(evaluacion.Id, "Pregunta 2", TipoPregunta.SeleccionUnica, NivelDificultad.Medio, 60, false, false),
            new AgregarPreguntaCommand(evaluacion.Id, "Pregunta 3", TipoPregunta.SeleccionMultiple, NivelDificultad.Dificil, 120, true, false),
        };

        foreach (var command in preguntas)
        {
            var id = await _handler.Handle(command, CancellationToken.None);
            id.Should().NotBeEmpty();
        }

        evaluacion.Preguntas.Should().HaveCount(3);
        _repositorioMock.Verify(r => r.ActualizarAsync(evaluacion, It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact(DisplayName = "Handle debe asignar orden incremental a cada pregunta")]
    public async Task Handle_MultiplesPreguntas_OrdenIncremental()
    {
        var evaluacion = CrearEvaluacionValida();

        _repositorioMock.Setup(r => r.ObtenerConPreguntasAsync(evaluacion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluacion);
        _repositorioMock.Setup(r => r.ActualizarAsync(It.IsAny<Evaluacion>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(new AgregarPreguntaCommand(
            evaluacion.Id, "Primera", TipoPregunta.TextoLibre, NivelDificultad.Facil, null, false, false), CancellationToken.None);
        await _handler.Handle(new AgregarPreguntaCommand(
            evaluacion.Id, "Segunda", TipoPregunta.TextoLibre, NivelDificultad.Medio, null, false, false), CancellationToken.None);

        var preguntas = evaluacion.Preguntas.ToList();
        preguntas[0].Orden.Should().Be(1);
        preguntas[1].Orden.Should().Be(2);
    }
}
