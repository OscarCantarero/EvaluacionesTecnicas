using FluentAssertions;
using TechEval.Domain.Common.Errors;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Events;

namespace TechEval.Domain.Tests.Evaluaciones;

public class EvaluacionTests
{
    [Fact(DisplayName = "Crear evaluación debe establecer estado Borrador")]
    public void Crear_DatosValidos_DebeEstablecerEstadoBorrador()
    {
        var evaluacion = Evaluacion.Crear("Test .NET", "Descripción", "usuario-1");

        evaluacion.Estado.Should().Be(EstadoEvaluacion.Borrador);
        evaluacion.Nombre.Should().Be("Test .NET");
        evaluacion.CreadoPor.Should().Be("usuario-1");
    }

    [Fact(DisplayName = "Activar orden aleatorio y progresivo al mismo tiempo debe lanzar excepción de dominio")]
    public void ActivarOrdenAleatorio_ConOrdenPorDificultadActivo_DebeLanzarDomainException()
    {
        var evaluacion = Evaluacion.Crear("Test", null, "usuario-1");
        evaluacion.ActivarOrdenPorDificultad();

        var accion = () => evaluacion.ActivarOrdenAleatorio();

        accion.Should().Throw<DomainException>()
            .WithMessage(ErroresEvaluacion.OrdenConflicto.Descripcion);
    }

    [Fact(DisplayName = "Activar orden progresivo con orden aleatorio activo debe lanzar excepción de dominio")]
    public void ActivarOrdenPorDificultad_ConOrdenAleatorioActivo_DebeLanzarDomainException()
    {
        var evaluacion = Evaluacion.Crear("Test", null, "usuario-1");
        evaluacion.ActivarOrdenAleatorio();

        var accion = () => evaluacion.ActivarOrdenPorDificultad();

        accion.Should().Throw<DomainException>()
            .WithMessage(ErroresEvaluacion.OrdenConflicto.Descripcion);
    }

    [Fact(DisplayName = "Agregar pregunta de texto libre debe tener lista de opciones vacía")]
    public void AgregarPregunta_TextoLibre_DebeCrearPreguntaSinOpciones()
    {
        var evaluacion = Evaluacion.Crear("Test", null, "usuario-1");

        var pregunta = evaluacion.AgregarPregunta(
            "¿Cómo funciona DI?", TipoPregunta.TextoLibre, NivelDificultad.Medio,
            null, false, true);

        pregunta.Opciones.Should().BeEmpty();
        pregunta.TipoPregunta.Should().Be(TipoPregunta.TextoLibre);
    }

    [Fact(DisplayName = "Agregar opción a pregunta de texto libre debe lanzar excepción de dominio")]
    public void AgregarOpcion_PreguntaTextoLibre_DebeLanzarDomainException()
    {
        var evaluacion = Evaluacion.Crear("Test", null, "usuario-1");
        var pregunta = evaluacion.AgregarPregunta(
            "¿Cómo funciona DI?", TipoPregunta.TextoLibre, NivelDificultad.Medio,
            null, false, false);

        var accion = () => evaluacion.AgregarOpcion(pregunta.Id, "Respuesta", 10, false);

        accion.Should().Throw<DomainException>()
            .WithMessage(ErroresEvaluacion.OpcionEnPreguntaLibre.Descripcion);
    }

    [Fact(DisplayName = "Limite de tiempo negativo en pregunta debe lanzar excepción de dominio")]
    public void AgregarPregunta_LimiteTiempoNegativo_DebeLanzarDomainException()
    {
        var evaluacion = Evaluacion.Crear("Test", null, "usuario-1");

        var accion = () => evaluacion.AgregarPregunta(
            "Pregunta", TipoPregunta.SeleccionUnica, NivelDificultad.Facil,
            -5, false, false);

        accion.Should().Throw<DomainException>()
            .WithMessage(ErroresEvaluacion.LimiteTiempoInvalido.Descripcion);
    }

    [Fact(DisplayName = "Opción con puntuación y revisión manual debe lanzar excepción de dominio")]
    public void AgregarOpcion_PuntuacionYRevisionManual_DebeLanzarDomainException()
    {
        var evaluacion = Evaluacion.Crear("Test", null, "usuario-1");
        var pregunta = evaluacion.AgregarPregunta(
            "¿Cuál es la respuesta correcta?", TipoPregunta.SeleccionUnica, NivelDificultad.Facil,
            null, false, false);

        var accion = () => evaluacion.AgregarOpcion(pregunta.Id, "Opción A", 10, true);

        accion.Should().Throw<DomainException>()
            .WithMessage(ErroresEvaluacion.PuntuacionConRevisionManual.Descripcion);
    }

    [Fact(DisplayName = "Eliminar pregunta inexistente debe lanzar excepción de dominio")]
    public void EliminarPregunta_IdInexistente_DebeLanzarDomainException()
    {
        var evaluacion = Evaluacion.Crear("Test", null, "usuario-1");

        var accion = () => evaluacion.EliminarPregunta(Guid.NewGuid());

        accion.Should().Throw<DomainException>()
            .WithMessage(ErroresEvaluacion.PreguntaNoEncontrada.Descripcion);
    }

    [Fact(DisplayName = "Crear evaluación debe registrar evento EvaluacionCreadaEvent")]
    public void Crear_DatosValidos_DebeRegistrarDomainEvent()
    {
        var evaluacion = Evaluacion.Crear("Test", null, "usuario-1");

        evaluacion.DominioEventos.Should().HaveCount(1);
        evaluacion.DominioEventos.First().Should().BeOfType<EvaluacionCreadaEvent>();
    }

    [Fact(DisplayName = "Desactivar orden permite reactivar cualquier tipo")]
    public void DesactivarOrden_LuegoActivarAleatorio_NoDebeLanzarExcepcion()
    {
        var evaluacion = Evaluacion.Crear("Test", null, "usuario-1");
        evaluacion.ActivarOrdenPorDificultad();
        evaluacion.DesactivarOrden();

        var accion = () => evaluacion.ActivarOrdenAleatorio();

        accion.Should().NotThrow();
        evaluacion.OrdenAleatorio.Should().BeTrue();
        evaluacion.OrdenPorDificultad.Should().BeFalse();
    }
}
