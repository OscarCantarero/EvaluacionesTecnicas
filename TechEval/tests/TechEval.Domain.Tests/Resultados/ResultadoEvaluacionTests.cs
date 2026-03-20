using FluentAssertions;
using TechEval.Domain.Resultados;

namespace TechEval.Domain.Tests.Resultados;

public class ResultadoEvaluacionTests
{
    private static ResultadoEvaluacion CrearResultado(
        Guid? sesionId = null, Guid? evaluacionId = null)
    {
        return ResultadoEvaluacion.Crear(
            sesionId: sesionId ?? Guid.NewGuid(),
            evaluacionId: evaluacionId ?? Guid.NewGuid(),
            candidatoId: "candidato-1",
            nombreCandidato: "Ana Test",
            tituloEvaluacion: "Evaluación .NET",
            violacionesPestana: 0,
            tiempoTotalSegundos: 600);
    }

    [Fact(DisplayName = "Crear resultado debe establecer estado PendienteRevision")]
    public void Crear_DatosValidos_DebeEstablecerEstadoPendienteRevision()
    {
        var resultado = CrearResultado();

        resultado.EstadoRevision.Should().Be(EstadoRevision.PendienteRevision);
        resultado.NombreCandidato.Should().Be("Ana Test");
        resultado.TituloEvaluacion.Should().Be("Evaluación .NET");
        resultado.PuntuacionTotal.Should().Be(0);
    }

