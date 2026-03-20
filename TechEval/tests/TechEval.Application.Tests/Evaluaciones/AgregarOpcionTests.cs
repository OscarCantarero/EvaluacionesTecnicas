using FluentAssertions;
using Moq;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Evaluaciones.Commands.AgregarOpcion;
using TechEval.Domain.Common.Errors;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Tests.Evaluaciones;

public class AgregarOpcionCommandHandlerTests
{
    private readonly Mock<IRepositorioEvaluacion> _repositorioMock = new();
    private readonly AgregarOpcionCommandHandler _handler;

    public AgregarOpcionCommandHandlerTests()
    {
        _handler = new AgregarOpcionCommandHandler(_repositorioMock.Object);
    }

    private static Evaluacion CrearEvaluacionConPreguntaSeleccion()
    {
        var evaluacion = Evaluacion.Crear("Evaluación Test", null, "evaluador-1");
        evaluacion.AgregarPregunta(
            "¿Cuál es el principio S de SOLID?",
            TipoPregunta.SeleccionUnica,
            NivelDificultad.Medio,
            null, false, false);
        return evaluacion;
    }

    private static Evaluacion CrearEvaluacionConPreguntaTextoLibre()
    {
        var evaluacion = Evaluacion.Crear("Evaluación Test", null, "evaluador-1");
        evaluacion.AgregarPregunta(
            "Explica el patrón Repository",
            TipoPregunta.TextoLibre,
            NivelDificultad.Medio,
            null, false, true);
        return evaluacion;
    }

    [Fact(DisplayName = "Handle con pregunta de selección debe agregar opción y retornar ID")]
    public async Task Handle_PreguntaSeleccion_DebeRetornarOpcionId()
    {
        var evaluacion = CrearEvaluacionConPreguntaSeleccion();
        var preguntaId = evaluacion.Preguntas.First().Id;

        var command = new AgregarOpcionCommand(
            evaluacion.Id, preguntaId, "Single Responsibility", 10, false);

        _repositorioMock.Setup(r => r.ObtenerConPreguntasAsync(evaluacion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluacion);
        _repositorioMock.Setup(r => r.ActualizarAsync(It.IsAny<Evaluacion>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().NotBeEmpty();
        evaluacion.Preguntas.First().Opciones.Should().HaveCount(1);
        _repositorioMock.Verify(r => r.ActualizarAsync(evaluacion, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handle debe persistir múltiples opciones secuencialmente sin error de concurrencia")]
    public async Task Handle_MultiplesOpciones_DebePersistirSinConcurrencia()
    {
        var evaluacion = CrearEvaluacionConPreguntaSeleccion();
        var preguntaId = evaluacion.Preguntas.First().Id;

        _repositorioMock.Setup(r => r.ObtenerConPreguntasAsync(evaluacion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluacion);
        _repositorioMock.Setup(r => r.ActualizarAsync(It.IsAny<Evaluacion>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var opciones = new[]
        {
            new AgregarOpcionCommand(evaluacion.Id, preguntaId, "Opción A", 10, false),
            new AgregarOpcionCommand(evaluacion.Id, preguntaId, "Opción B", 5, false),
            new AgregarOpcionCommand(evaluacion.Id, preguntaId, "Opción C", 0, false),
        };

        foreach (var command in opciones)
        {
            var id = await _handler.Handle(command, CancellationToken.None);
            id.Should().NotBeEmpty();
        }

        evaluacion.Preguntas.First().Opciones.Should().HaveCount(3);
        _repositorioMock.Verify(r => r.ActualizarAsync(evaluacion, It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact(DisplayName = "Handle con evaluación inexistente debe lanzar NotFoundException")]
    public async Task Handle_EvaluacionInexistente_DebeLanzarNotFoundException()
    {
        var idInexistente = Guid.NewGuid();
        _repositorioMock.Setup(r => r.ObtenerConPreguntasAsync(idInexistente, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Evaluacion?)null);

        var command = new AgregarOpcionCommand(
            idInexistente, Guid.NewGuid(), "Opción", 10, false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Evaluacion*");
    }

    [Fact(DisplayName = "Handle con pregunta de texto libre debe lanzar DomainException")]
    public async Task Handle_PreguntaTextoLibre_DebeLanzarDomainException()
    {
        var evaluacion = CrearEvaluacionConPreguntaTextoLibre();
        var preguntaId = evaluacion.Preguntas.First().Id;

        var command = new AgregarOpcionCommand(
            evaluacion.Id, preguntaId, "Opción inválida", 10, false);

        _repositorioMock.Setup(r => r.ObtenerConPreguntasAsync(evaluacion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluacion);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Error.Codigo == "Evaluacion.OpcionEnPreguntaLibre");
    }

    [Fact(DisplayName = "Handle con pregunta inexistente debe lanzar DomainException")]
    public async Task Handle_PreguntaInexistente_DebeLanzarDomainException()
    {
        var evaluacion = CrearEvaluacionConPreguntaSeleccion();
        var preguntaIdFalso = Guid.NewGuid();

        var command = new AgregarOpcionCommand(
            evaluacion.Id, preguntaIdFalso, "Opción", 5, false);

        _repositorioMock.Setup(r => r.ObtenerConPreguntasAsync(evaluacion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluacion);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Error.Codigo == "Evaluacion.PreguntaNoEncontrada");
    }

    [Fact(DisplayName = "Handle con puntaje y revisión manual debe lanzar DomainException")]
    public async Task Handle_PuntajeConRevisionManual_DebeLanzarDomainException()
    {
        var evaluacion = CrearEvaluacionConPreguntaSeleccion();
        var preguntaId = evaluacion.Preguntas.First().Id;

        var command = new AgregarOpcionCommand(
            evaluacion.Id, preguntaId, "Opción", 10, true);

        _repositorioMock.Setup(r => r.ObtenerConPreguntasAsync(evaluacion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluacion);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.Error.Codigo == "Evaluacion.PuntuacionConRevisionManual");
    }
}
