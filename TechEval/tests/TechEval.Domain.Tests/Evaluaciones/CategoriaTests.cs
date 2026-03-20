using FluentAssertions;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones;

namespace TechEval.Domain.Tests.Evaluaciones;

public class CategoriaTests
{
    [Fact(DisplayName = "Crear categoría debe inicializar correctamente")]
    public void Crear_DatosValidos_DebeInicializarCorrectamente()
    {
        var c = Categoria.Crear("Backend .NET", "Preguntas de C# y .NET", "evaluador-1");

        c.Nombre.Should().Be("Backend .NET");
        c.Descripcion.Should().Be("Preguntas de C# y .NET");
        c.CreadoPor.Should().Be("evaluador-1");
        c.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Crear categoría con nombre vacío debe lanzar excepción")]
    public void Crear_NombreVacio_DebeLanzarExcepcion()
    {
        var act = () => Categoria.Crear("   ", null, "evaluador-1");

        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Actualizar categoría debe modificar nombre y descripción")]
    public void Actualizar_DatosValidos_DebeModificarNombreYDescripcion()
    {
        var c = Categoria.Crear("SQL", null, "evaluador-1");

        c.Actualizar("SQL Avanzado", "Consultas complejas");

        c.Nombre.Should().Be("SQL Avanzado");
        c.Descripcion.Should().Be("Consultas complejas");
        c.ActualizadoEn.Should().NotBeNull();
    }

    [Fact(DisplayName = "Pregunta con CategoriaId debe retornar el id asignado")]
    public void Pregunta_AsignarCategoria_DebeEstablecerCategoriaId()
    {
        var ev = Evaluacion.Crear("Evaluación test", null, "evaluador-1");
        var catId = Guid.NewGuid();

        var p = ev.AgregarPregunta(
            "¿Qué es LINQ?",
            TipoPregunta.TextoLibre,
            NivelDificultad.Medio,
            null, false, true, catId);

        p.CategoriaId.Should().Be(catId);
    }

    [Fact(DisplayName = "Pregunta sin categoría debe tener CategoriaId null")]
    public void Pregunta_SinCategoria_DebeTenerCategoriaIdNull()
    {
        var ev = Evaluacion.Crear("Evaluación test", null, "evaluador-1");

        var p = ev.AgregarPregunta(
            "¿Qué es LINQ?",
            TipoPregunta.TextoLibre,
            NivelDificultad.Medio,
            null, false, true);

        p.CategoriaId.Should().BeNull();
    }
}