    [Fact(DisplayName = "AgregarPuntuacion duplicada debe lanzar excepción")]
    public void AgregarPuntuacion_Duplicada_DebeLanzarInvalidOperationException()
    {
        var resultado = CrearResultado();
        var preguntaId = Guid.NewGuid();
        var p1 = PuntuacionPregunta.Crear(preguntaId, 1, "TextoLibre", "Respuesta");
        resultado.AgregarPuntuacion(p1);

        var p2 = PuntuacionPregunta.Crear(preguntaId, 1, "TextoLibre", "Otra respuesta");
        var act = () => resultado.AgregarPuntuacion(p2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ya tiene puntuación*");
    }

    [Fact(DisplayName = "CalcularPuntuacionTotal sin puntuaciones debe retornar cero")]
    public void CalcularPuntuacionTotal_SinPuntuaciones_DebeSerCero()
    {
        var resultado = CrearResultado();

        resultado.CalcularPuntuacionTotal();

        resultado.PuntuacionTotal.Should().Be(0);
        resultado.PorcentajeObtenido.Should().Be(0);
    }

    [Fact(DisplayName = "CalcularPuntuacionTotal con puntuaciones debe sumar correctamente")]
    public void CalcularPuntuacionTotal_ConPuntuaciones_DebeCalcularCorrectamente()
    {
        var resultado = CrearResultado();
        var p1 = PuntuacionPregunta.Crear(Guid.NewGuid(), 1, "TextoLibre", "Resp 1");
        p1.AsignarPuntuacionManual(30);
        var p2 = PuntuacionPregunta.Crear(Guid.NewGuid(), 2, "TextoLibre", "Resp 2");
        p2.AsignarPuntuacionManual(45);
        resultado.AgregarPuntuacion(p1);
        resultado.AgregarPuntuacion(p2);

        resultado.CalcularPuntuacionTotal(100m);

        resultado.PuntuacionTotal.Should().Be(75);
        resultado.PorcentajeObtenido.Should().Be(75);
    }

    [Fact(DisplayName = "IniciarRevision debe cambiar estado a EnRevision")]
    public void IniciarRevision_DebeEstablecerEstadoEnRevision()
    {
        var resultado = CrearResultado();

        resultado.IniciarRevision("evaluador-1");

        resultado.EstadoRevision.Should().Be(EstadoRevision.EnRevision);
        resultado.EvaluadorRevision.Should().Be("evaluador-1");
        resultado.FechaInicioRevision.Should().NotBeNull();
    }

    [Fact(DisplayName = "CompletarRevision debe cambiar estado a Completada")]
    public void CompletarRevision_DebeEstablecerEstadoCompletada()
    {
        var resultado = CrearResultado();
        resultado.IniciarRevision("evaluador-1");

        resultado.CompletarRevision();

        resultado.EstadoRevision.Should().Be(EstadoRevision.Completada);
        resultado.FechaFinRevision.Should().NotBeNull();
    }

    [Theory(DisplayName = "ObtenerEstadoGeneral debe clasificar correctamente por rango")]
    [InlineData(95, "Aprobado")]
    [InlineData(85, "Aprobado")]
    [InlineData(75, "Aprobado")]
    [InlineData(70, "Aprobado")]
    [InlineData(65, "No aprobado")]
    [InlineData(40, "No aprobado")]
    public void ObtenerEstadoGeneral_VariosRangos_DebeClasificarCorrectamente(
        decimal puntaje, string estadoEsperado)
    {
        var resultado = CrearResultado();
        var p = PuntuacionPregunta.Crear(Guid.NewGuid(), 1, "TextoLibre", "Resp");
        p.AsignarPuntuacionManual(puntaje);
        resultado.AgregarPuntuacion(p);
        resultado.CalcularPuntuacionTotal(100m);

        resultado.ObtenerEstadoGeneral().Should().Be(estadoEsperado);
    }

    [Fact(DisplayName = "ObtenerPuntuacion debe encontrar por PreguntaSesionId")]
    public void ObtenerPuntuacion_IdExistente_DebeRetornarPuntuacion()
    {
        var resultado = CrearResultado();
        var preguntaId = Guid.NewGuid();
        var p = PuntuacionPregunta.Crear(preguntaId, 1, "TextoLibre", "Resp");
        resultado.AgregarPuntuacion(p);

        var encontrada = resultado.ObtenerPuntuacion(preguntaId);

        encontrada.Should().NotBeNull();
        encontrada!.PreguntaSesionId.Should().Be(preguntaId);
    }

    [Fact(DisplayName = "ObtenerPuntuacion con ID inexistente debe retornar null")]
    public void ObtenerPuntuacion_IdInexistente_DebeRetornarNull()
    {
        var resultado = CrearResultado();

        var encontrada = resultado.ObtenerPuntuacion(Guid.NewGuid());

        encontrada.Should().BeNull();
    }

    // --- Tests Transcripciones ---

    [Fact(DisplayName = "TranscripcionEvaluacion.Crear sin contenido ni url debe lanzar excepción")]
    public void TranscripcionEvaluacion_Crear_SinContenidoNiUrl_DebeLanzarExcepcion()
    {
        var act = () => TranscripcionEvaluacion.Crear(TipoTranscripcion.Entrevista);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*al menos*");
    }

    [Fact(DisplayName = "TranscripcionEvaluacion.Crear debe inicializar correctamente")]
    public void TranscripcionEvaluacion_Crear_DebeInicializarCorrectamente()
    {
        var t = TranscripcionEvaluacion.Crear(TipoTranscripcion.Entrevista, "Contenido de prueba");

        t.Id.Should().NotBe(Guid.Empty);
        t.Tipo.Should().Be(TipoTranscripcion.Entrevista);
        t.Contenido.Should().Be("Contenido de prueba");
        t.Estado.Should().Be(EstadoTranscripcion.Subida);
        t.PuntajeIA.Should().BeNull();
    }

    [Fact(DisplayName = "TranscripcionEvaluacion.RegistrarEvaluacionIA debe establecer puntaje")]
    public void TranscripcionEvaluacion_RegistrarEvaluacionIA_DebeEstablecerPuntaje()
    {
        var t = TranscripcionEvaluacion.Crear(TipoTranscripcion.Sesion, "Texto sesión");

        t.RegistrarEvaluacionIA(85.5m, "Buena comunicación");

        t.PuntajeIA.Should().Be(85.5m);
        t.JustificacionIA.Should().Be("Buena comunicación");
        t.Estado.Should().Be(EstadoTranscripcion.EvaluadaPorIA);
    }

    [Fact(DisplayName = "TranscripcionEvaluacion.RegistrarEvaluacionIA puntaje fuera de rango debe lanzar excepción")]
    public void TranscripcionEvaluacion_RegistrarEvaluacionIA_PuntajeFueraDeRango_DebeLanzarExcepcion()
    {
        var t = TranscripcionEvaluacion.Crear(TipoTranscripcion.Sesion, "Texto");

        var act1 = () => t.RegistrarEvaluacionIA(-1m, "");
        var act2 = () => t.RegistrarEvaluacionIA(101m, "");

        act1.Should().Throw<ArgumentOutOfRangeException>();
        act2.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact(DisplayName = "ResultadoEvaluacion.AgregarTranscripcion debe agregar correctamente")]
    public void ResultadoEvaluacion_AgregarTranscripcion_DebeAgregarCorrectamente()
    {
        var resultado = CrearResultado();
        var t = TranscripcionEvaluacion.Crear(TipoTranscripcion.Entrevista, "Entrevista técnica");

        resultado.AgregarTranscripcion(t);

        resultado.Transcripciones.Should().HaveCount(1);
        resultado.Transcripciones[0].Tipo.Should().Be(TipoTranscripcion.Entrevista);
    }

    [Fact(DisplayName = "CalcularPuntuacionTotal con transcripciones debe promediar componentes")]
    public void ResultadoEvaluacion_CalcularPuntuacionTotal_ConTranscripciones_DebePromediar()
    {
        var resultado = CrearResultado();
        var p = PuntuacionPregunta.Crear(Guid.NewGuid(), 1, "TextoLibre", "Resp");
        p.AsignarPuntuacionManual(60); // 60% of 100
        resultado.AgregarPuntuacion(p);

        var t = TranscripcionEvaluacion.Crear(TipoTranscripcion.Entrevista, "Transcripción");
        t.RegistrarEvaluacionIA(90m, "Excelente");
        resultado.AgregarTranscripcion(t);

        resultado.CalcularPuntuacionTotal(100m);

        // Promedio de 60% (respuestas) y 90% (transcripción IA) = 75%
        resultado.PorcentajeObtenido.Should().Be(75m);
    }

    [Fact(DisplayName = "ObtenerEstadoGeneral con porcentaje mayor o igual a 70 debe retornar Aprobado")]
    public void ObtenerEstadoGeneral_ConPorcentajeMayor70_DebeRetornarAprobado()
    {
        var resultado = CrearResultado();
        var p = PuntuacionPregunta.Crear(Guid.NewGuid(), 1, "TextoLibre", "Resp");
        p.AsignarPuntuacionManual(80);
        resultado.AgregarPuntuacion(p);
        resultado.CalcularPuntuacionTotal(100m);

        resultado.ObtenerEstadoGeneral().Should().Be("Aprobado");
    }

    [Fact(DisplayName = "ObtenerEstadoGeneral con porcentaje menor a 70 debe retornar No aprobado")]
    public void ObtenerEstadoGeneral_ConPorcentajeMenor70_DebeRetornarNoAprobado()
    {
        var resultado = CrearResultado();
        var p = PuntuacionPregunta.Crear(Guid.NewGuid(), 1, "TextoLibre", "Resp");
        p.AsignarPuntuacionManual(60);
        resultado.AgregarPuntuacion(p);
        resultado.CalcularPuntuacionTotal(100m);

        resultado.ObtenerEstadoGeneral().Should().Be("No aprobado");
    }
}

public class PuntuacionPreguntaTests
{
    [Fact(DisplayName = "Crear puntuación debe inicializar correctamente")]
    public void Crear_DatosValidos_DebeInicializarCorrectamente()
    {
        var preguntaId = Guid.NewGuid();
        var p = PuntuacionPregunta.Crear(preguntaId, 1, "SeleccionUnica", "Opción A",
            puntuacionAutomatica: 10, fueExpirada: false, tiempoEmpleadoSegundos: 30);

        p.PreguntaSesionId.Should().Be(preguntaId);
        p.NumeroPregunta.Should().Be(1);
        p.PuntuacionAutomatica.Should().Be(10);
        p.TiempoEmpleadoSegundos.Should().Be(30);
    }

