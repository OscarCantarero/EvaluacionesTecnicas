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
    [InlineData(95, "Excelente")]
    [InlineData(85, "Muy Bueno")]
    [InlineData(75, "Bueno")]
    [InlineData(65, "Aceptable")]
    [InlineData(40, "Insuficiente")]
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
