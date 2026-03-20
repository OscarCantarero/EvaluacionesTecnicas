using FluentAssertions;
using Moq;
using TechEval.Application.Common.Interfaces;
using TechEval.Application.Evaluaciones.Commands.CrearEvaluacion;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Tests.Evaluaciones;

public class CrearEvaluacionCommandHandlerTests
{
    private readonly Mock<IRepositorioEvaluacion> _repositorioMock = new();
    private readonly Mock<IContextoUsuario> _contextoMock = new();
    private readonly CrearEvaluacionCommandHandler _handler;

    public CrearEvaluacionCommandHandlerTests()
    {
        _contextoMock.Setup(c => c.UsuarioId).Returns("usuario-evaluador-1");
        _handler = new CrearEvaluacionCommandHandler(_repositorioMock.Object, _contextoMock.Object);
    }

    [Fact(DisplayName = "Handle con comando válido debe retornar el ID de la evaluación creada")]
    public async Task Handle_ComandoValido_DebeRetornarEvaluacionId()
    {
        var command = new CrearEvaluacionCommand("Test Backend", "Evaluación técnica .NET", false, false);
        _repositorioMock.Setup(r => r.AgregarAsync(It.IsAny<Evaluacion>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Should().NotBeEmpty();
        _repositorioMock.Verify(r => r.AgregarAsync(It.Is<Evaluacion>(e =>
            e.Nombre == "Test Backend" &&
            e.CreadoPor == "usuario-evaluador-1"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handle con orden aleatorio activa la propiedad correcta")]
    public async Task Handle_ConOrdenAleatorio_DebeActivarOrdenAleatorio()
    {
        var command = new CrearEvaluacionCommand("Test", null, true, false);
        Evaluacion? evaluacionCapturada = null;
        _repositorioMock.Setup(r => r.AgregarAsync(It.IsAny<Evaluacion>(), It.IsAny<CancellationToken>()))
            .Callback<Evaluacion, CancellationToken>((e, _) => evaluacionCapturada = e)
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        evaluacionCapturada!.OrdenAleatorio.Should().BeTrue();
        evaluacionCapturada.OrdenPorDificultad.Should().BeFalse();
    }
}

public class CrearEvaluacionCommandValidatorTests
{
    private readonly CrearEvaluacionCommandValidator _validator = new();

    [Fact(DisplayName = "Nombre vacío debe fallar la validación")]
    public async Task Validar_NombreVacio_DebeFallar()
    {
        var command = new CrearEvaluacionCommand("", null, false, false);
        var resultado = await _validator.ValidateAsync(command);
        resultado.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Orden aleatorio y progresivo simultáneos deben fallar la validación")]
    public async Task Validar_OrdenAleatorioYProgresivo_DebeFallar()
    {
        var command = new CrearEvaluacionCommand("Nombre válido", null, true, true);
        var resultado = await _validator.ValidateAsync(command);
        resultado.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Comando válido debe pasar la validación")]
    public async Task Validar_ComandoValido_DebeSerValido()
    {
        var command = new CrearEvaluacionCommand("Evaluación .NET", "Descripción", false, true);
        var resultado = await _validator.ValidateAsync(command);
        resultado.IsValid.Should().BeTrue();
    }
}
