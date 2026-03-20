using FluentAssertions;
using Moq;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;
using TechEval.Application.Resultados.Commands.CompletarRevision;
using TechEval.Application.Resultados.Commands.EvaluarConIA;
using TechEval.Application.Resultados.Commands.GenerarPDF;
using TechEval.Domain.Evaluaciones.Repositorios;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Application.Tests.Resultados;
public class EvaluarConIACommandHandlerTests
{
    private readonly Mock<IRepositorioResultadoEvaluacion> _repoResultadoMock = new();
    private readonly Mock<IRepositorioSesionEvaluacion> _repoSesionMock = new();
    private readonly Mock<IRepositorioEvaluacion> _repoEvaluacionMock = new();
    private readonly Mock<IServicioEvaluacionIA> _servicioIAMock = new();
    private readonly EvaluarConIACommandHandler _handler;

    public EvaluarConIACommandHandlerTests()
    {
        _handler = new EvaluarConIACommandHandler(
            _repoResultadoMock.Object,
            _repoSesionMock.Object,
            _repoEvaluacionMock.Object,
            _servicioIAMock.Object);
    }

    [Fact(DisplayName = "Handle con datos válidos debe retornar respuesta IA y registrar sugerencia")]
    public async Task Handle_DatosValidos_DebeRetornarRespuestaIA()
    {
        var sesionId = Guid.NewGuid();
        var preguntaId = Guid.NewGuid();
        var resultado = ResultadoEvaluacion.Crear(
            sesionId, Guid.NewGuid(), "c1", "Ana", "Eval Test", 0, 300);
        var puntuacion = PuntuacionPregunta.Crear(preguntaId, 1, "TextoLibre", "DI es un patrón...");
        resultado.AgregarPuntuacion(puntuacion);

        _repoResultadoMock
            .Setup(r => r.ObtenerPorSesionAsync(sesionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultado);

        _repoSesionMock
            .Setup(r => r.ObtenerConPreguntasAsync(sesionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Sesiones.SesionEvaluacion?)null);

        _servicioIAMock
            .Setup(s => s.EvaluarRespuestaAsync(It.IsAny<SolicitudEvaluacionIA>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RespuestaEvaluacionIA(
                PreguntaId: preguntaId,
                PuntajeSugerido: 8.0m,
                EscalaPuntuacion: 10,
                Justificacion: "Buena respuesta sobre DI",
                RequiereRevisionManual: true,
                AspectosPositivos: ["Claridad"],
                AspectosNegativos: ["Faltó ejemplo"]));

        var command = new EvaluarConIACommand(sesionId, preguntaId, "Evalúa como senior dev", 10);
        var response = await _handler.Handle(command, CancellationToken.None);

        response.PuntajeSugerido.Should().Be(8.0m);
        response.Justificacion.Should().Contain("DI");
        response.AspectosPositivos.Should().Contain("Claridad");
        puntuacion.PuntuacionIASugerida.Should().Be(8.0m);
        _repoResultadoMock.Verify(r => r.ActualizarAsync(resultado, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handle con resultado no encontrado debe lanzar NotFoundException")]
    public async Task Handle_ResultadoNoEncontrado_DebeLanzarNotFoundException()
    {
        _repoResultadoMock
            .Setup(r => r.ObtenerPorSesionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResultadoEvaluacion?)null);

        var command = new EvaluarConIACommand(Guid.NewGuid(), Guid.NewGuid(), "Contexto");
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = "Handle con pregunta no encontrada debe lanzar NotFoundException")]
    public async Task Handle_PreguntaNoEncontrada_DebeLanzarNotFoundException()
    {
        var sesionId = Guid.NewGuid();
        var resultado = ResultadoEvaluacion.Crear(
            sesionId, Guid.NewGuid(), "c1", "Ana", "Eval", 0, 300);

        _repoResultadoMock
            .Setup(r => r.ObtenerPorSesionAsync(sesionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultado);

        var command = new EvaluarConIACommand(sesionId, Guid.NewGuid(), "Contexto");
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

public class CompletarRevisionCommandHandlerTests
{
    private readonly Mock<IRepositorioResultadoEvaluacion> _repoMock = new();
    private readonly Mock<IServicioGeneradorPDF> _pdfMock = new();
    private readonly Mock<IServicioEmail> _emailMock = new();
    private readonly Mock<IServicioAuth> _authMock = new();
    private readonly Mock<IServicioArchivos> _archivosMock = new();
    private readonly CompletarRevisionCommandHandler _handler;

    public CompletarRevisionCommandHandlerTests()
    {
        _handler = new CompletarRevisionCommandHandler(
            _repoMock.Object,
            _pdfMock.Object,
            _emailMock.Object,
            _authMock.Object,
            _archivosMock.Object);
    }

    [Fact(DisplayName = "Handle debe completar revisión y retornar resultado exitoso")]
    public async Task Handle_DatosValidos_DebeCompletarRevision()
    {
        var sesionId = Guid.NewGuid();
        var resultado = ResultadoEvaluacion.Crear(
            sesionId, Guid.NewGuid(), "c1", "Ana", "Eval Test", 0, 300);

        _repoMock
            .Setup(r => r.ObtenerPorSesionAsync(sesionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultado);

        _pdfMock
            .Setup(p => p.GenerarPDFResultadoAsync(It.IsAny<ResultadoEvaluacion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 0x25, 0x50, 0x44, 0x46 });

        _archivosMock
            .Setup(a => a.GuardarAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("resultados/resultado.pdf");

        _authMock
            .Setup(a => a.ListarUsuariosAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsuarioDto>());

        var command = new CompletarRevisionCommand(sesionId, "evaluador-1");
        var response = await _handler.Handle(command, CancellationToken.None);

        response.Exito.Should().BeTrue();
        response.EstadoRevision.Should().Be("Completada");
        resultado.EstadoRevision.Should().Be(EstadoRevision.Completada);
        _repoMock.Verify(r => r.ActualizarAsync(resultado, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handle con resultado no encontrado debe lanzar NotFoundException")]
    public async Task Handle_ResultadoNoEncontrado_DebeLanzarNotFoundException()
    {
        _repoMock
            .Setup(r => r.ObtenerPorSesionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResultadoEvaluacion?)null);

        var command = new CompletarRevisionCommand(Guid.NewGuid(), "evaluador-1");
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

public class GenerarPDFCommandHandlerTests
{
    private readonly Mock<IRepositorioResultadoEvaluacion> _repoMock = new();
    private readonly Mock<IServicioGeneradorPDF> _pdfMock = new();
    private readonly Mock<IServicioArchivos> _archivosMock = new();
    private readonly GenerarPDFCommandHandler _handler;

    public GenerarPDFCommandHandlerTests()
    {
        _handler = new GenerarPDFCommandHandler(
            _repoMock.Object, _pdfMock.Object, _archivosMock.Object);
    }

    [Fact(DisplayName = "Handle debe generar PDF y retornar URL")]
    public async Task Handle_DatosValidos_DebeRetornarUrlPDF()
    {
        var sesionId = Guid.NewGuid();
        var resultado = ResultadoEvaluacion.Crear(
            sesionId, Guid.NewGuid(), "c1", "Ana", "Eval", 0, 300);

        _repoMock
            .Setup(r => r.ObtenerPorSesionAsync(sesionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultado);

        _pdfMock
            .Setup(p => p.GenerarPDFResultadoAsync(resultado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 0x25, 0x50, 0x44, 0x46 });

        _archivosMock
            .Setup(a => a.GuardarAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("resultados/resultado.pdf");

        _archivosMock
            .Setup(a => a.ObtenerUrlPublica("resultados/resultado.pdf"))
            .Returns("/uploads/resultados/resultado.pdf");

        var command = new GenerarPDFCommand(sesionId);
        var response = await _handler.Handle(command, CancellationToken.None);

        response.ExitoGeneration.Should().BeTrue();
        response.UrlPDF.Should().Contain("resultado.pdf");
        _pdfMock.Verify(p => p.GenerarPDFResultadoAsync(resultado, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handle con resultado no encontrado debe lanzar NotFoundException")]
    public async Task Handle_ResultadoNoEncontrado_DebeLanzarNotFoundException()
    {
        _repoMock
            .Setup(r => r.ObtenerPorSesionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResultadoEvaluacion?)null);

        var command = new GenerarPDFCommand(Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
