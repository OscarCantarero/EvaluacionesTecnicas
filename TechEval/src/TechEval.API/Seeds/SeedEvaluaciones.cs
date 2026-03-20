using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones;
using TechEval.Infrastructure.Identity;
using TechEval.Infrastructure.Persistence;

namespace TechEval.API.Seeds;

/// <summary>
/// Seeder de evaluaciones de desarrollo con preguntas variadas por tema técnico.
/// Idempotente: solo crea datos si no existen evaluaciones seed previas.
/// </summary>
public static class SeedEvaluaciones
{
    private const string EvaluadorEmail = "evaluador@techeval.com";

    public static async Task EjecutarAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<TechEvalDbContext>();
        var userManager = services.GetRequiredService<UserManager<UsuarioApp>>();

        var evaluador = await userManager.FindByEmailAsync(EvaluadorEmail);
        if (evaluador is null) return;

        var yaExisten = await db.Evaluaciones
            .AnyAsync(e => e.CreadoPor == evaluador.Id && e.Nombre.StartsWith("[Seed]"));
        if (yaExisten) return;

        var evaluaciones = CrearEvaluaciones(evaluador.Id);
        foreach (var eval in evaluaciones)
            db.Evaluaciones.Add(eval);

        await db.SaveChangesAsync();
    }

    private static List<Evaluacion> CrearEvaluaciones(string evaluadorId) =>
    [
        CrearEvalBackendDotNet(evaluadorId),
        CrearEvalFrontendAngular(evaluadorId),
        CrearEvalBaseDeDatos(evaluadorId),
        CrearEvalArquitecturaSoftware(evaluadorId),
        CrearEvalDevOpsCloud(evaluadorId),
        CrearEvalSeguridad(evaluadorId),
        CrearEvalPythonDataScience(evaluadorId),
    ];

    // ─── Helpers ──────────────────────────────────────────────────
    private static Pregunta Agregar(Evaluacion e, string texto, TipoPregunta tipo, NivelDificultad nivel, int? tiempo, bool adjunto, bool manual)
        => e.AgregarPregunta(texto, tipo, nivel, tiempo, adjunto, manual);

    private static void Opc(Evaluacion e, Guid pId, string texto, int? pts)
        => e.AgregarOpcion(pId, texto, pts, false);

    // ─────────────────────────────────────────────────────────────
    // 1. Backend .NET
    // ─────────────────────────────────────────────────────────────
    private static Evaluacion CrearEvalBackendDotNet(string uid)
    {
        var e = Evaluacion.Crear("[Seed] Backend .NET 8 — Nivel Intermedio-Avanzado",
            "Evaluación completa de conocimientos en C#, ASP.NET Core, Entity Framework y patrones de diseño backend.", uid);
        e.CambiarEstado(EstadoEvaluacion.Activa);

        // P1 — Texto libre: DI lifecycles
        Agregar(e, "Explique las diferencias entre los ciclos de vida Transient, Scoped y Singleton en la inyección de dependencias de ASP.NET Core. Proporcione un ejemplo de cuándo usaría cada uno.",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 180, false, true);

        // P2 — Selección única: async/await
        var p2 = Agregar(e, "¿Cuál de las siguientes afirmaciones sobre async/await en C# es CORRECTA?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Medio, 60, false, false);
        Opc(e, p2.Id, "async/await crea un nuevo hilo para cada operación asíncrona", null);
        Opc(e, p2.Id, "await libera el hilo actual y lo devuelve al threadpool mientras espera la operación I/O", 3);
        Opc(e, p2.Id, "Las tareas asíncronas siempre se ejecutan en paralelo", null);
        Opc(e, p2.Id, "No se puede usar async/await en métodos que retornan void", null);

        // P3 — Selección múltiple: middleware pipeline
        var p3 = Agregar(e, "Seleccione TODOS los middleware que forman parte del pipeline por defecto de ASP.NET Core:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Facil, 45, false, false);
        Opc(e, p3.Id, "UseRouting", 1);
        Opc(e, p3.Id, "UseAuthentication", 1);
        Opc(e, p3.Id, "UseAuthorization", 1);
        Opc(e, p3.Id, "UseGraphQL", null);
        Opc(e, p3.Id, "UseEndpoints", 1);

        // P4 — Texto libre: Repository + UoW
        Agregar(e, "Describa cómo implementaría el patrón Repository con Unit of Work en una aplicación .NET con Entity Framework Core. ¿Cuáles son las ventajas y desventajas?",
            TipoPregunta.TextoLibre, NivelDificultad.Dificil, 240, true, true);

        // P5 — Selección única: AsNoTracking
        var p5 = Agregar(e, "En Entity Framework Core, ¿cuál es la diferencia principal entre AsNoTracking() y AsTracking()?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Facil, 45, false, false);
        Opc(e, p5.Id, "AsNoTracking() desactiva las migraciones automáticas", null);
        Opc(e, p5.Id, "AsNoTracking() evita que el DbContext rastree cambios en las entidades, mejorando el rendimiento en consultas de solo lectura", 2);
        Opc(e, p5.Id, "AsTracking() es más rápido que AsNoTracking()", null);
        Opc(e, p5.Id, "No hay diferencia funcional entre ambos", null);

        // P6 — Selección múltiple: SOLID
        var p6 = Agregar(e, "¿Cuáles son principios SOLID? Seleccione todos los correctos:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Facil, 30, false, false);
        Opc(e, p6.Id, "Single Responsibility Principle", 1);
        Opc(e, p6.Id, "Open/Closed Principle", 1);
        Opc(e, p6.Id, "Liskov Substitution Principle", 1);
        Opc(e, p6.Id, "Lazy Loading Principle", null);
        Opc(e, p6.Id, "Dependency Inversion Principle", 1);

        // P7 — Texto libre: CQRS + MediatR
        Agregar(e, "Explique qué es CQRS (Command Query Responsibility Segregation) y cómo lo implementaría con MediatR en .NET. ¿En qué escenarios es recomendable y cuándo no?",
            TipoPregunta.TextoLibre, NivelDificultad.Dificil, 300, true, true);

        // P8 — Selección única: Strategy pattern
        var p8 = Agregar(e, "¿Qué patrón de diseño se utiliza cuando se quiere definir una familia de algoritmos intercambiables?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Medio, 45, false, false);
        Opc(e, p8.Id, "Observer", null);
        Opc(e, p8.Id, "Factory Method", null);
        Opc(e, p8.Id, "Strategy", 2);
        Opc(e, p8.Id, "Singleton", null);

        // P9 — Texto libre: concurrencia optimista
        Agregar(e, "¿Cómo manejaría la concurrencia optimista en Entity Framework Core? Escriba un ejemplo de código.",
            TipoPregunta.TextoLibre, NivelDificultad.Dificil, 240, true, true);

        // P10 — Selección múltiple: filtros ASP.NET
        var p10 = Agregar(e, "Seleccione las afirmaciones correctas sobre los filtros (filters) en ASP.NET Core:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Medio, 60, false, false);
        Opc(e, p10.Id, "Los Action Filters se ejecutan antes y después de una acción del controlador", 1);
        Opc(e, p10.Id, "Los Exception Filters solo capturan excepciones de la capa de datos", null);
        Opc(e, p10.Id, "Los Authorization Filters se ejecutan primero en el pipeline", 1);
        Opc(e, p10.Id, "Los Result Filters se ejecutan antes y después del result de la acción", 1);

        return e;
    }

    // ─────────────────────────────────────────────────────────────
    // 2. Frontend Angular
    // ─────────────────────────────────────────────────────────────
    private static Evaluacion CrearEvalFrontendAngular(string uid)
    {
        var e = Evaluacion.Crear("[Seed] Frontend Angular 18+ — Signals y Standalone",
            "Evaluación de conocimientos en Angular moderno: standalone components, signals, reactive forms, RxJS y buenas prácticas.", uid);
        e.CambiarEstado(EstadoEvaluacion.Activa);

        var p1 = Agregar(e, "¿Cuál es la principal ventaja de los Standalone Components en Angular?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Facil, 45, false, false);
        Opc(e, p1.Id, "Permiten usar TypeScript en vez de JavaScript", null);
        Opc(e, p1.Id, "Eliminan la necesidad de NgModules, simplificando la estructura del proyecto", 2);
        Opc(e, p1.Id, "Hacen que la aplicación sea compatible con React", null);
        Opc(e, p1.Id, "Obligan a usar signals en todas partes", null);

        Agregar(e, "Explique la diferencia entre Signals y Observables en Angular. ¿Cuándo usaría cada uno?",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 180, false, true);

        var p3 = Agregar(e, "Seleccione las estrategias de detección de cambios disponibles en Angular:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Medio, 45, false, false);
        Opc(e, p3.Id, "Default (CheckAlways)", 1);
        Opc(e, p3.Id, "OnPush", 1);
        Opc(e, p3.Id, "Manual", null);
        Opc(e, p3.Id, "Lazy", null);

        var p4 = Agregar(e, "¿Qué operador de RxJS usaría para cancelar una petición HTTP anterior cuando llega una nueva (ej: búsqueda autocomplete)?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Medio, 45, false, false);
        Opc(e, p4.Id, "mergeMap", null);
        Opc(e, p4.Id, "concatMap", null);
        Opc(e, p4.Id, "switchMap", 3);
        Opc(e, p4.Id, "exhaustMap", null);

        Agregar(e, "Escriba un ejemplo de cómo implementaría un guard funcional en Angular 18+ que redirija al login si el usuario no está autenticado.",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 180, true, true);

        var p6 = Agregar(e, "Seleccione las afirmaciones correctas sobre Reactive Forms en Angular:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Facil, 45, false, false);
        Opc(e, p6.Id, "Se crean con FormBuilder o FormGroup en el componente TS", 1);
        Opc(e, p6.Id, "Requieren importar FormsModule", null);
        Opc(e, p6.Id, "Son más testables que Template-Driven Forms", 1);
        Opc(e, p6.Id, "Usan la directiva ngModel internamente", null);
        Opc(e, p6.Id, "Permiten validaciones síncronas y asíncronas", 1);

        var p7 = Agregar(e, "¿Cuál es la función de computed() en el sistema de signals de Angular?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Facil, 30, false, false);
        Opc(e, p7.Id, "Crea un signal que calcula su valor a partir de otros signals, actualizándose automáticamente", 2);
        Opc(e, p7.Id, "Ejecuta un efecto secundario cuando cambia un signal", null);
        Opc(e, p7.Id, "Convierte un Observable en un Signal", null);
        Opc(e, p7.Id, "Memoriza funciones puras para optimizar rendimiento", null);

        Agregar(e, "Describa cómo configuraría lazy loading de rutas en Angular standalone. Proporcione un ejemplo del archivo de rutas.",
            TipoPregunta.TextoLibre, NivelDificultad.Dificil, 240, true, true);

        var p9 = Agregar(e, "¿Qué mecanismo usa Angular para prevenir ataques XSS?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Medio, 45, false, false);
        Opc(e, p9.Id, "CSRF tokens automáticos", null);
        Opc(e, p9.Id, "Sanitización automática de valores inseguros en el DOM", 2);
        Opc(e, p9.Id, "Encriptación de templates", null);
        Opc(e, p9.Id, "Content Security Policy integrada", null);

        var p10 = Agregar(e, "Seleccione los interceptores válidos que se pueden configurar con provideHttpClient() en Angular 18+:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Dificil, 60, false, false);
        Opc(e, p10.Id, "withInterceptors([fn]) — interceptores funcionales", 1);
        Opc(e, p10.Id, "withInterceptorsFromDi() — interceptores basados en clase", 1);
        Opc(e, p10.Id, "withXsrfConfiguration() — protección XSRF", 1);
        Opc(e, p10.Id, "withModuleInterceptors() — interceptores desde NgModule", null);

        return e;
    }

    // ─────────────────────────────────────────────────────────────
    // 3. Base de Datos
    // ─────────────────────────────────────────────────────────────
    private static Evaluacion CrearEvalBaseDeDatos(string uid)
    {
        var e = Evaluacion.Crear("[Seed] Base de Datos — SQL y Modelado",
            "Evaluación de conocimientos en SQL, diseño de esquemas, normalización, índices y optimización de consultas.", uid);
        e.CambiarEstado(EstadoEvaluacion.Activa);

        Agregar(e, "Explique las tres primeras formas normales (1NF, 2NF, 3NF) con un ejemplo práctico de una tabla que viola cada una.",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 240, false, true);

        var p2 = Agregar(e, "¿Cuál es la diferencia entre INNER JOIN y LEFT JOIN?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Facil, 30, false, false);
        Opc(e, p2.Id, "INNER JOIN retorna solo filas con coincidencia en ambas tablas; LEFT JOIN retorna todas las filas de la tabla izquierda", 2);
        Opc(e, p2.Id, "LEFT JOIN es más rápido que INNER JOIN", null);
        Opc(e, p2.Id, "No hay diferencia, son sinónimos", null);
        Opc(e, p2.Id, "INNER JOIN solo funciona con claves primarias", null);

        var p3 = Agregar(e, "Seleccione los tipos de índices disponibles en PostgreSQL:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Dificil, 60, false, false);
        Opc(e, p3.Id, "B-tree", 1);
        Opc(e, p3.Id, "Hash", 1);
        Opc(e, p3.Id, "GIN (Generalized Inverted Index)", 1);
        Opc(e, p3.Id, "GiST (Generalized Search Tree)", 1);
        Opc(e, p3.Id, "Binary Tree", null);

        Agregar(e, "Escriba una consulta SQL que obtenga los 5 productos más vendidos del último mes, incluyendo el total de unidades vendidas y el ingreso total.",
            TipoPregunta.TextoLibre, NivelDificultad.Dificil, 300, true, true);

        Agregar(e, "¿Qué es una transacción ACID? Explique cada propiedad.",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 180, false, true);

        var p6 = Agregar(e, "¿Cuál es el propósito de un índice covering (cubriente)?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Dificil, 45, false, false);
        Opc(e, p6.Id, "Prevenir SQL injection", null);
        Opc(e, p6.Id, "Incluir todas las columnas necesarias en el índice, evitando acceder a la tabla base", 3);
        Opc(e, p6.Id, "Cubrir todas las tablas de la base de datos con un solo índice", null);
        Opc(e, p6.Id, "Crear respaldos automáticos del índice", null);

        var p7 = Agregar(e, "Seleccione las afirmaciones correctas sobre las vistas materializadas (materialized views):",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Dificil, 60, false, false);
        Opc(e, p7.Id, "Almacenan el resultado de la consulta en disco", 1);
        Opc(e, p7.Id, "Se actualizan automáticamente cuando cambian los datos fuente", null);
        Opc(e, p7.Id, "Requieren REFRESH MATERIALIZED VIEW para actualizarse", 1);
        Opc(e, p7.Id, "Pueden tener índices propios", 1);

        Agregar(e, "¿Cuál es la diferencia entre DELETE, TRUNCATE y DROP?",
            TipoPregunta.TextoLibre, NivelDificultad.Facil, 120, false, true);

        return e;
    }

    // ─────────────────────────────────────────────────────────────
    // 4. Arquitectura de Software
    // ─────────────────────────────────────────────────────────────
    private static Evaluacion CrearEvalArquitecturaSoftware(string uid)
    {
        var e = Evaluacion.Crear("[Seed] Arquitectura de Software — Clean Architecture y DDD",
            "Evaluación de conocimientos en patrones arquitectónicos, Clean Architecture, Domain-Driven Design y microservicios.", uid);
        e.CambiarEstado(EstadoEvaluacion.Activa);

        Agregar(e, "Describa las capas de Clean Architecture y explique la regla de dependencia. ¿Por qué el dominio no debe depender de la infraestructura?",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 240, true, true);

        var p2 = Agregar(e, "¿Cuál es la diferencia entre un Aggregate Root y una Entity en DDD?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Medio, 60, false, false);
        Opc(e, p2.Id, "No hay diferencia, son sinónimos", null);
        Opc(e, p2.Id, "El Aggregate Root es la entidad principal que controla el acceso a las demás entidades del agregado y asegura invariantes", 3);
        Opc(e, p2.Id, "El Aggregate Root es solo un patrón de persistencia", null);
        Opc(e, p2.Id, "Una Entity no puede contener Value Objects", null);

        var p3 = Agregar(e, "Seleccione los conceptos que pertenecen a Domain-Driven Design:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Facil, 45, false, false);
        Opc(e, p3.Id, "Bounded Context", 1);
        Opc(e, p3.Id, "Ubiquitous Language", 1);
        Opc(e, p3.Id, "Aggregate", 1);
        Opc(e, p3.Id, "Endpoint Routing", null);
        Opc(e, p3.Id, "Domain Events", 1);
        Opc(e, p3.Id, "Middleware Pipeline", null);

        Agregar(e, "Compare monolitos vs microservicios. ¿Cuándo recomendaría cada enfoque? Mencione al menos 3 pros y 3 contras de cada uno.",
            TipoPregunta.TextoLibre, NivelDificultad.Dificil, 300, true, true);

        var p5 = Agregar(e, "¿Qué patrón se usa para desacoplar el envío de comandos de su ejecución, permitiendo encolar, registrar y deshacer operaciones?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Medio, 45, false, false);
        Opc(e, p5.Id, "Observer", null);
        Opc(e, p5.Id, "Command", 2);
        Opc(e, p5.Id, "Decorator", null);
        Opc(e, p5.Id, "Adapter", null);

        Agregar(e, "Explique el patrón Event Sourcing y cuándo lo usaría. ¿Cuáles son sus ventajas sobre CRUD tradicional?",
            TipoPregunta.TextoLibre, NivelDificultad.Dificil, 240, false, true);

        var p7 = Agregar(e, "Seleccione los patrones que ayudan a manejar la comunicación entre microservicios:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Medio, 60, false, false);
        Opc(e, p7.Id, "API Gateway", 1);
        Opc(e, p7.Id, "Saga Pattern", 1);
        Opc(e, p7.Id, "Circuit Breaker", 1);
        Opc(e, p7.Id, "Singleton Pattern", null);
        Opc(e, p7.Id, "Event-Driven Messaging", 1);

        Agregar(e, "¿Qué es el principio de Inversión de Dependencias y cómo se implementa en una aplicación .NET con Clean Architecture?",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 180, false, true);

        var p9 = Agregar(e, "¿Cuál es la diferencia entre un Value Object y una Entity en DDD?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Facil, 45, false, false);
        Opc(e, p9.Id, "Un Value Object tiene identidad propia; una Entity no", null);
        Opc(e, p9.Id, "Un Value Object se define por sus atributos (sin identidad); una Entity tiene identidad única", 2);
        Opc(e, p9.Id, "Son iguales pero con nombres diferentes", null);
        Opc(e, p9.Id, "Las Entities no pueden ser inmutables, los Value Objects sí", null);

        Agregar(e, "Diseñe un diagrama de agregados para un sistema de e-commerce (Orden, Producto, Cliente, Pago). Explique qué sería cada Aggregate Root y por qué.",
            TipoPregunta.TextoLibre, NivelDificultad.Dificil, 360, true, true);

        return e;
    }

    // ─────────────────────────────────────────────────────────────
    // 5. DevOps y Cloud
    // ─────────────────────────────────────────────────────────────
    private static Evaluacion CrearEvalDevOpsCloud(string uid)
    {
        var e = Evaluacion.Crear("[Seed] DevOps y Cloud — CI/CD, Docker y Kubernetes",
            "Evaluación de conocimientos en contenedores, orquestación, CI/CD pipelines y servicios cloud.", uid);
        e.CambiarEstado(EstadoEvaluacion.Activa);

        var p1 = Agregar(e, "¿Cuál es la diferencia entre una imagen Docker y un contenedor Docker?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Facil, 30, false, false);
        Opc(e, p1.Id, "No hay diferencia", null);
        Opc(e, p1.Id, "La imagen es una plantilla de solo lectura; el contenedor es una instancia en ejecución de esa imagen", 2);
        Opc(e, p1.Id, "El contenedor es más ligero que la imagen", null);
        Opc(e, p1.Id, "La imagen se ejecuta y el contenedor se almacena", null);

        Agregar(e, "Escriba un Dockerfile multi-stage para una aplicación .NET 8 que compile y publique la app en una imagen optimizada.",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 240, true, true);

        var p3 = Agregar(e, "Seleccione los conceptos fundamentales de Kubernetes:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Medio, 60, false, false);
        Opc(e, p3.Id, "Pod", 1);
        Opc(e, p3.Id, "Service", 1);
        Opc(e, p3.Id, "Deployment", 1);
        Opc(e, p3.Id, "Ingress", 1);
        Opc(e, p3.Id, "Stored Procedure", null);
        Opc(e, p3.Id, "ConfigMap", 1);

        Agregar(e, "Explique la diferencia entre CI (Integración Continua) y CD (Despliegue/Entrega Continua). Describa un pipeline típico de CI/CD para una aplicación web.",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 240, false, true);

        var p5 = Agregar(e, "¿Qué comando de Docker se usa para construir una imagen a partir de un Dockerfile?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Facil, 20, false, false);
        Opc(e, p5.Id, "docker run", null);
        Opc(e, p5.Id, "docker build", 1);
        Opc(e, p5.Id, "docker create", null);
        Opc(e, p5.Id, "docker compile", null);

        var p6 = Agregar(e, "Seleccione las estrategias de despliegue válidas:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Dificil, 60, false, false);
        Opc(e, p6.Id, "Blue-Green Deployment", 1);
        Opc(e, p6.Id, "Canary Release", 1);
        Opc(e, p6.Id, "Rolling Update", 1);
        Opc(e, p6.Id, "Big Bang Deployment", 1);
        Opc(e, p6.Id, "Parallel Universe Deployment", null);

        Agregar(e, "¿Cómo manejaría los secretos (contraseñas, API keys) en un pipeline de CI/CD y en contenedores de producción?",
            TipoPregunta.TextoLibre, NivelDificultad.Dificil, 180, false, true);

        var p8 = Agregar(e, "¿Cuál es la función de un Ingress Controller en Kubernetes?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Medio, 45, false, false);
        Opc(e, p8.Id, "Ejecutar migraciones de base de datos", null);
        Opc(e, p8.Id, "Gestionar el tráfico HTTP/HTTPS entrante al cluster, con reglas de enrutamiento y TLS", 3);
        Opc(e, p8.Id, "Monitorear el uso de CPU de los pods", null);
        Opc(e, p8.Id, "Escalar automáticamente los pods", null);

        return e;
    }

    // ─────────────────────────────────────────────────────────────
    // 6. Seguridad Informática
    // ─────────────────────────────────────────────────────────────
    private static Evaluacion CrearEvalSeguridad(string uid)
    {
        var e = Evaluacion.Crear("[Seed] Seguridad Informática — OWASP y Autenticación",
            "Evaluación de conocimientos en seguridad web, OWASP Top 10, autenticación, autorización y criptografía básica.", uid);
        e.CambiarEstado(EstadoEvaluacion.Activa);

        var p1 = Agregar(e, "Seleccione las vulnerabilidades que pertenecen al OWASP Top 10:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Medio, 60, false, false);
        Opc(e, p1.Id, "Broken Access Control", 1);
        Opc(e, p1.Id, "SQL Injection", 1);
        Opc(e, p1.Id, "Cross-Site Scripting (XSS)", 1);
        Opc(e, p1.Id, "Buffer Overflow", null);
        Opc(e, p1.Id, "Server-Side Request Forgery (SSRF)", 1);

        Agregar(e, "Explique la diferencia entre autenticación y autorización. ¿Cómo implementaría ambas en una API REST?",
            TipoPregunta.TextoLibre, NivelDificultad.Facil, 180, false, true);

        var p3 = Agregar(e, "¿Cuál es la diferencia entre JWT y OAuth 2.0?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Medio, 60, false, false);
        Opc(e, p3.Id, "Son lo mismo", null);
        Opc(e, p3.Id, "JWT es un formato de token; OAuth 2.0 es un protocolo de autorización que puede usar JWT como formato de token", 3);
        Opc(e, p3.Id, "OAuth 2.0 reemplazó completamente a JWT", null);
        Opc(e, p3.Id, "JWT es más seguro que OAuth 2.0", null);

        Agregar(e, "¿Qué es SQL Injection y cómo se previene? Proporcione un ejemplo de código vulnerable y su versión corregida.",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 240, true, true);

        var p5 = Agregar(e, "Seleccione las mejores prácticas para almacenar contraseñas:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Facil, 45, false, false);
        Opc(e, p5.Id, "Usar hashing con salt (bcrypt, Argon2)", 1);
        Opc(e, p5.Id, "Almacenar en texto plano en la base de datos", null);
        Opc(e, p5.Id, "Usar encriptación reversible (AES)", null);
        Opc(e, p5.Id, "Nunca almacenar la contraseña original", 1);
        Opc(e, p5.Id, "Usar un salt único por usuario", 1);

        Agregar(e, "¿Qué es un ataque CSRF (Cross-Site Request Forgery) y cómo se mitiga en aplicaciones web modernas?",
            TipoPregunta.TextoLibre, NivelDificultad.Dificil, 180, false, true);

        var p7 = Agregar(e, "¿Cuál de las siguientes es la mejor práctica para manejar tokens JWT en el frontend?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Medio, 45, false, false);
        Opc(e, p7.Id, "Almacenar en localStorage sin expiración", null);
        Opc(e, p7.Id, "Almacenar en memoria o httpOnly cookies con refresh token rotation y expiración corta", 3);
        Opc(e, p7.Id, "Enviar el token como parámetro en la URL", null);
        Opc(e, p7.Id, "Almacenar en sessionStorage sin validación de expiración", null);

        Agregar(e, "Explique qué es el principio de mínimo privilegio y cómo lo aplicaría en una API con múltiples roles.",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 180, false, true);

        return e;
    }

    // ─────────────────────────────────────────────────────────────
    // 7. Python y Data Science
    // ─────────────────────────────────────────────────────────────
    private static Evaluacion CrearEvalPythonDataScience(string uid)
    {
        var e = Evaluacion.Crear("[Seed] Python y Data Science — Fundamentos",
            "Evaluación de conocimientos en Python, pandas, numpy, visualización de datos y conceptos básicos de machine learning.", uid);
        // Dejamos esta como Borrador para variedad de estados

        var p1 = Agregar(e, "¿Cuál es la diferencia entre una lista y una tupla en Python?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Facil, 30, false, false);
        Opc(e, p1.Id, "No hay diferencia", null);
        Opc(e, p1.Id, "Las listas son mutables; las tuplas son inmutables", 2);
        Opc(e, p1.Id, "Las tuplas son más lentas que las listas", null);
        Opc(e, p1.Id, "Las listas solo contienen números", null);

        var p2 = Agregar(e, "Seleccione las librerías usadas comúnmente en Data Science con Python:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Facil, 45, false, false);
        Opc(e, p2.Id, "pandas", 1);
        Opc(e, p2.Id, "numpy", 1);
        Opc(e, p2.Id, "matplotlib", 1);
        Opc(e, p2.Id, "scikit-learn", 1);
        Opc(e, p2.Id, "jQuery", null);

        Agregar(e, "Escriba un script en Python que lea un archivo CSV, filtre las filas donde la columna 'ventas' sea mayor a 1000, y guarde el resultado en un nuevo CSV.",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 180, true, true);

        Agregar(e, "Explique la diferencia entre aprendizaje supervisado y no supervisado. Dé dos ejemplos de algoritmos para cada tipo.",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 180, false, true);

        var p5 = Agregar(e, "¿Qué hace el método .groupby() en pandas?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Facil, 30, false, false);
        Opc(e, p5.Id, "Ordena el DataFrame alfabéticamente", null);
        Opc(e, p5.Id, "Agrupa filas por una o más columnas para aplicar funciones de agregación", 2);
        Opc(e, p5.Id, "Elimina filas duplicadas", null);
        Opc(e, p5.Id, "Une dos DataFrames por una columna común", null);

        Agregar(e, "¿Qué es overfitting en machine learning y qué técnicas se usan para prevenirlo?",
            TipoPregunta.TextoLibre, NivelDificultad.Dificil, 240, false, true);

        var p7 = Agregar(e, "Seleccione los tipos de gráficos disponibles en matplotlib:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Facil, 30, false, false);
        Opc(e, p7.Id, "Línea (plot)", 1);
        Opc(e, p7.Id, "Barras (bar)", 1);
        Opc(e, p7.Id, "Dispersión (scatter)", 1);
        Opc(e, p7.Id, "Histograma (hist)", 1);
        Opc(e, p7.Id, "3D hologram", null);

        Agregar(e, "Explique qué es un DataFrame en pandas y cómo se diferencia de un array de NumPy.",
            TipoPregunta.TextoLibre, NivelDificultad.Medio, 120, false, true);

        var p9 = Agregar(e, "¿Cuál es la función de train_test_split en scikit-learn?",
            TipoPregunta.SeleccionUnica, NivelDificultad.Medio, 30, false, false);
        Opc(e, p9.Id, "Entrena un modelo y lo prueba automáticamente", null);
        Opc(e, p9.Id, "Divide un dataset en conjuntos de entrenamiento y prueba para validar el modelo", 2);
        Opc(e, p9.Id, "Combina dos datasets en uno solo", null);
        Opc(e, p9.Id, "Normaliza los datos antes de entrenar", null);

        var p10 = Agregar(e, "Seleccione las métricas de evaluación válidas para un modelo de clasificación:",
            TipoPregunta.SeleccionMultiple, NivelDificultad.Dificil, 60, false, false);
        Opc(e, p10.Id, "Accuracy", 1);
        Opc(e, p10.Id, "Precision", 1);
        Opc(e, p10.Id, "Recall (Sensitivity)", 1);
        Opc(e, p10.Id, "F1-Score", 1);
        Opc(e, p10.Id, "R-squared", null);

        return e;
    }
}
