using FluentAssertions;
using TechEval.Domain.Evaluaciones;

namespace TechEval.Domain.Tests.Evaluaciones;

public class DistribucionDificultadTests
{
    [Fact(DisplayName = "Crear distribución válida debe retornar el total correcto")]
    public void Crear_Valida_DebeCalcularTotal()
    {
        var dist = DistribucionDificultad.Crear(3, 4, 3);
        dist.Total.Should().Be(10);
        dist.Facil.Should().Be(3);
        dist.Medio.Should().Be(4);
        dist.Dificil.Should().Be(3);
    }

    [Fact(DisplayName = "Crear distribución con todos en cero debe lanzar excepción")]
    public void Crear_TodosCero_DebeLanzarExcepcion()
    {
        var accion = () => DistribucionDificultad.Crear(0, 0, 0);
        accion.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Crear distribución con valor negativo debe lanzar excepción")]
    public void Crear_ValorNegativo_DebeLanzarExcepcion()
    {
        var accion = () => DistribucionDificultad.Crear(-1, 4, 3);
        accion.Should().Throw<ArgumentException>();
    }
}
