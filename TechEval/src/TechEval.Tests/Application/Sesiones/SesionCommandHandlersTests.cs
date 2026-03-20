using FluentAssertions;
using Moq;
using TechEval.Application.Sesiones.Commands.CrearSesion;
using TechEval.Application.Sesiones.Commands.IniciarSesion;
using TechEval.Application.Sesiones.Commands.RegistrarRespuesta;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Sesiones;
using TechEval.Domain.Usuarios;
using TechEval.Domain.Evaluaciones;
using Xunit;

namespace TechEval.Tests.Application.Sesiones;

/// <summary>
/// Tests de los handlers de CQRS para sesiones.
/// 
/// Escenarios:
/// - CrearSesionCommandHandler
/// - IniciarSesionCommandHandler  
/// - RegistrarRespuestaCommandHandler
/// </summary>
public sealed class CrearSesionCommandHandlerTests
{
    private readonly Mock<IRepositorioSesionEvaluacion> _repositorioSesionMock;
    private readonly Mock<IRepositorioEvaluacion> _repositorioEvaluacionMock;
    private readonly Mock<IRepositorioUsuario> _repositorioUsuarioMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public CrearSesionCommandHandlerTests()
    {
        _repositorioSesionMock = new Mock<IRepositorioSesionEvaluacion>();
        _repositorioEvaluacionMock = new Mock<IRepositorioEvaluacion>();
        _repositorioUsuarioMock = new Mock<IRepositorioUsuario>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task Handle_ConEvaluacionValida_DebeCrearSesion()
    {
        // Arrange
        var evaluacionId = Guid.NewGuid();
        var candidatoId = Guid.NewGuid();

        // Crear evaluación mock con preguntas
        var preguntas = new[]
        {
            Pregunta.Crear(
                evaluacionId,
                1,
                "multiple",
                "¿Qué es SOLID?",
                "media",
                60,
                ["A", "B", "C"],
                "A"
            )
        };

        var evaluacion = Evaluacion.Crear(
            titulo: "Evaluación Test",
            descripcion: "Test",
            creadorId: Guid.NewGuid(),
            preguntas: preguntas
        );

        _repositorioEvaluacionMock
            .Setup(r => r.ObtenerPorIdAsync(evaluacionId))
            .ReturnsAsync(evaluacion);

        _repositorioSesionMock
            .Setup(r => r.CrearAsync(It.IsAny<SesionEvaluacion>()))
            .ReturnsAsync((SesionEvaluacion s) => s);

        var handler = new CrearSesionCommandHandler(
            _repositorioSesionMock.Object,
            _repositorioEvaluacionMock.Object,
            _unitOfWorkMock.Object
        );

        var command = new CrearSesionCommand(
            EvaluacionId: evaluacionId,
            CandidatoId: candidatoId,
            TiempoMaximoMinutos: 30,
            ModoSeleccionPreguntas: "random"
        );

        // Act
        var resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.SesionId.Should().NotBe(Guid.Empty);
        resultado.CodigoAcceso.Should().NotBeNullOrEmpty();
        resultado.CodigoAcceso.Length.Should().Be(8);

        _repositorioSesionMock.Verify(
            r => r.CrearAsync(It.IsAny<SesionEvaluacion>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_EvaluacionNoEncontrada_DebeLanzarExcepcion()
    {
        // Arrange
        _repositorioEvaluacionMock
            .Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Evaluacion?)null);

        var handler = new CrearSesionCommandHandler(
            _repositorioSesionMock.Object,
            _repositorioEvaluacionMock.Object,
            _unitOfWorkMock.Object
        );

        var command = new CrearSesionCommand(
            EvaluacionId: Guid.NewGuid(),
            CandidatoId: Guid.NewGuid(),
            TiempoMaximoMinutos: 30,
            ModoSeleccionPreguntas: "random"
        );

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }
}

public sealed class IniciarSesionCommandHandlerTests
{
    private readonly Mock<IRepositorioSesionEvaluacion> _repositorioMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public IniciarSesionCommandHandlerTests()
    {
        _repositorioMock = new Mock<IRepositorioSesionEvaluacion>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task Handle_SesionValida_DebeIniciarYRetornarPrimeraPreg()
    {
        // Arrange
        var sesionId = Guid.NewGuid();
        var pregunta = Pregunta.Crear(
            Guid.NewGuid(),
            1,
            "multiple",
            "¿Pregunta?",
            "media",
            60,
            ["A", "B"],
            "A"
        );

        var sesion = SesionEvaluacion.Crear(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new[] { pregunta },
            30,
            "random"
        );

        _repositorioMock
            .Setup(r => r.ObtenerPorIdAsync(sesionId))
            .ReturnsAsync(sesion);

        _repositorioMock
            .Setup(r => r.ActualizarAsync(It.IsAny<SesionEvaluacion>()))
            .ReturnsAsync(sesion);

        var handler = new IniciarSesionCommandHandler(
            _repositorioMock.Object,
            _unitOfWorkMock.Object
        );

        var command = new IniciarSesionCommand(
            SesionId: sesionId,
            CodigoAcceso: sesion.CodigoAcceso
        );

        // Act
        var resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.PreguntaActual.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_CodigoAccesoInvalido_DebeLanzarExcepcion()
    {
        // Arrange
        var sesionId = Guid.NewGuid();
        var sesion = SesionEvaluacion.Crear(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new[] { Pregunta.Crear(Guid.NewGuid(), 1, "multiple", "?", "media", 60, ["A"], "A") },
            30,
            "random"
        );

        _repositorioMock
            .Setup(r => r.ObtenerPorIdAsync(sesionId))
            .ReturnsAsync(sesion);

        var handler = new IniciarSesionCommandHandler(
            _repositorioMock.Object,
            _unitOfWorkMock.Object
        );

        var command = new IniciarSesionCommand(
            SesionId: sesionId,
            CodigoAcceso: "INVALIDCODE"
        );

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }
}

public sealed class RegistrarRespuestaCommandHandlerTests
{
    private readonly Mock<IRepositorioSesionEvaluacion> _repositorioMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public RegistrarRespuestaCommandHandlerTests()
    {
        _repositorioMock = new Mock<IRepositorioSesionEvaluacion>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task Handle_RespuestaValida_DebeRegistrarYAvanzar()
    {
        // Arrange
        var pregunta1 = Pregunta.Crear(
            Guid.NewGuid(), 1, "multiple", "Pregunta 1?", "media", 60, ["A", "B"], "A"
        );
        var pregunta2 = Pregunta.Crear(
            Guid.NewGuid(), 2, "multiple", "Pregunta 2?", "media", 60, ["A", "B"], "A"
        );

        var sesion = SesionEvaluacion.Crear(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new[] { pregunta1, pregunta2 },
            30,
            "random"
        );
        sesion.Iniciar();

        var sesionId = sesion.Id;
        var preguntaId = pregunta1.Id;

        _repositorioMock
            .Setup(r => r.ObtenerPorIdAsync(sesionId))
            .ReturnsAsync(sesion);

        _repositorioMock
            .Setup(r => r.ActualizarAsync(It.IsAny<SesionEvaluacion>()))
            .ReturnsAsync(sesion);

        var handler = new RegistrarRespuestaCommandHandler(
            _repositorioMock.Object,
            _unitOfWorkMock.Object
        );

        var command = new RegistrarRespuestaCommand(
            SesionId: sesionId,
            PreguntaId: preguntaId,
            Texto: "Respuesta A",
            TiempoEmpleadoSegundos: 30,
            FueExpirado: false
        );

        // Act
        var resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.SesionCompletada.Should().BeFalse();
    }
}
