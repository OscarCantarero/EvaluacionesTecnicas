using FluentAssertions;
using Moq;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Resultados.Queries.CompararCandidatos;
using TechEval.Application.Resultados.Queries.ObtenerRanking;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;

namespace TechEval.Application.Tests.Evaluaciones;

public class ComparacionRankingTests
{
    private readonly Mock<IRepositorioResultadoEvaluacion> _repositorioMock = new();

    private static ResultadoEvaluacion CrearResultado(
        Guid sesionId, Guid evaluacionId, string candidatoId, string nombre,
        decimal puntuacion, int tiempo)
    {
        var resultado = ResultadoEvaluacion.Crear(
            sesionId, evaluacionId, candidatoId, nombre, "Evaluación Test", 0, tiempo);

        var p1 = PuntuacionPregunta.Crear(Guid.NewGuid(), 1, "TextoLibre", "Respuesta");
        p1.AsignarPuntuacionManual(puntuacion);
        resultado.AgregarPuntuacion(p1);
        resultado.CalcularPuntuacionTotal(100m);
        return resultado;
    }

    // ── CompararCandidatos ──

    [Fact]
    public async Task CompararCandidatos_ConDosSesiones_RetornaTablaComparativa()
    {
        var evaluacionId = Guid.NewGuid();
        var sesionA = Guid.NewGuid();
        var sesionB = Guid.NewGuid();
        var resultados = new List<ResultadoEvaluacion>
        {
            CrearResultado(sesionA, evaluacionId, "c1", "Ana", 80, 300),
            CrearResultado(sesionB, evaluacionId, "c2", "Luis", 65, 400)
        };

        _repositorioMock
            .Setup(r => r.ObtenerPorEvaluacionAsync(evaluacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultados);

        var handler = new CompararCandidatosQueryHandler(_repositorioMock.Object);
        var query = new CompararCandidatosQuery(evaluacionId, [sesionA, sesionB]);

        var response = await handler.Handle(query, CancellationToken.None);

        response.Candidatos.Should().HaveCount(2);
        response.Candidatos[0].NombreCandidato.Should().Be("Ana"); // mayor porcentaje primero
        response.Preguntas.Should().HaveCount(1);
        response.TituloEvaluacion.Should().Be("Evaluación Test");
    }

    [Fact]
    public async Task CompararCandidatos_ConMenosDeDos_LanzaValidationException()
    {
        var handler = new CompararCandidatosQueryHandler(_repositorioMock.Object);
        var query = new CompararCandidatosQuery(Guid.NewGuid(), [Guid.NewGuid()]);

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CompararCandidatos_SinResultados_LanzaNotFoundException()
    {
        var evaluacionId = Guid.NewGuid();
        _repositorioMock
            .Setup(r => r.ObtenerPorEvaluacionAsync(evaluacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ResultadoEvaluacion>());

        var handler = new CompararCandidatosQueryHandler(_repositorioMock.Object);
        var query = new CompararCandidatosQuery(evaluacionId, [Guid.NewGuid(), Guid.NewGuid()]);

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── ObtenerRanking ──

    [Fact]
    public async Task ObtenerRanking_ConVariosCandidatos_RetornaOrdenadoPorPuntaje()
    {
        var evaluacionId = Guid.NewGuid();
        var resultados = new List<ResultadoEvaluacion>
        {
            CrearResultado(Guid.NewGuid(), evaluacionId, "c1", "Ana", 65, 400),
            CrearResultado(Guid.NewGuid(), evaluacionId, "c2", "Luis", 90, 300),
            CrearResultado(Guid.NewGuid(), evaluacionId, "c3", "María", 80, 350)
        };

        _repositorioMock
            .Setup(r => r.ObtenerPorEvaluacionAsync(evaluacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultados);

        var handler = new ObtenerRankingQueryHandler(_repositorioMock.Object);
        var query = new ObtenerRankingQuery(evaluacionId);

        var response = await handler.Handle(query, CancellationToken.None);

        response.TotalCandidatos.Should().Be(3);
        response.Ranking[0].NombreCandidato.Should().Be("Luis");
        response.Ranking[0].Posicion.Should().Be(1);
        response.Ranking[1].NombreCandidato.Should().Be("María");
        response.Ranking[2].NombreCandidato.Should().Be("Ana");
    }

    [Fact]
    public async Task ObtenerRanking_ConEmpate_DesempataPorTiempo()
    {
        var evaluacionId = Guid.NewGuid();
        var resultados = new List<ResultadoEvaluacion>
        {
            CrearResultado(Guid.NewGuid(), evaluacionId, "c1", "Ana", 80, 500),
            CrearResultado(Guid.NewGuid(), evaluacionId, "c2", "Luis", 80, 300) // mismo puntaje, menos tiempo
        };

        _repositorioMock
            .Setup(r => r.ObtenerPorEvaluacionAsync(evaluacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultados);

        var handler = new ObtenerRankingQueryHandler(_repositorioMock.Object);
        var query = new ObtenerRankingQuery(evaluacionId);

        var response = await handler.Handle(query, CancellationToken.None);

        response.Ranking[0].NombreCandidato.Should().Be("Luis"); // menos tiempo gana
        response.Ranking[1].NombreCandidato.Should().Be("Ana");
    }

    [Fact]
    public async Task ObtenerRanking_SinResultados_LanzaNotFoundException()
    {
        var evaluacionId = Guid.NewGuid();
        _repositorioMock
            .Setup(r => r.ObtenerPorEvaluacionAsync(evaluacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ResultadoEvaluacion>());

        var handler = new ObtenerRankingQueryHandler(_repositorioMock.Object);
        var query = new ObtenerRankingQuery(evaluacionId);

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
