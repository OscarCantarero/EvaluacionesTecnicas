using FluentAssertions;
using Moq;
using TechEval.Application.Common.Interfaces;
using TechEval.Application.Evaluaciones.Queries.ListarEvaluaciones;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Tests.Evaluaciones;

public class ListarEvaluacionesQueryHandlerTests
{
    private readonly Mock<IRepositorioEvaluacion> _repositorioMock = new();
    private readonly Mock<IContextoUsuario> _contextoMock = new();
    private readonly ListarEvaluacionesQueryHandler _handler;

    public ListarEvaluacionesQueryHandlerTests()
    {
        _contextoMock.Setup(c => c.UsuarioId).Returns("evaluador-1");
        _handler = new ListarEvaluacionesQueryHandler(_repositorioMock.Object, _contextoMock.Object);
    }

    private static List<Evaluacion> CrearEvaluaciones(int cantidad)
    {
        var lista = new List<Evaluacion>();
        for (var i = 1; i <= cantidad; i++)
        {
            lista.Add(Evaluacion.Crear($"Evaluación {i}", $"Descripción {i}", "evaluador-1"));
        }
        return lista;
    }

    [Fact(DisplayName = "Handle debe paginar resultados correctamente")]
    public async Task Handle_Paginacion_DebeRetornarPaginaCorrecta()
    {
        var evaluaciones = CrearEvaluaciones(25);
        _repositorioMock.Setup(r => r.ListarAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluaciones);

        var query = new ListarEvaluacionesQuery(Pagina: 2, TamanoPagina: 10);
        var resultado = await _handler.Handle(query, CancellationToken.None);

        resultado.Items.Should().HaveCount(10);
        resultado.TotalItems.Should().Be(25);
        resultado.Pagina.Should().Be(2);
        resultado.TotalPaginas.Should().Be(3);
        resultado.TieneSiguiente.Should().BeTrue();
        resultado.TieneAnterior.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle con búsqueda debe filtrar por nombre")]
    public async Task Handle_Busqueda_DebeFiltraPorNombre()
    {
        var evaluaciones = new List<Evaluacion>
        {
            Evaluacion.Crear("Backend .NET", null, "evaluador-1"),
            Evaluacion.Crear("Frontend Angular", null, "evaluador-1"),
            Evaluacion.Crear("Backend Java", null, "evaluador-1"),
        };
        _repositorioMock.Setup(r => r.ListarAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluaciones);

        var query = new ListarEvaluacionesQuery(Busqueda: "backend");
        var resultado = await _handler.Handle(query, CancellationToken.None);

        resultado.Items.Should().HaveCount(2);
        resultado.TotalItems.Should().Be(2);
        resultado.Items.Should().AllSatisfy(e => e.Nombre.Should().Contain("Backend"));
    }

    [Fact(DisplayName = "Handle última página debe contener ítems restantes")]
    public async Task Handle_UltimaPagina_DebeContenerItemsRestantes()
    {
        var evaluaciones = CrearEvaluaciones(15);
        _repositorioMock.Setup(r => r.ListarAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evaluaciones);

        var query = new ListarEvaluacionesQuery(Pagina: 2, TamanoPagina: 10);
        var resultado = await _handler.Handle(query, CancellationToken.None);

        resultado.Items.Should().HaveCount(5);
        resultado.TieneSiguiente.Should().BeFalse();
        resultado.TieneAnterior.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle sin resultados debe retornar lista vacía paginada")]
    public async Task Handle_SinResultados_DebeRetornarVacio()
    {
        _repositorioMock.Setup(r => r.ListarAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Evaluacion>());

        var query = new ListarEvaluacionesQuery();
        var resultado = await _handler.Handle(query, CancellationToken.None);

        resultado.Items.Should().BeEmpty();
        resultado.TotalItems.Should().Be(0);
        resultado.TotalPaginas.Should().Be(0);
    }
}