    [Fact(DisplayName = "AsignarPuntuacionManual debe establecer valor y observaciones")]
    public void AsignarPuntuacionManual_DebeEstablecerValor()
    {
        var p = PuntuacionPregunta.Crear(Guid.NewGuid(), 1, "TextoLibre", "Resp");

        p.AsignarPuntuacionManual(8.5m, "Buen razonamiento");

        p.PuntuacionManual.Should().Be(8.5m);
        p.Observaciones.Should().Be("Buen razonamiento");
    }

    [Fact(DisplayName = "RegistrarSugerenciaIA debe establecer puntaje y justificación")]
    public void RegistrarSugerenciaIA_DebeEstablecerValores()
    {
        var p = PuntuacionPregunta.Crear(Guid.NewGuid(), 1, "TextoLibre", "Resp");

        p.RegistrarSugerenciaIA(7.0m, "Respuesta parcialmente correcta");

        p.PuntuacionIASugerida.Should().Be(7.0m);
        p.JustificacionIA.Should().Be("Respuesta parcialmente correcta");
    }

    [Fact(DisplayName = "ObtenerPuntuacionFinal debe priorizar Manual sobre Automática sobre IA")]
    public void ObtenerPuntuacionFinal_Prioridad_Manual_Automatica_IA()
    {
        var p = PuntuacionPregunta.Crear(Guid.NewGuid(), 1, "TextoLibre", "Resp",
            puntuacionAutomatica: 5);
        p.RegistrarSugerenciaIA(7, "IA sugiere 7");
        p.AsignarPuntuacionManual(9);

        p.ObtenerPuntuacionFinal().Should().Be(9);
    }

    [Fact(DisplayName = "ObtenerPuntuacionFinal sin puntajes debe retornar cero")]
    public void ObtenerPuntuacionFinal_SinPuntuaciones_DebeRetornarCero()
    {
        var p = PuntuacionPregunta.Crear(Guid.NewGuid(), 1, "TextoLibre", "Resp");

        p.ObtenerPuntuacionFinal().Should().Be(0);
    }

    [Fact(DisplayName = "ObtenerPuntuacionFinal con solo IA debe retornar sugerencia")]
    public void ObtenerPuntuacionFinal_SoloIA_DebeRetornarSugerencia()
    {
        var p = PuntuacionPregunta.Crear(Guid.NewGuid(), 1, "TextoLibre", "Resp");
        p.RegistrarSugerenciaIA(6.5m, "Justificación");

        p.ObtenerPuntuacionFinal().Should().Be(6.5m);
    }
}
