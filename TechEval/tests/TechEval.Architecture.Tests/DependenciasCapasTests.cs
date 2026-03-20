using FluentAssertions;
using NetArchTest.Rules;
using TechEval.API.Controllers;

namespace TechEval.Architecture.Tests;

/// <summary>
/// Verifica que las dependencias entre capas respetan la arquitectura limpia:
/// Domain → sin dependencias externas
/// Application → solo depende de Domain
/// Infrastructure → puede depender de Application y Domain
/// API → puede depender de todas las capas
/// </summary>
public class DependenciasCapasTests
{

    [Fact(DisplayName = "Domain no debe depender de Application")]
    public void DomainLayer_NoDependeDe_ApplicationLayer()
    {
        var resultado = Types.InAssembly(typeof(Domain.Evaluaciones.Evaluacion).Assembly)
            .ShouldNot()
            .HaveDependencyOn("TechEval.Application")
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue(
            because: "La capa Domain no debe conocer Application. Tipos con dependencia no permitida: " +
                     string.Join(", ", resultado.FailingTypeNames ?? []));
    }

    [Fact(DisplayName = "Domain no debe depender de Infrastructure")]
    public void DomainLayer_NoDependeDe_InfrastructureLayer()
    {
        var resultado = Types.InAssembly(typeof(Domain.Evaluaciones.Evaluacion).Assembly)
            .ShouldNot()
            .HaveDependencyOn("TechEval.Infrastructure")
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue(
            because: "La capa Domain no debe conocer Infrastructure. Tipos con dependencia no permitida: " +
                     string.Join(", ", resultado.FailingTypeNames ?? []));
    }

    [Fact(DisplayName = "Application no debe depender de Infrastructure")]
    public void ApplicationLayer_NoDependeDe_InfrastructureLayer()
    {
        var resultado = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn("TechEval.Infrastructure")
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue(
            because: "La capa Application no debe conocer Infrastructure. Tipos con dependencia no permitida: " +
                     string.Join(", ", resultado.FailingTypeNames ?? []));
    }

    [Fact(DisplayName = "Application no debe depender de la API")]
    public void ApplicationLayer_NoDependeDe_ApiLayer()
    {
        var resultado = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn("TechEval.API")
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue(
            because: "La capa Application no debe conocer la API. Tipos con dependencia no permitida: " +
                     string.Join(", ", resultado.FailingTypeNames ?? []));
    }

    [Fact(DisplayName = "Domain no debe depender de la API")]
    public void DomainLayer_NoDependeDe_ApiLayer()
    {
        var resultado = Types.InAssembly(typeof(Domain.Evaluaciones.Evaluacion).Assembly)
            .ShouldNot()
            .HaveDependencyOn("TechEval.API")
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue(
            because: "La capa Domain no debe conocer la API. Tipos con dependencia no permitida: " +
                     string.Join(", ", resultado.FailingTypeNames ?? []));
    }
}

public class NombreConvencionesTests
{
    [Fact(DisplayName = "Los handlers de Application deben terminar en CommandHandler o QueryHandler")]
    public void Handlers_DebenTenerNombreConvencional()
    {
        var resultado = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue(
            because: "Todos los handlers deben terminar en 'Handler'. Tipos incumplidos: " +
                     string.Join(", ", resultado.FailingTypeNames ?? []));
    }

    [Fact(DisplayName = "Los controladores de la API deben residir en el namespace Controllers")]
    public void Controllers_DebenResidirEnNamespaceControllers()
    {
        var resultado = Types.InAssembly(typeof(AuthController).Assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .ResideInNamespace("TechEval.API.Controllers")
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue(
            because: "Los controladores deben estar en TechEval.API.Controllers. Tipos incumplidos: " +
                     string.Join(", ", resultado.FailingTypeNames ?? []));
    }
}
