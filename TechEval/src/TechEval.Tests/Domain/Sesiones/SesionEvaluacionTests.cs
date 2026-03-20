using FluentAssertions;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Sesiones;
using TechEval.Domain.Usuarios;
using Xunit;

namespace TechEval.Tests.Domain.Sesiones;

/// <summary>
/// Tests del agregado SesionEvaluacion.
/// 
/// Escenarios:
/// - Crear sesión con preguntas
/// - Iniciar sesión
/// - Registrar respuesta y avanzar
/// - Registrar violación de pestaña
/// - Completar sesión
/// - Verificar estado y progreso
/// </summary>
public sealed class SesionEvaluacionTests
{
    private readonly Guid _evaluacionId = Guid.NewGuid();
    private readonly Guid _candidatoId = Guid.NewGuid();
    private readonly Usuario _evaluador;

    public SesionEvaluacionTests()
    {
        _evaluador = Usuario.Crear(
            nombre: "Juan Evaluador",
            email: "evaluador@test.com",
            numeroEmpleado: "E001"
        );
    }

    [Fact]
    public void Crear_DebeGenerarCodigoAccesoUnico()
    {
        // Arrange
        var preguntas = new[] { GenerarPregunta(), GenerarPregunta(), GenerarPregunta() };

        // Act
        var sesion = SesionEvaluacion.Crear(
            evaluacionId: _evaluacionId,
            candidatoId: _candidatoId,
            preguntas: preguntas,
            tiempoMaximoMinutos: 30,
            modoSeleccionPreguntas: "random"
        );

        // Assert
        sesion.CodigoAcceso.Should().NotBeNullOrEmpty();
        sesion.CodigoAcceso.Length.Should().Be(8);
        sesion.Should().HaveCount(3); // 3 preguntas
    }

    [Fact]
    public void Crear_ConModoOrdenado_DebeManenerOrdenOriginal()
    {
        // Arrange
        var preguntas = new[] { GenerarPregunta(1), GenerarPregunta(2), GenerarPregunta(3) };

        // Act
        var sesion = SesionEvaluacion.Crear(
            evaluacionId: _evaluacionId,
            candidatoId: _candidatoId,
            preguntas: preguntas,
            tiempoMaximoMinutos: 30,
            modoSeleccionPreguntas: "ordenado"
        );

        // Assert
        sesion.Should().HaveCount(3);
        sesion.ObtenerPreguntaActual()?.Numero.Should().Be(1);
    }

    [Fact]
    public void Iniciar_DebePublicarEvento()
    {
        // Arrange
        var sesion = GenerarSesion();

        // Act
        sesion.Iniciar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.EnProgreso);
        sesion.FechaInicio.Should().NotBeNull();
        sesion.DomainEvents.Should().ContainSingle(e => e is SesionIniciadaEvent);
    }

    [Fact]
    public void RegistrarRespuesta_DebeAvanzarSiguientePregunta()
    {
        // Arrange
        var sesion = GenerarSesion();
        sesion.Iniciar();
        var preguntaActual = sesion.ObtenerPreguntaActual();

        // Act
        sesion.RegistrarRespuesta(
            preguntaId: preguntaActual!.Id,
            texto: "Respuesta de prueba",
            tiempoEmpleadoSegundos: 30
        );

        // Assert
        var (respondidas, total) = sesion.ObtenerProgreso();
        respondidas.Should().Be(1);
        total.Should().Be(3);
        sesion.DomainEvents.Should().Contain(e => e is RespuestaRegistradaEvent);
    }

    [Fact]
    public void RegistrarRespuesta_EnUltimaPregunta_DebeCompletarSesion()
    {
        // Arrange
        var sesion = GenerarSesion();
        sesion.Iniciar();

        // Responder todas las preguntas
        foreach (var pregunta in sesion)
        {
            sesion.RegistrarRespuesta(
                preguntaId: pregunta.Id,
                texto: "Respuesta",
                tiempoEmpleadoSegundos: 10
            );
        }

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Completada);
        sesion.FechaFin.Should().NotBeNull();
        sesion.DomainEvents.Should().Contain(e => e is SesionCompletadaEvent);
    }

    [Fact]
    public void RegistrarViolacionPestana_DebeIncrementarContador()
    {
        // Arrange
        var sesion = GenerarSesion();
        sesion.Iniciar();

        // Act
        sesion.RegistrarViolacionPestana();
        sesion.RegistrarViolacionPestana();

        // Assert
        sesion.ContadorViolacionesPestana.Should().Be(2);
        sesion.DomainEvents.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void ObtenerProgreso_DebeRetornarConteoPreguntasRespondidas()
    {
        // Arrange
        var sesion = GenerarSesion();
        sesion.Iniciar();

        // Act
        sesion.RegistrarRespuesta(
            preguntaId: sesion.ObtenerPreguntaActual()!.Id,
            texto: "Respuesta 1",
            tiempoEmpleadoSegundos: 20
        );

        var (respondidas, total) = sesion.ObtenerProgreso();

        // Assert
        respondidas.Should().Be(1);
        total.Should().Be(3);
    }

    [Fact]
    public void ObtenerPreguntaActual_DebeRetornarNullCuandoTodoFueRespondido()
    {
        // Arrange
        var sesion = GenerarSesion();
        sesion.Iniciar();

        // Responder todas
        foreach (var pregunta in sesion)
        {
            sesion.RegistrarRespuesta(
                preguntaId: pregunta.Id,
                texto: "Respuesta",
                tiempoEmpleadoSegundos: 10
            );
        }

        // Act
        var actual = sesion.ObtenerPreguntaActual();

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public void AgregarAdjunto_DebeAsociarConRespuesta()
    {
        // Arrange
        var sesion = GenerarSesion();
        sesion.Iniciar();
        var pregunta = sesion.ObtenerPreguntaActual();

        sesion.RegistrarRespuesta(
            preguntaId: pregunta!.Id,
            texto: "Respuesta",
            tiempoEmpleadoSegundos: 10
        );

        // Act
        var urlAdjunto = "https://storage.example.com/adjuntos/file123.pdf";
        sesion.AgregarAdjuntoARespuesta(
            preguntaId: pregunta.Id,
            urlAdjunto: urlAdjunto
        );

        // Assert
        var respuesta = pregunta.ObtenerRespuesta();
        respuesta?.UrlAdjunto.Should().Be(urlAdjunto);
    }

    // Métodos helper
    private SesionEvaluacion GenerarSesion()
    {
        var preguntas = new[] { GenerarPregunta(), GenerarPregunta(), GenerarPregunta() };
        return SesionEvaluacion.Crear(
            evaluacionId: _evaluacionId,
            candidatoId: _candidatoId,
            preguntas: preguntas,
            tiempoMaximoMinutos: 30,
            modoSeleccionPreguntas: "random"
        );
    }

    private Pregunta GenerarPregunta(int numero = 1)
    {
        return Pregunta.Crear(
            evaluacionId: _evaluacionId,
            numero: numero,
            tipo: "multiple",
            texto: $"Pregunta de prueba {numero}",
            dificultad: "media",
            tiempoMaximoSegundos: 60,
            opciones: ["Opción A", "Opción B", "Opción C"],
            respuestaCorrecta: "Opción A"
        );
    }
}
