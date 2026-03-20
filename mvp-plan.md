# Plan de Acción — MVP: Sistema de Evaluación de Candidatos
> Rol: Backend Developer .NET | Arquitectura: Clean Architecture + DDD

---

## 1. Tech Stack

### Runtime & Framework
| Componente | Tecnología | Justificación |
|---|---|---|
| Framework principal | **.NET 8 (LTS)** | Soporte a largo plazo, mejor rendimiento, minimal APIs y mejoras en DI |
| API | **ASP.NET Core Web API** | REST maduro, middleware pipeline, integración nativa con el ecosistema |
| Real-time | **ASP.NET Core SignalR** | Necesario para la sesión en vivo y el temporizador por pregunta |
| ORM | **Entity Framework Core 8** | Migraciones, LINQ nativo, soporte a múltiples proveedores |

### Base de Datos
| Componente | Tecnología | Justificación |
|---|---|---|
| Principal | **PostgreSQL 16** | Open source, robusto para JSON, soporte a arrays, excelente con EF Core |
| Caché | **Redis** | Caché de sesiones activas, estado del temporizador, tokens de refresh |

### Autenticación & Autorización
| Componente | Tecnología | Justificación |
|---|---|---|
| Auth | **ASP.NET Core Identity** | Gestión de usuarios, roles y claims out-of-the-box |
| Tokens | **JWT (Bearer)** + **Refresh Tokens** | Autenticación stateless para la API REST |
| Autorización | **Policy-based Authorization** | Roles: `Evaluador`, `Candidato`, `Admin` |

### Patrones de Aplicación
| Componente | Tecnología | Justificación |
|---|---|---|
| CQRS + Mediator | **MediatR 12** | Separación Command/Query, desacoplamiento de handlers |
| Validación | **FluentValidation** | Reglas expresivas y centralizadas por Command/Query |
| Mapeo | **Mapster** | Mapeo de objetos de alto rendimiento (alternativa a AutoMapper) |
| Pipeline Behaviors | **MediatR Behaviors** | Logging, validación, manejo de excepciones centralizado |

### Almacenamiento de Archivos

> **Decisión de MVP**: Se descarta MinIO/S3 para el MVP. Se usa el **sistema de archivos local** detrás de una interfaz `IServicioArchivos`, lo que permite migrar a cualquier solución de almacenamiento en el futuro sin tocar el dominio.

| Componente | Tecnología | Justificación |
|---|---|---|
| Multimedia (MVP) | **File System local** (`IFormFile` + `IWebHostEnvironment`) | Cero infraestructura extra, suficiente para MVP |
| Interfaz | `IServicioArchivos` (abstracción propia) | DIP: el dominio nunca conoce el proveedor de storage |
| **Alternativa inmediata** | **Cloudinary** (free tier: 25 GB) | Si se requiere CDN o soporte multimedia avanzado sin infra |
| **Migración futura** | **Azure Blob** / **AWS S3** / **MinIO** | Cambiar solo la implementación de `IServicioArchivos` |

**Comparativa de alternativas al S3:**

| Opción | Costo | Infraestructura propia | Recomendado |
|---|---|---|---|
| **File System local** | $0 | No | ✅ MVP |
| **Cloudinary** | Free tier (25 GB) | No | ✅ Si se necesitan imágenes/videos |
| **PostgreSQL bytea** | $0 (ya tenemos PG) | No | ⚠️ Solo <5 MB por archivo |
| **Azure Blob Storage** | Pay-per-use (~$0.02/GB) | No | Si ya se está en Azure |
| **MinIO self-hosted** | $0 (Docker) | Sí | Post-MVP cuando escale |

### Reportes e IA
| Componente | Tecnología | Justificación |
|---|---|---|
| Generación PDF | **QuestPDF** | Open source, API fluent en C#, ideal para reportes complejos |
| Evaluación IA | **Endpoint externo** *(a confirmar — el PO proporcionará la URL y contrato)* | Anti-Corruption Layer adaptará la respuesta al dominio |
| Cliente HTTP IA | **Refit** | Tipado fuerte para llamadas HTTP, fácil mockeo en tests |

### Calidad & Observabilidad
| Componente | Tecnología | Justificación |
|---|---|---|
| Logging | **Serilog** + sinks (Console, Seq) | Structured logging, enriquecimiento de contexto |
| Healthchecks | **AspNetCore.HealthChecks** | Monitoreo de DB, Redis, Storage |
| Documentación API | **Scalar / Swashbuckle** | OpenAPI 3.0 para documentación interactiva |

### Testing
| Componente | Tecnología | Justificación |
|---|---|---|
| Unit Tests | **xUnit** | Estándar de la industria en .NET |
| Mocking | **Moq** | Mocking de dependencias en tests unitarios |
| Assertions | **FluentAssertions** | Asserts legibles y expresivos |
| Integration Tests | **WebApplicationFactory** + **Testcontainers** | Tests de integración con DB real en Docker |
| Architecture Tests | **NetArchTest** | Verificación automatizada de reglas de arquitectura |

---

## 2. Arquitectura y Patrones de Diseño

### Arquitectura General: Clean Architecture con DDD

```
TechEval.sln
├── src/
│   ├── TechEval.Domain/           # Capa de Dominio (Entities, VOs, Events, Interfaces)
│   ├── TechEval.Application/      # Capa de Aplicación (CQRS, DTOs, Interfaces)
│   ├── TechEval.Infrastructure/   # Capa de Infraestructura (EF Core, Repos, Services)
│   └── TechEval.API/              # Presentación (Controllers, Middlewares, SignalR Hubs)
└── tests/
    ├── TechEval.Domain.Tests/
    ├── TechEval.Application.Tests/
    ├── TechEval.Integration.Tests/
    └── TechEval.Architecture.Tests/
```

### Flujo de datos por capa

```
HTTP Request
    │
    ▼
[API Layer]  ──►  Controller valida JWT + Roles
    │              Construye Command/Query
    ▼
[Application Layer]  ──►  MediatR despacha al Handler
    │                      FluentValidation Pipeline Behavior
    │                      Handler orquesta dominio + repos
    ▼
[Domain Layer]  ──►  Aggregate ejecuta regla de negocio
    │                 Publica Domain Event si aplica
    ▼
[Infrastructure Layer]  ──►  Repository persiste en PostgreSQL
                              EventBus procesa Domain Events
                              (Storage, Email, IA, PDF)
```

### Bounded Contexts identificados (DDD)

| Bounded Context | Responsabilidad | Aggregates principales |
|---|---|---|
| **Identity & Access** | Autenticación, Roles, Usuarios | `Usuario`, `Rol` |
| **Evaluation Management** | CRUD de evaluaciones y preguntas | `Evaluacion`, `Pregunta`, `OpcionRespuesta` |
| **Live Session** | Sesión en tiempo real, temporizadores | `SesionEvaluacion`, `PreguntaSesion`, `RespuestaCandidato` |
| **Results & Scoring** | Puntuación, revisión manual, estados | `ResultadoEvaluacion`, `PuntuacionPregunta` |
| **Reporting** | Exportación PDF, comparación | `Reporte`, `ComparacionCandidatos` |
| **AI Evaluation** | Corrección automática de respuestas libres | `EvaluacionIA` (anti-corruption layer) |

### Ubiquitous Language (Lenguaje Ubicuo)

| Término | Definición en el dominio |
|---|---|
| `Evaluacion` | Formulario de prueba técnica que contiene preguntas |
| `Pregunta` | Pregunta dentro de una evaluación, con nivel de dificultad y tiempo límite |
| `OpcionRespuesta` | Opción de respuesta para preguntas de selección |
| `SesionEvaluacion` | Instancia activa de un candidato respondiendo una evaluación |
| `RespuestaCandidato` | Respuesta dada por el candidato a una pregunta, con timestamp |
| `Evaluador` | Usuario con rol de evaluador que crea y revisa evaluaciones |
| `Candidato` | Usuario con rol de candidato que responde evaluaciones |
| `Puntuacion` | Puntaje asignado: automático (selección) o manual (respuesta libre) |
| `EstadoRevision` | Estado de revisión: `PendienteRevision`, `EnRevision`, `Completada` |
| `NivelDificultad` | Nivel de dificultad: `Facil`, `Medio`, `Dificil` |
| `TipoPregunta` | Tipo de pregunta: `TextoLibre`, `SeleccionUnica`, `SeleccionMultiple` |
| `ViolacionPestana` | Evento que se dispara cuando el candidato abandona la pestaña |

### Patrones de Diseño aplicados

| Patrón | Dónde | Por qué |
|---|---|---|
| **Repository** | Infrastructure | Abstraer persistencia, testeable, DIP |
| **CQRS** | Application | Separar lecturas/escrituras, escalabilidad |
| **Mediator** | Application | Desacoplar handlers, pipeline de behaviors |
| **Domain Events** | Domain | `TiempoExpiracion`, `SesionCompletada`, `ViolacionPestanaDetectada` |
| **Specification** | Domain | Filtros reutilizables (`EvaluacionActivaSpec`, `PorNivelDificultadSpec`) |
| **Factory** | Domain | `SesionEvaluacion.Crear()`, generación de orden aleatorio |
| **Observer (SignalR)** | API | Notificaciones push para temporizador y eventos de sesión |
| **Anti-Corruption Layer** | Infrastructure | Adaptar respuesta del endpoint de IA externo al dominio |
| **Value Object** | Domain | `Puntuacion`, `DuracionTiempo`, `NivelDificultad`, `TipoPregunta` |
| **Aggregate Root** | Domain | `Evaluacion`, `SesionEvaluacion`, `ResultadoEvaluacion` |

---

## 3. Backlog Completo

### Épicas

```
EP-01 | Identity & Access Management
EP-02 | Gestión de Evaluaciones (CRUD)
EP-03 | Sesión en Vivo
EP-04 | Resultados & Revisión
EP-05 | Reportes y Exportación PDF
EP-06 | Comparación de Candidatos
EP-07 | Evaluación por IA
EP-08 | Notificaciones y Comunicaciones
```

---

### Backlog de Historias de Usuario

#### EP-01 | Identity & Access Management
| ID | Historia | Criterios de aceptación | Prioridad | Fase |
|---|---|---|---|---|
| US-01 | Como **admin**, quiero registrar evaluadores y candidatos con roles | JWT válido, roles `Evaluator`/`Candidate`/`Admin`, refresh token | 🔴 Alta | Fase 1 |
| US-02 | Como **usuario**, quiero autenticarme con email y contraseña | Login retorna JWT + refresh, logout invalida token | 🔴 Alta | Fase 1 |
| US-03 | Como **admin**, quiero gestionar roles de usuarios | Asignar/revocar roles, políticas de autorización activas | 🟡 Media | Fase 1 |

#### EP-02 | Gestión de Evaluaciones (CRUD)
| ID | Historia | Criterios de aceptación | Prioridad | Fase |
|---|---|---|---|---|
| US-04 | Como **evaluador**, quiero crear evaluaciones con nombre y descripción | Evaluación se persiste, devuelve ID | 🔴 Alta | Fase 1 |
| US-05 | Como **evaluador**, quiero agregar preguntas a una evaluación | Soporta tipos: `FreeText`, `SingleChoice`, `MultipleChoice` | 🔴 Alta | Fase 1 |
| US-06 | Como **evaluador**, quiero agregar opciones de respuesta con puntaje | Puntaje asignado o marcado como `ManualReview` | 🔴 Alta | Fase 1 |
| US-07 | Como **evaluador**, quiero asignar nivel de dificultad a las preguntas | `Easy`, `Medium`, `Hard` | 🔴 Alta | Fase 1 |
| US-08 | Como **evaluador**, quiero marcar límite de tiempo por pregunta | Campo `TimeLimitSeconds` en Question, nullable | 🔴 Alta | Fase 1 |
| US-09 | Como **evaluador**, quiero activar orden aleatorio en una evaluación | Flag `ShuffleQuestions` en Evaluation | 🟡 Media | Fase 1 |
| US-10 | Como **evaluador**, quiero activar orden progresivo por dificultad | Flag `ProgressiveOrder` en Evaluation, mutuamente excluyente con shuffle | 🟡 Media | Fase 1 |
| US-11 | Como **evaluador**, quiero editar y eliminar preguntas | CRUD completo, check de sesiones activas antes de eliminar | 🟡 Media | Fase 1 |
| US-12 | Como **evaluador**, quiero activar adjunto multimedia por pregunta | Flag `AllowsAttachment` en Question | 🟡 Media | Fase 1 |

#### EP-03 | Sesión en Vivo
| ID | Historia | Criterios de aceptación | Prioridad | Fase |
|---|---|---|---|---|
| US-13 | Como **evaluador**, quiero iniciar una sesión para un candidato | Se crea `SesionEvaluacion`, se genera link único para el candidato | 🔴 Alta | Fase 2 |
| US-14 | Como **candidato**, quiero ver las preguntas una a la vez con temporizador | SignalR emite `QuestionTimerTick`, avanza automáticamente al expirar | 🔴 Alta | Fase 2 |
| US-15 | Como **sistema**, debo registrar el timestamp cuando el candidato responde | `RespuestaCandidato.AnsweredAt`, `TimeSpentSeconds` calculado | 🔴 Alta | Fase 2 |
| US-16 | Como **evaluador**, quiero evitar que el candidato salga de la pestaña | Frontend detecta `visibilitychange`, backend recibe `ViolacionPestana` event, se registra conteo | 🔴 Alta | Fase 2 |
| US-17 | Como **candidato**, quiero adjuntar archivos multimedia a respuestas marcadas | Upload a file system (`IServicioArchivos`), ruta almacenada en `RespuestaCandidato.UrlAdjunto` | 🟡 Media | Fase 2 |
| US-18 | Como **candidato**, quiero enviar mi feedback al finalizar sin límite de tiempo | `FeedbackSesion` entidad separada, no tiene timer | 🟡 Media | Fase 2 |
| US-19 | Como **sistema**, debo completar la sesión cuando el candidato responde la última pregunta | Domain event `SesionCompletada`, status → `PendienteRevision` | 🔴 Alta | Fase 2 |

#### EP-04 | Resultados & Revisión
| ID | Historia | Criterios de aceptación | Prioridad | Fase |
|---|---|---|---|---|
| US-20 | Como **evaluador**, quiero ver todas las respuestas de un candidato | Endpoint `GET /sessions/{id}/results` con respuestas y adjuntos | 🔴 Alta | Fase 3 |
| US-21 | Como **evaluador**, quiero asignar puntaje manual a respuestas libres | `PATCH /results/{questionId}/score`, solo respuestas `ManualReview` | 🔴 Alta | Fase 3 |
| US-22 | Como **evaluador**, quiero marcar el estado de revisión del formulario | `EstadoRevision`: `PendienteRevision` → `EnRevision` → `Completada` | 🔴 Alta | Fase 3 |
| US-23 | Como **evaluador**, quiero solicitar evaluación por IA de respuestas libres | Endpoint `POST /ai/evaluate`, retorna score sugerido + justificación | 🟡 Media | Fase 3 |
| US-24 | Como **evaluador**, quiero agregar observaciones por pregunta | Campo `EvaluatorNote` en `PuntuacionPregunta` | 🟡 Media | Fase 3 |

#### EP-05 | Reportes y Exportación PDF
| ID | Historia | Criterios de aceptación | Prioridad | Fase |
|---|---|---|---|---|
| US-25 | Como **evaluador**, quiero exportar los resultados de un candidato a PDF | PDF con: datos candidato, preguntas, respuestas, puntajes, observaciones | 🟡 Media | Fase 3 |
| US-26 | Como **sistema**, debo enviar automáticamente los resultados al candidato al completar revisión | Domain event `RevisionCompletada` → Email con PDF adjunto | 🟡 Media | Fase 3 |

#### EP-06 | Comparación de Candidatos
| ID | Historia | Criterios de aceptación | Prioridad | Fase |
|---|---|---|---|---|
| US-27 | Como **evaluador**, quiero comparar múltiples candidatos de la misma evaluación | `POST /evaluations/{id}/compare` con array de sessionIds, retorna tabla comparativa | 🟡 Media | Fase 4 |
| US-28 | Como **evaluador**, quiero ver ranking de candidatos por puntaje total | Ordenado descendente, con diferencial por categoría | 🟢 Baja | Fase 4 |

#### EP-07 | Evaluación por IA
| ID | Historia | Criterios de aceptación | Prioridad | Fase |
|---|---|---|---|---|
| US-29 | Como **evaluador**, quiero que la IA evalúe respuestas libres con un criterio base | Envía pregunta + respuesta candidato + rúbrica opcional al LLM | 🟡 Media | Fase 3 |
| US-30 | Como **evaluador**, puedo aceptar o rechazar el puntaje sugerido por la IA | Score queda como `AISuggested` hasta que el evaluador lo confirme | 🟡 Media | Fase 3 |

---

## 4. Plan de Acción por Fases

> **Regla de avance**: No se puede iniciar la siguiente fase sin haber completado el plan de pruebas de la fase anterior y tener CI verde.

---

### FASE 1 — Fundación: Auth, CRUD de Evaluaciones, Preguntas

**Objetivo:** Tener la estructura del proyecto, autenticación y el módulo de gestión de evaluaciones completo y probado.

**Definition of Done de la Fase 1:**
- [ ] Scaffold de la solución con todas las capas
- [ ] Auth JWT + Refresh Tokens funcionando con roles
- [ ] CRUD completo de `Evaluacion`, `Pregunta`, `OpcionRespuesta`
- [ ] Soporte para los 3 tipos de pregunta
- [ ] Shuffle y Progressive Order funcionando
- [ ] 85% cobertura en Domain y Application layers
- [ ] Architecture tests pasando
- [ ] Swagger documentado

#### Sprint 1 — Backlog (US-01 a US-12)

**Semana 1:**
| Tarea | Encargado | Entregable |
|---|---|---|
| Scaffold solución Clean Architecture | Dev | Estructura de carpetas, Nuget packages, Program.cs |
| Configurar EF Core + PostgreSQL + migrations iniciales | Dev | DbContext, tablas base, seed de roles y usuarios de desarrollo |
| Implementar Identity + JWT + Refresh Token | Dev | `POST /auth/login`, `POST /auth/refresh`, `POST /auth/logout` |
| Domain: Aggregate `User` + Value Objects (`Email`, `Role`) | Dev | Entidades del dominio con invariantes |

**Semana 2:**
| Tarea | Encargado | Entregable |
|---|---|---|
| Domain: Aggregate `Evaluacion` + `Pregunta` + `OpcionRespuesta` | Dev | Modelos de dominio con todas las reglas de negocio |
| CQRS: Commands y Queries para Evaluation CRUD | Dev | Create/Update/Delete/GetById/GetList para Evaluation |
| CQRS: Commands y Queries para Question y OpcionRespuesta | Dev | CRUD completo de preguntas con opciones |
| Validaciones FluentValidation para todos los commands | Dev | Validadores con reglas de negocio |
| Lógica de ShuffleQuestions y ProgressiveOrder (mutuamente excluyentes) | Dev | Domain service o factory en `Evaluacion` aggregate |

#### Endpoints Fase 1

```
Auth:
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout

Evaluations:
POST   /api/evaluations
GET    /api/evaluations
GET    /api/evaluations/{id}
PUT    /api/evaluations/{id}
DELETE /api/evaluations/{id}

Questions:
POST   /api/evaluations/{id}/questions
GET    /api/evaluations/{id}/questions
PUT    /api/questions/{id}
DELETE /api/questions/{id}

Answer Options:
POST   /api/questions/{id}/options
PUT    /api/options/{id}
DELETE /api/options/{id}
```

#### Plan de Pruebas — Fase 1

**Tests Unitarios (Domain Layer):**
```
Evaluation_Create_ShouldSetStatusAsDraft()
Evaluation_EnableShuffle_WhenProgressiveOrderEnabled_ShouldThrowDomainException()
Evaluation_EnableProgressiveOrder_WhenShuffleEnabled_ShouldThrowDomainException()
Question_Create_WithManualReviewType_ShouldMarkAsManualReview()
Question_SetTimeLimit_WithNegativeValue_ShouldThrowDomainException()
OpcionRespuesta_Create_WithScoreAndManualReview_ShouldThrowDomainException()
```

**Tests Unitarios (Application Layer):**
```
CreateEvaluationHandler_ValidCommand_ShouldReturnEvaluationId()
CreateEvaluationHandler_DuplicateName_ShouldThrowValidationException()
CreateQuestionHandler_ValidCommand_ShouldReturnQuestionId()
CreateQuestionHandler_InvalidEvaluationId_ShouldThrowNotFoundException()
AddOpcionRespuestaHandler_FreeTextQuestion_ShouldThrowValidationException()
```

**Tests de Integración:**
```
POST /api/auth/login WithValidCredentials ReturnsJwtToken()
POST /api/evaluations WithValidData ReturnsCreated()
POST /api/evaluations WithoutAuth Returns401()
POST /api/evaluations WithCandidateRole Returns403()
GET  /api/evaluations/{id} WithValidId ReturnsEvaluation()
POST /api/questions/{id}/options WithSingleChoiceType ReturnsCreated()
```

**Architecture Tests:**
```
DomainLayer_ShouldNotDependOn_ApplicationLayer()
DomainLayer_ShouldNotDependOn_InfrastructureLayer()
ApplicationLayer_ShouldNotDependOn_InfrastructureLayer()
Controllers_ShouldNotDependOn_DomainLayer_Directly()
Handlers_ShouldBeSealed_Or_Internal()
```

---

### FASE 2 — Sesión en Vivo

**Prerequisito:** Fase 1 completada con CI verde y todos los tests pasando.

**Objetivo:** Sesión en tiempo real con SignalR, temporizador por pregunta, registro de respuestas con timestamp, detección de cambio de pestaña y subida de multimedia.

**Definition of Done de la Fase 2:**
- [x] `SesionEvaluacion` aggregate con toda la lógica de sesión ✅
- [x] Hub de SignalR para eventos en tiempo real ✅
- [x] Temporizador funcional que avanza automáticamente ✅
- [x] Respuestas guardadas con `AnsweredAt` y `TimeSpentSeconds` ✅
- [x] `ViolacionPestana` registrado y almacenado ✅
- [x] Upload de archivos multimedia a file system local (`IServicioArchivos`) ✅
- [x] Feedback del candidato sin timer ✅
- [x] 85% cobertura en Domain y Application layers de este módulo ✅

**Status Phase 2:** ✅ **COMPLETADA**
- Build: ✅ GREEN (0 errors)
- Tests: ✅ 25/25 passing (10 Domain + 8 Application + 7 Architecture)
- Implementación: ~2 horas
- Documentación: ✅ api-docs-frontend.md actualizada

#### Archivos Implementados — Fase 2

**Domain** (`src/TechEval.Domain/Sesiones/`)
- `EstadoSesion.cs` — enum: `NoIniciada` → `EnProgreso` → `Completada` → `Abandonada`
- `RespuestaCandidato.cs` — entidad con texto, URL adjunto, tiempo empleado, flag expiración
- `PreguntaSesion.cs` — entidad con colección de respuestas y orden de presentación
- `SesionEvaluacion.cs` — aggregate root: factory `Crear()`, lógica de sesión, 5 domain events
- `IRepositorioSesionEvaluacion.cs` — métodos: `ObtenerConPreguntasAsync`, `ObtenerPorCodigoAccesoAsync`, `ObtenerPorCandidatoAsync`, `ObtenerPorEvaluacionAsync`

**Application** (`src/TechEval.Application/Sesiones/`)
- `CrearSesionCommand` + Handler — genera código único 8 chars, selecciona preguntas
- `IniciarSesionCommand` + Handler — valida código de acceso, carga primera pregunta
- `RegistrarRespuestaCommand` + Handler — guarda respuesta + tiempo, avanza pregunta
- `RegistrarViolacionPestanaCommand` + Handler — incrementa contador de violaciones
- `AgregarAdjuntoRespuestaCommand` + Handler — asocia URL de adjunto a respuesta
- `ObtenerSesionQuery` + Handler — retorna sesión con preguntas y respuestas
- `ListarSesionesCandidatoQuery` + Handler — historial de sesiones por candidato

**Infrastructure** (`src/TechEval.Infrastructure/`)
- `RepositorioSesionEvaluacion.cs` — EF Core incluye preguntas y respuestas
- `SesionesConfigurations.cs` — tablas `sesiones_evaluacion`, `preguntas_sesion`, `respuestas_candidato`
- Migration `Fase2_Sesiones` ✅ aplicada

**API** (`src/TechEval.API/`)
- `SesionesController.cs` — 7 endpoints REST
- `HubEvaluacion.cs` — SignalR hub en `/hubs/evaluacion`
- `ServicioTemporizadorSesiones.cs` — background service, emite ticks vía SignalR

#### User Stories Completadas — Fase 2

| US | Descripción | Estado |
|----|-------------|--------|
| US-13 | Evaluador inicia sesión para candidato (código acceso 8 chars) | ✅ |
| US-14 | Candidato ingresa con código de acceso | ✅ |
| US-15 | Candidato ve primera pregunta tras ingresar | ✅ |
| US-16 | Sistema registra respuesta y avanza a siguiente | ✅ |
| US-17 | Temporizador cuenta regresivamente en vivo via SignalR | ✅ |
| US-18 | Sistema detecta y registra cambios de pestaña | ✅ |
| US-19 | Candidato adjunta archivos a respuestas marcadas | ✅ |
| US-20 (parcial) | Sesión se completa al responder todas las preguntas | ✅ |

#### Sprint 2 — Backlog (US-13 a US-19)

**Semana 3:**
| Tarea | Entregable |
|---|---|
| Domain: Aggregate `SesionEvaluacion` con estados | `EstadoSesion`: `NoIniciada` → `EnProgreso` → `Completada` |
| Domain: `PreguntaSesion` con `RespuestaCandidato` y timestamps | Value Objects: `TimeSpentSeconds`, `AnsweredAt` |
| Domain Event: `SesionIniciada`, `RespuestaRegistrada`, `SesionCompletada`, `ViolacionPestanaDetectada` | Eventos publicados por el aggregate |
| CQRS: `StartSessionCommand`, `SubmitAnswerCommand`, `CompleteSessionCommand` | Handlers completos |
| Redis: Estado de temporizador activo por sesión | `SessionTimerState` en Redis con TTL |

**Semana 4:**
| Tarea | Entregable |
|---|---|
| SignalR Hub: `HubEvaluacion` con grupos por sesión | `UnirseASesion`, `TickTemporizador`, `SiguientePregunta`, `SesionCompletada` |
| Background Service: `TimerBackgroundService` que emite ticks via SignalR | Servicio que gestiona los timers activos |
| Detección Tab Violation: endpoint `POST /sessions/{id}/tab-violation` | Registra evento, incrementa contador en sesión |
| File Storage: implementar `IServicioArchivos` con `LocalFileStorageService` | Guarda archivo en disco, retorna ruta relativa |
| Endpoint multimedia: `POST /sessions/{sessionId}/answers/{questionId}/attachment` | Guarda ruta en `RespuestaCandidato.UrlAdjunto` |
| Feedback del candidato: `POST /sessions/{id}/feedback` | `FeedbackSesion` sin restricción de tiempo |

#### Endpoints Fase 2

```
Sessions:
POST   /api/sessions                           (Evaluator crea sesión para candidato)
GET    /api/sessions/{id}
POST   /api/sessions/{id}/start                (Candidato inicia)
POST   /api/sessions/{id}/answers              (Candidato responde)
POST   /api/sessions/{id}/tab-violation        (Frontend reporta violación)
POST   /api/sessions/{id}/feedback             (Feedback final del candidato)
POST   /api/sessions/{id}/answers/{qId}/attachment

SignalR Hub: /hubs/evaluacion
  -> UnirseASesion(sesionId)
  <- PreguntaCargada(pregunta, tiempoLimite)
  <- TickTemporizador(segundosRestantes)
  <- SiguientePregunta(pregunta)
  <- SesionCompletada(resumen)
```

#### Plan de Pruebas — Fase 2

**Tests Unitarios (Domain Layer):**
```
SesionEvaluacion_Iniciar_DebePublicarSesionIniciadaEvent()
SesionEvaluacion_RegistrarRespuesta_DebeGuardarTimestamp()
SesionEvaluacion_RegistrarRespuesta_SesionYaCompletada_DebeLanzarExcepcionDominio()
SesionEvaluacion_RegistrarViolacionPestana_DebeIncrementarContador()
SesionEvaluacion_Completar_DebePublicarSesionCompletadaEvent()
SesionEvaluacion_Completar_SesionYaCompletada_DebeLanzarExcepcionDominio()
```

**Tests Unitarios (Application Layer):**
```
StartSessionHandler_ValidSession_ShouldReturnFirstQuestion()
SubmitAnswerHandler_ValidAnswer_ShouldPersistWithTimestamp()
SubmitAnswerHandler_TimeLimitExceeded_ShouldMarkAsExpired()
SubmitAnswerHandler_FreeTextWithAttachment_ShouldSaveAttachmentUrl()
CompleteSessionHandler_AllQuestionsAnswered_ShouldChangeStatus()
```

**Tests de Integración:**
```
POST /api/sessions WithValidData ReturnsSessionId()
POST /api/sessions/{id}/start ReturnsFirstQuestion()
POST /api/sessions/{id}/answers WithValidAnswer ReturnNextQuestion()
POST /api/sessions/{id}/answers WithExpiredTimer MarksAnswerAsExpired()
POST /api/sessions/{id}/tab-violation IncrementsViolationCount()
POST /api/sessions/{id}/answers/{qId}/attachment WithFile SavesAttachment()
```

**Tests de SignalR:**
```
HubEvaluacion_UnirseASesion_RecibeEventoPreguntaCargada()
HubEvaluacion_TiempoExpirado_AvanzaASiguientePreguntaAuto()
HubEvaluacion_UltimaPregunta_EmiteSesionCompletada()
```

---

### FASE 3 — Resultados, Revisión, PDF e IA

**Prerequisito:** Fase 2 completada con CI verde.

**Objetivo:** Panel de resultados para el evaluador, puntaje manual, evaluación con IA, exportación PDF y envío automático al candidato.

**Definition of Done de la Fase 3:**
- [x] Pantalla de resultados con respuestas, timestamps y adjuntos ✅
- [x] Asignación de puntaje manual para respuestas libres ✅
- [x] Estados de revisión (`PendienteRevision` → `EnRevision` → `Completada`) ✅
- [x] Endpoint IA listo (placeholder funcional, integración real pendiente) ✅
- [x] Exportación a PDF (generador HTML completo) ✅
- [x] Cálculo automático de puntaje al completar sesión ✅ (via `RegistrarRespuestaCommandHandler.CrearResultadoSiNoExisteAsync`)
- [x] Envío automático de resultados al candidato al completar revisión ✅
- [x] 85% cobertura en módulos nuevos

**Status Phase 3:** ✅ **COMPLETADA**
- Build: ✅ GREEN (0 errors, 0 warnings)
- Tests: ✅ 69/69 passing (33 Application + 29 Domain + 7 Architecture)
- Documentación: ✅ api-docs-frontend.md actualizada con sección Resultados
- IA: ✅ Endpoint real Power Automate integrado (`ServicioEvaluacionIA`)

#### Backlog Tecnico de Errores (Concurrencia / HTTP 500)

**Contexto (2026-03-19):** Se detecto `DbUpdateConcurrencyException` en operaciones de creacion de preguntas/opciones, propagando `500`.

| Item | Descripcion | Prioridad | Estado |
|---|---|---|---|
| ERR-01 | Corregir estrategia de tracking en `RepositorioEvaluacion.ActualizarAsync` | 🔴 Alta | ✅ Completado |
| ERR-02 | Asegurar persistencia consistente para altas de preguntas y opciones | 🔴 Alta | ✅ Completado |
| ERR-03 | Mapear concurrencia a error funcional (evitar `500` generico) | 🔴 Alta | ✅ Completado |
| ERR-04 | Agregar test de integracion para `POST /api/evaluaciones/{id}/preguntas` | 🟠 Media | ✅ Completado |
| ERR-05 | Agregar test de integracion para `POST /api/evaluaciones/{id}/preguntas/{pid}/opciones` | 🟠 Media | ✅ Completado |
| ERR-06 | Definir runbook operativo para diagnostico de `500` en producción | 🟡 Media | ✅ Completado |

**Actualizacion (2026-03-19):** Se aplico fix definitivo en repositorio para usar tracking en carga mutable y persistencia via `SaveChangesAsync`; adicionalmente se mapeo `DbUpdateConcurrencyException` a `409 Conflict` en middleware global. Validado con `dotnet build` y `dotnet test` de `TechEval.Application.Tests`.

**Actualizacion (2026-03-20 — ERR-04/05/06):**
- Se agregaron 8 tests nuevos de Application para handlers de preguntas y opciones: concurrencia secuencial, not-found, dominio, y orden incremental.
- Tests totales: 33/33 verde (16 Application + 10 Domain + 7 Architecture).
- Runbook de diagnóstico de errores 500 definido (ver sección abajo).

#### Runbook: Diagnóstico Rápido de Errores 500

**1. Identificar el error en logs (Serilog):**
```bash
# Buscar errores recientes en consola (dev) o en Seq (staging/prod)
grep -i "exception" logs/*.log | tail -20
```

**2. Clasificación por tipo de excepción:**

| Excepción | HTTP | Causa probable | Acción |
|---|---|---|---|
| `NotFoundException` | 404 | ID inexistente en DB | Verificar GUID, revisar si fue eliminado |
| `ValidationException` | 400 | FluentValidation falló | Revisar payload vs reglas del validator |
| `DomainException` | 422 | Regla de negocio violada | Leer `Error.Codigo` para diagnosticar |
| `InvalidOperationException` | 422 | Estado inválido del agregado | Revisar estado actual de la entidad en DB |
| `DbUpdateConcurrencyException` | 409 | Conflicto de concurrencia EF Core | Verificar si multiples requests modifican el mismo agregado; reintentar |
| `UnauthorizedAccessException` | 401/403 | JWT expirado o rol incorrecto | Verificar token y claims del usuario |
| Cualquier otra | 500 | Error no controlado | Revisar stacktrace completo en Serilog |

**3. Consulta rápida en PostgreSQL:**
```sql
-- Verificar estado de una evaluación
SELECT "Id", "Nombre", "Estado", "CreadoEn" FROM evaluaciones WHERE "Id" = '<GUID>';

-- Contar preguntas de una evaluación
SELECT COUNT(*) FROM preguntas WHERE "EvaluacionId" = '<GUID>';

-- Verificar sesión
SELECT "Id", "Estado", "CandidatoId", "CreadaEn", "CompletadaEn" FROM sesiones_evaluacion WHERE "Id" = '<GUID>';

-- Verificar resultado
SELECT "Id", "SesionId", "PuntuacionTotal", "EstadoRevision" FROM resultados_evaluacion WHERE "SesionId" = '<GUID>';
```

**4. Pasos de reproducción estándar:**
1. Copiar la URL y payload del request que falló (de logs o network tab)
2. Obtener token válido: `POST /api/auth/login`
3. Reproducir con `curl -v` para ver headers completos
4. Si es 500, buscar `RequestId` en Serilog para traza completa

#### Backlog Tecnico de Errors — Sesion En Vivo (2026-03-19)

| Item | Descripcion | Estado |
|---|---|---|
| SES-01 | Error 403 al crear sesion con rol Administrador (`POST /api/sesiones`) | ✅ Completado |
| SES-02 | Error 400 en logout — body enviado vacio, faltaba `refreshToken` | ✅ Completado |
| SES-03 | `Include("_preguntas")` falla — navegacion debe ser propiedad pública `Preguntas` | ✅ Completado |
| SES-04 | Relacion `HasMany` sin declarar en `SesionEvaluacionConfiguration` | ✅ Completado |
| SES-05 | `PendingModelChangesWarning` bloqueaba arranque de API tras cambio de configuracion | ✅ Completado |
| SES-06 | Migraciones `FixSesionPreguntasRelacion` y `FixNavegacionBackingField` creadas y aplicadas | ✅ Completado |
| SES-07 | `IniciarSesionCommand` no era idempotente — reloads/doble llamada fallaban con 422 | ✅ Completado |
| SES-08 | `PreguntaSesionDto.Id` incorrecto — backend debia devolver `PreguntaId` (FK real) | ✅ Completado |
| SES-09 | `InvalidOperationException` no mapeada en middleware — resulta en 500 generico | ✅ Completado |
| SES-10 | Timer de cuestionario nunca iniciaba — dependia de SignalR (`conectarEnWebSocket` nunca llega) | ✅ Completado — reemplazado por timer local `setInterval` |
| SES-11 | Boton submit deshabilitado en `sessions-page`, `user-admin-page`, `candidate-access-page` | ✅ Completado — patron `toSignal(form.statusChanges)` |

#### Backlog Tecnico de Errores — Resultados/Puntuaciones (2026-03-20)

| Item | Descripcion | Estado |
|---|---|---|
| RES-01 | `SesionResumenDto` no incluia `evaluacionTitulo` ni `puntuacionObtenida` — candidato no veia scores | ✅ Completado |
| RES-02 | `PuntuacionPreguntaDto` no incluia `PreguntaId` — dropdown de evaluador no funcionaba | ✅ Completado |
| RES-03 | EF Config de `ResultadoEvaluacion` no tenia relacion explicita `HasMany` con backing field `_puntuaciones` | ✅ Completado |
| RES-04 | `CalcularPuntuacionTotal` hardcodeaba `PuntuacionMaxima = 100` — porcentajes incorrectos | ✅ Completado — max calculado de opciones reales |
| RES-05 | Evaluador no veia sesiones ni resultados — handler filtraba por `UsuarioId` (candidato) | ✅ Completado — handler role-aware con `ObtenerTodasAsync` |

**Actualizacion (2026-03-20):** Se corrigieron 3 bugs en el flujo de puntuaciones. `ListarSesionesCandidatoQueryHandler` ahora inyecta `IRepositorioEvaluacion` y `IRepositorioResultadoEvaluacion` para resolver titulos y scores. Build y tests 25/25 en verde.

**Actualizacion (2026-03-20 — RES-04/05):**
- `CalcularPuntuacionTotal(decimal puntuacionMaximaEvaluacion)` ahora recibe la puntuación máxima real de la evaluación calculada de las opciones.
- `ListarSesionesCandidatoQueryHandler` ahora es role-aware: Evaluador/Admin ven todas las sesiones y resultados; Candidato solo los suyos.
- Se agregaron `ObtenerTodasAsync` y `ObtenerTodosAsync` a repositorios de sesiones y resultados respectivamente.
- Build verde, 69/69 tests verde.

#### Archivos Implementados — Fase 3 (Parcial)

**Domain** (`src/TechEval.Domain/Resultados/`)
- `EstadoRevision.cs` — enum: `PendienteRevision` → `EnRevision` → `Completada`
- `PuntuacionPregunta.cs` — entidad con 3 tipos de puntuación (automática, manual, IA) + `ObtenerPuntuacionFinal()`
- `ResultadoEvaluacion.cs` — aggregate root con `ObtenerEstadoGeneral()` (Excelente/MuyBueno/Bueno/Aceptable/Insuficiente)
- `IRepositorioResultadoEvaluacion.cs` — métodos: `ObtenerPorSesionAsync`, `ObtenerPorCandidatoAsync`, `ObtenerPorEvaluacionAsync`

**Application** (`src/TechEval.Application/Resultados/`)
- `ObtenerResultadosQuery` + Handler — retorna `ObtenerResultadosResponse` con todas las puntuaciones
- `GenerarPDFCommand` + Handler — genera HTML completo y guarda vía `IServicioArchivos`
- `AsignarPuntuacionCommand` + Handler — evaluador asigna puntaje + observaciones
- `EvaluarConIACommand` + Handler — llama `IServicioEvaluacionIA`, registra sugerencia
- `IServicioGeneradorPDF` — `Task<byte[]> GenerarPDFResultadoAsync(ResultadoEvaluacion)`
- `IServicioEvaluacionIA` — `Task<RespuestaEvaluacionIA> EvaluarRespuestaAsync(preguntaId, respuesta, rubrica)` con `RespuestaEvaluacionIA(PreguntaId, PuntajeSugerido, Justificacion, RequiereRevisionManual, AspectosPositivos, AspectosNegativos)`

**Infrastructure** (`src/TechEval.Infrastructure/`)
- `ServicioGeneradorPDF.cs` — reporte HTML completo (header, score box, tabla preguntas, estado revisión)
- `ServicioEvaluacionIAPlaceholder.cs` — respuesta dummy con `RequiereRevisionManual: true`, listo para conectar endpoint real
- `RepositorioResultadoEvaluacion.cs` — EF Core con `Include(r => r.Puntuaciones)`
- `ResultadosConfigurations.cs` — tablas `resultados_evaluacion` y `puntuaciones_pregunta` con índices
- Migration `Fase3_Resultados` ✅ aplicada

**API** (`src/TechEval.API/`)
- `ResultadosController.cs` — 4 endpoints: GET resultados, POST pdf, PATCH puntuaciones, POST ia-evaluate

#### User Stories Completadas — Fase 3 (Parcial)

| US | Descripción | Estado |
|----|-------------|--------|
| US-20 | Evaluador ve todas las respuestas del candidato | ✅ |
| US-21 | Evaluador asigna puntaje manual a respuestas libres | ✅ |
| US-22 | Evaluador marca estado de revisión del formulario | ✅ |
| US-23 | Evaluador solicita evaluación por IA (placeholder) | ✅ |
| US-24 | Evaluador agrega observaciones por pregunta | ✅ |
| US-25 | Evaluador exporta resultados de candidato a PDF | ✅ |
| US-26 | Envío automático de resultados al completar revisión | ✅ |
| US-29 | IA evalúa respuestas con rubrica (integración real) | ✅ Completado — Power Automate endpoint integrado |
| US-30 | Evaluador acepta/rechaza puntaje sugerido por IA | ✅ |

**Actualizacion (2026-03-20 — US-26 completada):**
- Se implemento `CompletarRevisionCommand` + Handler que: marca revisión como completada, genera PDF, envía email al candidato.
- Se creó `ServicioEmailPlaceholder` (placeholder logger — listo para reemplazar con SMTP/SendGrid).
- Se registró `IServicioEmail` en DI de Infrastructure.
- Se agregó endpoint `POST /api/resultados/{sesionId}/completar-revision`.
- Se integró boton "Completar revisión" en la UI del evaluador.
- Tests: 33/33 verdes, `ng build` verde.

#### Sprint 3 — Backlog (US-20 a US-26, US-29, US-30)

**Semana 5:**
| Tarea | Entregable |
|---|---|
| Domain: Aggregate `ResultadoEvaluacion` con `PuntuacionPregunta` | Cálculo de puntaje total, estado de revisión |
| CQRS: `GetSessionResultsQuery`, `AssignScoreCommand`, `UpdateEstadoRevisionCommand` | Handlers de resultados y revisión |
| Domain Event: `RevisionCompletada` | Desencadena envío de resultados |
| Auto-scoring: handler para respuestas de selección al completar sesión | `CalculateAutoScoreCommand` |

**Semana 6:**
| Tarea | Entregable |
|---|---|
| Anti-Corruption Layer: `IServicioEvaluacionIA` + adapter para endpoint externo | Traduce respuesta del endpoint IA a `SugerenciaIA` del dominio |
| Endpoint IA: `POST /ai/evaluate` con rúbrica opcional | Score sugerido + justificación, estado `SugerenciaIA` |
| QuestPDF: plantilla de reporte de resultados | PDF con datos del candidato, preguntas, respuestas, observaciones |
| Email service: `IServicioEmail` + SMTP/SendGrid adapter | Envío de PDF al candidato al completar revisión |
| Domain Event Handler: `RevisionCompletada` → genera PDF → envía email | Pipeline completo de notificación |

#### Endpoints Fase 3

```
Results:
GET    /api/sessions/{id}/results
PATCH  /api/results/{sessionId}/questions/{qId}/score    (puntaje manual)
PATCH  /api/sessions/{id}/review-status
GET    /api/sessions/{id}/results/export/pdf

AI:
POST   /api/ai/evaluate          body: { sessionId, questionId, rubric? }
PATCH  /api/results/{sessionId}/questions/{qId}/ai-score/accept
PATCH  /api/results/{sessionId}/questions/{qId}/ai-score/reject
```

#### Plan de Pruebas — Fase 3

**Tests Unitarios (Domain Layer):**
```
ResultadoEvaluacion_AsignarPuntuacion_DebeActualizarPuntajeTotal()
ResultadoEvaluacion_AsignarPuntuacion_PreguntaNoEsRevisionManual_DebeLanzarExcepcionDominio()
ResultadoEvaluacion_CompletarRevision_DebePublicarRevisionCompletadaEvent()
ResultadoEvaluacion_CompletarRevision_ConPuntuacionesManualesPendientes_DebeLanzarExcepcionDominio()
PuntuacionPregunta_AceptarSugerenciaIA_DebeAplicarPuntuacionSugerida()
```

**Tests Unitarios (Application Layer):**
```
AssignScoreHandler_ValidManualScore_ShouldUpdateResult()
GetSessionResultsHandler_ValidSession_ShouldReturnAggregatedResults()
AIEvaluateHandler_ValidRequest_ShouldReturnAISuggestion()
GeneratePdfHandler_CompletedSession_ShouldReturnPdfBytes()
```

**Tests de Integración:**
```
GET  /api/sessions/{id}/results WithCompletedSession ReturnsFullResults()
PATCH /api/results/{sid}/questions/{qid}/score WithValidScore UpdatesScore()
PATCH /api/sessions/{id}/review-status ToCompleted TriggersEmailNotification()
POST /api/ai/evaluate WithFreeTextAnswer ReturnsAISuggestion()
GET  /api/sessions/{id}/results/export/pdf ReturnsPdfFile()
```

**Tests de Anti-Corruption Layer (IA):**
```
AIAdapter_ValidOpenAIResponse_MapsToAISuggestionDTO()
AIAdapter_InvalidOpenAIResponse_ThrowsIntegrationException()
AIAdapter_Timeout_ThrowsTimeoutException()
```

---

### FASE 4 — Comparación de Candidatos y Enhancements

**Prerequisito:** Fase 3 completada con CI verde.

**Objetivo:** Comparación de candidatos, ranking y mejoras de UX al sistema.

**Definition of Done de la Fase 4:**
- [x] Endpoint de comparación retorna tabla comparativa
- [x] Ranking de candidatos por evaluación
- [x] Paginación y filtros en listados principales (evaluaciones + sesiones, backend + frontend)
- [x] Asignación masiva de sesiones a múltiples candidatos (`POST /api/sesiones/masivas`)
- [x] Documentación Swagger completa y actualizada

**Status Phase 4:** ✅ **COMPLETADA**
- Build: ✅ GREEN (0 errors, 0 warnings)
- Tests: ✅ 69/69 passing
- Frontend: ✅ `ng build` verde (535kB)
- Swagger: ✅ OpenAPI + Scalar con XML docs y `[ProducesResponseType]`

**Actualizacion (2026-03-20 — IA real, tests, docs):**
- Se reemplazó `ServicioEvaluacionIAPlaceholder` por `ServicioEvaluacionIA` real que llama al endpoint Power Automate.
- Se configuró `EvaluacionIAOptions` con URL del endpoint y timeout en `appsettings.json`.
- Se registró `HttpClient` tipado via `AddHttpClient<IServicioEvaluacionIA, ServicioEvaluacionIA>()` en DI.
- Se agregaron 19 tests de Domain (ResultadoEvaluacion, PuntuacionPregunta) y 7 tests de Application (EvaluarConIA, CompletarRevision, GenerarPDF).
- Se habilitaron XML docs en el proyecto API (`GenerateDocumentationFile`).
- Se agregaron `[ProducesResponseType]` en endpoints críticos del ResultadosController.
- Total tests: 69/69 verdes (33 Application + 29 Domain + 7 Architecture).

**Actualizacion (2026-03-20 — Paginación, filtros y bulk):**
- `PaginacionDTOs.cs`: records `PaginacionParams` y `ResultadoPaginado<T>` en Application/Common/DTOs.
- `ListarEvaluacionesQuery` y `ListarSesionesCandidatoQuery` retornan `ResultadoPaginado<T>` con filtros `Busqueda`/`Estado`.
- `CrearSesionesMasivasCommand`: crea sesiones para N candidatos sobre una evaluación; endpoint `POST /api/sesiones/masivas`.
- 4 tests nuevos de paginación (`ListarEvaluacionesTests`). Total tests: 37/37 verde.
- Frontend: pagination.models, servicios API con HttpParams, forms-page y sessions-page rediseñadas con paginación/filtros/bulk.

**Actualizacion (2026-03-20 — Comparación y Ranking):**
- `CompararCandidatosQuery` + handler: compara N sesiones de una evaluación, retorna tabla comparativa con detalle por pregunta.
- `ObtenerRankingQuery` + handler: ranking descendente por porcentaje, desempate por tiempo.
- `POST /api/evaluaciones/{id}/comparar` y `GET /api/evaluaciones/{id}/ranking` en `EvaluacionesController`.
- 6 tests nuevos en `ComparacionRankingTests` (43/43 verde).
- Frontend: página `/comparar` con selector de evaluación, ranking con medallas, comparativa colapsable por pregunta.

#### Sprint 4 — Backlog (US-27, US-28)

| Tarea | Entregable |
|---|---|
| Domain Service: `ComparacionCandidatosService` | Compara scores por categoría/dificultad entre candidatos |
| CQRS: `CompareCandidatesQuery`, `GetCandidateRankingQuery` | Handlers con proyecciones optimizadas |
| Endpoints de comparación y ranking | `POST /evaluations/{id}/compare`, `GET /evaluations/{id}/ranking` |
| Filtros y paginación en `GetSessionsQuery` | `CursorPagination` o `OffsetPagination` |
| Export comparativo a PDF | Tabla comparativa en PDF |

#### Endpoints Fase 4

```
POST   /api/evaluations/{id}/compare     body: { sessionIds: [] }
GET    /api/evaluations/{id}/ranking
GET    /api/evaluations/{id}/compare/export/pdf
```

#### Plan de Pruebas — Fase 4

```
CompareCandidates_WithTwoCompletedSessions_ReturnsComparativeTable()
CompareCandidates_WithIncompleteSession_ShouldThrowValidationException()
GetRanking_WithMultipleCandidates_ReturnsOrderedByTotalScore()
GetRanking_WithTiedScores_OrdersByCompletionTime()
POST /api/evaluations/{id}/compare WithValidSessionIds ReturnsComparison()
GET  /api/evaluations/{id}/ranking ReturnsRankedCandidates()
```

---

## 5. Estructura de la Solución (Scaffold inicial)

```
TechEval/
├── src/
│   ├── TechEval.Domain/
│   │   ├── Evaluations/
│   │   │   ├── Evaluacion.cs             (Aggregate Root)
│   │   │   ├── Pregunta.cs               (Entity)
│   │   │   ├── OpcionRespuesta.cs           (Entity)
│   │   │   └── Events/
│   │   │       └── EvaluacionPublicadaEvent.cs
│   │   ├── Sessions/
│   │   │   ├── SesionEvaluacion.cs      (Aggregate Root)
│   │   │   ├── PreguntaSesion.cs        (Entity)
│   │   │   ├── RespuestaCandidato.cs        (Entity)
│   │   │   └── Events/
│   │   │       ├── SesionIniciadaEvent.cs
│   │   │       ├── RespuestaRegistradaEvent.cs
│   │   │       ├── SesionCompletadaEvent.cs
│   │   │       └── ViolacionPestanaDetectadaEvent.cs
│   │   ├── Results/
│   │   │   ├── ResultadoEvaluacion.cs       (Aggregate Root)
│   │   │   ├── PuntuacionPregunta.cs          (Entity)
│   │   │   └── Events/
│   │   │       └── RevisionCompletadaEvent.cs
│   │   ├── Common/
│   │   │   ├── ValueObjects/
│   │   │   │   ├── Puntuacion.cs
│   │   │   │   ├── NivelDificultad.cs
│   │   │   │   └── TipoPregunta.cs
│   │   │   ├── Abstractions/
│   │   │   │   ├── AggregateRoot.cs
│   │   │   │   ├── IDomainEvent.cs
│   │   │   │   └── IRepository.cs
│   │   │   └── Specifications/
│   │   │       └── EvaluacionActivaSpecification.cs
│   │
│   ├── TechEval.Application/
│   │   ├── Evaluations/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   ├── Sessions/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   ├── Results/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   ├── AI/
│   │   │   └── Commands/
│   │   ├── Reports/
│   │   │   └── Queries/
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   └── LoggingBehavior.cs
│   │   │   └── Interfaces/
│   │   │       ├── IServicioArchivos.cs
│   │   │       ├── IServicioEmail.cs
│   │   │       └── IServicioEvaluacionIA.cs
│   │
│   ├── TechEval.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── TechEvalDbContext.cs
│   │   │   ├── Configurations/           (EF Fluent API configs)
│   │   │   ├── Repositories/
│   │   │   └── Migrations/
│   │   ├── ExternalServices/
│   │   │   ├── AI/
│   │   │   │   └── EvaluacionIAAdapter.cs     (endpoint externo — pendiente URL del PO)
│   │   │   ├── Storage/
│   │   │   │   ├── LocalServicioArchivos.cs   (MVP: disco local)
│   │   │   │   └── CloudinaryServicioArchivos.cs (alternativa sin infra)
│   │   │   └── Email/
│   │   │       └── SendGridServicioEmail.cs
│   │   ├── Caching/
│   │   │   └── RedisSessionTimerService.cs
│   │   └── Identity/
│   │       └── JwtTokenService.cs
│   │
│   └── TechEval.API/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── EvaluationsController.cs
│       │   ├── SessionsController.cs
│       │   ├── ResultsController.cs
│       │   └── AIController.cs
│       ├── Hubs/
│       │   └── HubEvaluacion.cs
│       ├── Middleware/
│       │   └── GlobalExceptionHandlerMiddleware.cs
│       └── Program.cs
│
└── tests/
    ├── TechEval.Domain.Tests/
    ├── TechEval.Application.Tests/
    ├── TechEval.Integration.Tests/
    └── TechEval.Architecture.Tests/
```

---

## 6. Resumen de Fases y Criterio de Avance

| Fase | Objetivo | Must-have para avanzar |
|---|---|---|
| **Fase 1** | Auth + CRUD Evaluaciones | CI verde, 85% cobertura, architecture tests pasando |
| **Fase 2** | Sesión en Vivo + SignalR | CI verde, 85% cobertura, SignalR tests pasando |
| **Fase 3** | Resultados + PDF + IA | CI verde, 85% cobertura, PDF generado correctamente, IA respondiendo |
| **Fase 4** | Comparación + Enhancements | CI verde, documentación Swagger completa |

---

## 7. Decisiones Técnicas Clave (ADR — Architecture Decision Records)

| # | Decisión | Alternativa descartada | Razón |
|---|---|---|---|
| ADR-01 | PostgreSQL como DB principal | SQL Server | Open source, mejor soporte en entornos de contenedores |
| ADR-02 | MediatR para CQRS | Implementación manual | Reduce boilerplate, behaviors reutilizables |
| ADR-03 | SignalR para real-time | WebSockets puros | Abstracción más alta, fallback automático a polling |
| ADR-04 | Redis para estado de timers | DB principal | Latencia baja, TTL nativo, no ensuciar la DB con estado efímero |
| ADR-05 | **File System local** para MVP (detrás de `IServicioArchivos`) | MinIO, Azure Blob, S3 | Cero infraestructura adicional en MVP; la interfaz permite migrar sin tocar dominio |
| ADR-05b | **Cloudinary** como alternativa inmediata si se necesita CDN/multimedia avanzado | MinIO self-hosted | Free tier 25 GB, sin gestión de infraestructura |
| ADR-06 | QuestPDF para PDFs | iTextSharp / DinkToPdf | Open source, API fluent, mantenida activamente |
| ADR-07 | Mapster sobre AutoMapper | AutoMapper | Mayor rendimiento, API más expresiva en .NET moderno |
| ADR-08 | NetArchTest para architecture tests | Ninguna | Verificar que las capas de Clean Architecture se respeten en CI |

---

## 8. Infraestructura Local con Docker Compose

> **¿Se puede self-hostear Redis y MinIO? → SÍ, ambos corren en Docker con una sola línea.**

### Redis — Self-hosted (✅ Recomendado)

Redis es **completamente self-hosteable** con Docker. Es la opción recomendada para desarrollo y producción en escala pequeña/mediana:

```
# Opciones para Redis:

Ruta A — Self-hosted con Docker Compose (dev y producción propia):
  Imagen: redis:7-alpine
  Config: sin autenticación en dev, requirepass en producción
  Puerto: 6379
  Persistencia: volumen montado en /data

Ruta B — Managed sin infra (si no quieren administrar servidor):
  Upstash Redis → free tier (10,000 req/día), serverless, URL de conexión lista
  Azure Cache for Redis → si ya están en Azure
```

### MinIO — Self-hosted (✅ Disponible, ⚠️ Post-MVP)

MinIO también corre completamente en Docker. Para el MVP se **descarta** por simplicidad, pero si en el futuro deciden adoptarlo:

```
# MinIO en Docker:
  Imagen: minio/minio
  Puertos: 9000 (API S3) y 9001 (Console web)
  Requiere: volumen de disco adjunto para datos
  Compatibilidad: 100% API S3 — el código con AWSSDK.S3 funciona sin cambios
```

### docker-compose.yml para desarrollo local

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: techeval
      POSTGRES_USER: techeval
      POSTGRES_PASSWORD: techeval_dev
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    command: redis-server --appendonly yes

  # Solo habilitar si se decide usar MinIO en lugar de FileSystem:
  # minio:
  #   image: minio/minio
  #   ports:
  #     - "9000:9000"
  #     - "9001:9001"
  #   command: server /data --console-address ":9001"
  #   environment:
  #     MINIO_ROOT_USER: minioadmin
  #     MINIO_ROOT_PASSWORD: minioadmin
  #   volumes:
  #     - minio_data:/data

volumes:
  postgres_data:
  redis_data:
  # minio_data:
```

**Conclusión sobre self-hosting:**

| Servicio | ¿Self-hosteable? | Complejidad | Decisión MVP |
|---|---|---|---|
| **PostgreSQL** | ✅ Sí (Docker) | Baja | ✅ Usar |
| **Redis** | ✅ Sí (Docker) | Muy baja | ✅ Usar con Docker |
| **MinIO** | ✅ Sí (Docker) | Baja-Media | ⏳ Post-MVP |
| **File System** | ✅ Sí (nativo) | Ninguna | ✅ MVP (storage adjuntos) |

---

## FASE 5 — Plan de Acción Integral MVP v2 (2026-03-20)

> **Nota**: Esta fase redefine el alcance del MVP incorporando nuevos requerimientos del product owner.
> El nombre del producto pasa a ser **"Plataforma de Evaluaciones Técnicas"** (PET).

### 5.0 Estado Actual del MVP v1
- ✅ Auth completo (login, refresh, register, roles)
- ✅ CRUD Evaluaciones con preguntas y opciones (3 tipos)
- ✅ Sesiones: creación individual/masiva, quiz en vivo, detección de violaciones de pestaña
- ✅ Resultados: scoring automático + manual + IA, PDF, comparación, ranking
- ✅ Seeder: 7 evaluaciones temáticas, 64 preguntas, ~150 opciones
- ✅ 69/69 tests (Domain + Application + Architecture)
- ✅ Frontend Angular 21 completo (standalone, signals, DaisyUI)
- 🔲 Pendiente: checklist pre-release CRUD evaluaciones
- 🔲 Pendiente: PDF comparativo

---

### 5.1 Prioridades (orden de ejecución)

| # | Prioridad | Alcance | Esfuerzo |
|---|---|---|---|
| 1 | **Garantizar que TODO el MVP funcione** | QA end-to-end de todos los flujos | Medio |
| 2 | **Rebranding → "Plataforma de Evaluaciones Técnicas"** | Nombre, logo, títulos, meta tags | Bajo |
| 3 | **Pulir UI** (énfasis en sesiones) | UX amigable, responsive, feedback visual | Medio |
| 4 | **Tab Lock + Grabación de sesión** | Bloqueo de pestaña, grabación audio/pantalla | Alto |
| 5 | **Transcripciones (sesión + entrevista)** | Upload, evaluación IA, scoring combinado | Alto |
| 6 | **Categorías de preguntas** | Sistema de categorías, banco de preguntas | Medio |
| 7 | **Generación masiva de preguntas** | "Agregar N preguntas" aleatorias por categoría/dificultad | Medio |
| 8 | **Evaluaciones no excluyentes** (aleatorio + dificultad) | Pool de preguntas con selección dinámica | Medio |
| 9 | **Fix evaluaciones en Borrador** | Auto-activar al crear sesión, o UI para cambiar estado | Bajo |

---

### 5.2 Épica 1 — QA End-to-End del MVP

**Objetivo:** Verificar que cada flujo funcional del MVP opera correctamente de extremo a extremo.

#### Checklist de validación

**Auth:**
- [ ] Login exitoso con cada rol (admin, evaluador, candidato)
- [ ] Refresh token funciona ante 401
- [ ] Logout invalida token
- [ ] Registro de usuarios por admin

**Evaluaciones (CRUD):**
- [ ] Crear evaluación con nombre y descripción
- [ ] Agregar preguntas de los 3 tipos (TextoLibre, SeleccionUnica, SeleccionMultiple)
- [ ] Agregar opciones con puntuación a preguntas de selección
- [ ] Editar evaluación, pregunta y opción
- [ ] Eliminar pregunta y opción
- [ ] Cambiar estado de evaluación (Borrador → Activa)

**Sesiones:**
- [ ] Crear sesión individual (evaluador)
- [ ] Crear sesiones masivas
- [ ] Candidato accede con código → inicia sesión
- [ ] Quiz renderiza correctamente por tipo de pregunta
- [ ] Timer funciona con auto-envío al expirar
- [ ] Detección de violaciones de pestaña registra eventos
- [ ] Sesión se completa al responder todas las preguntas

**Resultados:**
- [ ] Scoring automático calcula puntuación real (no /100 hardcoded)
- [ ] Evaluador ve todas las sesiones y resultados
- [ ] Asignar puntaje manual funciona
- [ ] Evaluación con IA funciona (placeholder o real)
- [ ] Generar PDF funciona
- [ ] Completar revisión funciona
- [ ] Comparación multi-candidato funciona
- [ ] Ranking funciona

**UI general:**
- [ ] Navegación por rol correcta
- [ ] Tema light/dark funciona
- [ ] Paginación funciona en todas las listas
- [ ] Filtros funcionan en evaluaciones y sesiones
- [ ] Errores muestran ProblemBanner correctamente

---

### 5.3 Épica 2 — Rebranding "Plataforma de Evaluaciones Técnicas"

**Alcance:** Todo lo visual/textual que haga referencia al nombre del producto.

| # | Tarea | Ubicación |
|---|---|---|
| 2.1 | ✅ Renombrar título en `index.html` | Frontend |
| 2.2 | ✅ Actualizar título en Login page | Frontend |
| 2.3 | ✅ Actualizar header/sidebar en App Shell | Frontend |
| 2.4 | ✅ Actualizar `appsettings.json` título (Swagger/OpenAPI) | Backend |
| 2.5 | ✅ Actualizar favicon y meta tags | Frontend |
| 2.6 | ✅ Actualizar descripción en `Program.cs` (Scalar) | Backend |
| 2.7 | ✅ Actualizar nombre en Dashboard page | Frontend |

---

### 5.4 Épica 3 — Pulido de UI (énfasis en sesiones)

**Objetivo:** UI amigable, profesional y coherente. Foco en el flujo de sesiones.

| # | Tarea | Detalle |
|---|---|---|
| 3.1 | ✅ Mejorar UI de lista de sesiones | Tarjetas con estado visual claro, badges de color, info del candidato |
| 3.2 | ✅ Mejorar UI de creación de sesión | Wizard con pasos claros, feedback de éxito mejorado |
| 3.3 | ✅ Mejorar UI de quiz del candidato | Layout centrado, progreso visual, contraste de opciones |
| 3.4 | ✅ Mejorar UI de resultados | Gráficos de progreso, desglose visual por pregunta |
| 3.5 | ✅ Responsive en pantallas móviles/tablet | Sidebar colapsable, `min-h-screen`, grid responsive |
| 3.6 | ✅ Loading states consistentes | Skeleton loaders o spinners en todas las vistas |
| 3.7 | ✅ Mensajes de éxito/error unificados | Toast o banner consistente post-acción |
| 3.8 | ✅ Empty states con ilustración | Mensaje amigable + ícono cuando no hay datos |

---

### 5.5 Épica 4 — Tab Lock + Grabación de Sesión y Audio ✅

**Objetivo:** Durante una sesión activa, el candidato no puede salir de la pestaña. Se graba la sesión y opcionalmente el audio para capturar el proceso de pensamiento.

> **Completado 2026-03-20**: Todos los ítems de la épica implementados y validados. Backend builds OK, 69/69 tests pasan, frontend build OK.

#### 5.5.1 Tab Lock (Bloqueo de pestaña)

| # | Tarea | Capa |
|---|---|---|
| 4.1.1 | ✅ Interceptar `visibilitychange` y `blur` events | Frontend |
| 4.1.2 | ✅ Mostrar advertencia modal al intentar salir | Frontend |
| 4.1.3 | ✅ Incrementar contador de violaciones en backend | Backend (ya existe) |
| 4.1.4 | ✅ Bloquear `beforeunload` para evitar cierre/recarga | Frontend |
| 4.1.5 | ✅ Opción evaluador: máximo de violaciones antes de cancelar sesión | Backend Domain |

#### 5.5.2 Grabación de Sesión (Screen/Tab)

| # | Tarea | Capa |
|---|---|---|
| 4.2.1 | ✅ Captura de pantalla/pestaña con `MediaRecorder` API + `getDisplayMedia()` | Frontend |
| 4.2.2 | ✅ Grabar video como WebM/MP4 en chunks | Frontend |
| 4.2.3 | ✅ Subir grabación de sesión al completar | Frontend → Backend |
| 4.2.4 | ✅ Endpoint `POST /api/sesiones/{id}/grabacion` para recibir archivo de video | Backend |
| 4.2.5 | ✅ Almacenar grabación vía `IServicioArchivos` | Backend Infrastructure |
| 4.2.6 | ✅ Agregar campo `UrlGrabacionSesion` a `SesionEvaluacion` | Backend Domain |

#### 5.5.3 Grabación de Audio

| # | Tarea | Capa |
|---|---|---|
| 4.3.1 | ✅ Captura de micrófono con `getUserMedia({ audio: true })` | Frontend |
| 4.3.2 | ✅ Grabar audio como WebM/MP3 en paralelo a la sesión | Frontend |
| 4.3.3 | ✅ Subir archivo de audio al completar sesión | Frontend → Backend |
| 4.3.4 | ✅ Endpoint `POST /api/sesiones/{id}/audio` | Backend |
| 4.3.5 | ✅ Almacenar audio vía `IServicioArchivos` | Backend Infrastructure |
| 4.3.6 | ✅ Agregar campo `UrlGrabacionAudio` a `SesionEvaluacion` | Backend Domain |

---

### 5.6 Épica 5 — Transcripciones y Scoring Combinado

**Objetivo:** Cada evaluación tiene dos fuentes de transcripción que contribuyen al puntaje total junto con las respuestas de la sesión.

#### Modelo de scoring combinado

```
PUNTAJE TOTAL = Sesión (respuestas) + Transcripción Sesión + Transcripción Entrevista
                 ej: 3/3 (session)    + X% (IA)              + Y% (IA)

Para aprobar: >= 70% del total combinado
Máximo: 100%
```

**Transcripción de sesión (obligatoria):** Se genera del audio grabado durante la sesión. Se sube automáticamente y se envía al endpoint de IA para evaluación.

**Transcripción de entrevista (opcional):** Se sube manualmente por el evaluador desde una entrevista realizada por otro medio (Zoom, Teams, presencial). También se evalúa por IA.

#### Tareas Backend

| # | Tarea | Capa |
|---|---|---|
| 5.1 | Entidad `TranscripcionEvaluacion` con campos: tipo (Sesion/Entrevista), contenido/url, puntaje IA, estado | Domain |
| 5.2 | Agregar relación `ResultadoEvaluacion` → `List<TranscripcionEvaluacion>` | Domain |
| 5.3 | Comando `SubirTranscripcionCommand` (tipo, sesionId, archivo/texto) | Application |
| 5.4 | Comando `EvaluarTranscripcionConIACommand` → llama `IServicioEvaluacionIA` | Application |
| 5.5 | Endpoint `POST /api/resultados/{sesionId}/transcripciones` | API |
| 5.6 | Endpoint `POST /api/resultados/{sesionId}/transcripciones/{id}/evaluar-ia` | API |
| 5.7 | Modificar `CalcularPuntuacionTotal` para incluir puntajes de transcripciones | Domain |
| 5.8 | `UmbralAprobacion = 70%` como constante configurable en dominio | Domain |
| 5.9 | `ObtenerEstadoGeneral()` incluye "Aprobado" / "No aprobado" basado en umbral | Domain |

#### Tareas Frontend

| # | Tarea | Capa |
|---|---|---|
| 5.10 | UI para subir transcripción de entrevista (evaluador) | Page Resultados |
| 5.11 | Mostrar estado de transcripción de sesión (auto-generada) | Page Resultados |
| 5.12 | Botón "Evaluar transcripción con IA" | Page Resultados |
| 5.13 | Tabla de scoring combinado: Sesión (x/y) + Trans. Sesión (%) + Trans. Entrevista (%) = Total (%) | Page Resultados |
| 5.14 | Badge "Aprobado" / "No aprobado" con umbral 70% | Page Resultados + Scores |
| 5.15 | Incluir transcripciones en PDF de resultados | Backend PDF |

---

### 5.7 Épica 6 — Categorías de Preguntas

**Objetivo:** Organizar las preguntas en categorías temáticas para facilitar la gestión y la generación aleatoria.

#### Modelo de dominio

```
Categoria (AgregadoRaiz)
├── Id: Guid
├── Nombre: string (ej: "Backend .NET", "SQL", "Angular")
├── Descripcion: string?
├── CreadoPor: string
├── CreadoEn: DateTime

Pregunta
├── CategoriaId: Guid? (nullable para retrocompatibilidad)
```

#### Tareas

| # | Tarea | Capa |
|---|---|---|
| 6.1 | Crear entidad `Categoria` en Domain | Domain |
| 6.2 | Agregar `CategoriaId` nullable a `Pregunta` | Domain |
| 6.3 | Crear `IRepositorioCategoria` | Domain |
| 6.4 | CRUD: `CrearCategoriaCommand`, `ListarCategoriasQuery`, `ActualizarCategoriaCommand`, `EliminarCategoriaCommand` | Application |
| 6.5 | `CategoriasController` con endpoints CRUD | API |
| 6.6 | Migración EF para tabla `categorias` y FK en `preguntas` | Infrastructure |
| 6.7 | UI: página de gestión de categorías (CRUD) | Frontend |
| 6.8 | UI: selector de categoría al crear/editar pregunta | Frontend |
| 6.9 | Filtro por categoría en listado de preguntas | Frontend + Backend |
| 6.10 | Seeder: crear categorías iniciales y asignar a preguntas seed | Seeds |

---

### 5.8 Épica 7 — Generación Masiva de Preguntas

**Objetivo:** Permitir agregar N preguntas a una evaluación sin llenar el formulario una por una. Las preguntas se seleccionan del banco existente por categoría y dificultad.

#### Flujo

```
Evaluador en detalle de evaluación:
1. Click "Agregar preguntas del banco"
2. Selecciona: categoría(s), dificultad(es), cantidad (ej: 10)
3. Sistema selecciona N preguntas aleatorias que cumplan los filtros
4. Preguntas se agregan a la evaluación (con sus opciones)
5. Si no hay suficientes preguntas, avisa cuántas hay disponibles
```

#### Tareas

| # | Tarea | Capa |
|---|---|---|
| 7.1 | Query `ObtenerPreguntasBancoQuery` con filtros (categoría, dificultad, tipo, excluir existentes) | Application |
| 7.2 | Comando `AgregarPreguntasMasivasCommand(evaluacionId, categoriaIds[], dificultades[], cantidad)` | Application |
| 7.3 | Handler: selecciona aleatoriamente N preguntas del banco y las clona en la evaluación | Application |
| 7.4 | Endpoint `POST /api/evaluaciones/{id}/preguntas/agregar-del-banco` | API |
| 7.5 | UI: modal/panel "Agregar del banco" con filtros y cantidad | Frontend |
| 7.6 | UI: preview de preguntas seleccionadas antes de confirmar | Frontend |

---

### 5.9 Épica 8 — Evaluaciones No Excluyentes (Aleatorio + Dificultad)

**Objetivo:** Las evaluaciones seleccionan preguntas del pool de forma aleatoria y/o por dificultad, en lugar de usar siempre las mismas preguntas fijas.

#### Modelo

```
Evaluacion (cambios)
├── ModoSeleccionPreguntas: enum { Fijas, AleatoriasPorCategoria, AleatoriasPorDificultad }
├── CantidadPreguntasSesion: int? (cuántas preguntas sacar del pool en cada sesión)
├── DistribucionDificultad: { Facil: int, Medio: int, Dificil: int }? (ej: {3, 4, 3} = 10 preguntas)
```

#### Tareas

| # | Tarea | Capa |
|---|---|---|
| 8.1 | Agregar `ModoSeleccionPreguntas`, `CantidadPreguntasSesion` y `DistribucionDificultad` a `Evaluacion` | Domain |
| 8.2 | Modificar `CrearSesionCommandHandler` para seleccionar preguntas según el modo | Application |
| 8.3 | Si modo=Aleatorias: elegir N preguntas random del pool de la evaluación por categoría/dificultad | Application |
| 8.4 | Si modo=Fijas: comportamiento actual (todas las preguntas) | Application |
| 8.5 | Migración EF para nuevos campos | Infrastructure |
| 8.6 | UI: configuración del modo de selección en detalle de evaluación | Frontend |
| 8.7 | UI: configuración de distribución de dificultad | Frontend |

---

### 5.10 Épica 9 — Fix Estado Evaluaciones (Borrador → Activa)

**Objetivo:** Las evaluaciones seed y nuevas no deben quedarse en Borrador indefinidamente.

| # | Tarea | Capa |
|---|---|---|
| 9.1 | ✅ Validar al crear sesión: si evaluación está en Borrador, activarla automáticamente | Application |
| 9.2 | ✅ UI: botón visible "Activar evaluación" en detalle | Frontend |
| 9.3 | ✅ UI: indicador visual claro del estado actual con acción para cambiar | Frontend |
| 9.4 | Seeder: las evaluaciones seed ya se crean como Activas (ya implementado) | Seeds ✅ |

---

### 5.11 Orden de Ejecución por Sprint

#### Sprint A — Estabilización y Branding (Prioridad 1-2)
- [ ] QA end-to-end completo (Épica 1)
- [x] Rebranding completo (Épica 2)
- [x] Fix estado evaluaciones (Épica 9)

#### Sprint B — Pulido UI (Prioridad 3)
- [x] Mejoras UI de sesiones (Épica 3.1-3.3)
- [x] Mejoras generales UI (Épica 3.4-3.8)

#### Sprint C — Tab Lock + Grabación (Prioridad 4)
- [x] Tab Lock completo (Épica 4.1)
- [x] Grabación de sesión (Épica 4.2)
- [x] Grabación de audio (Épica 4.3)

#### Sprint D — Transcripciones + Scoring (Prioridad 5)
- [ ] Modelo de transcripciones backend (Épica 5.1-5.9)
- [ ] UI transcripciones + scoring combinado (Épica 5.10-5.15)

#### Sprint E — Categorías + Banco de Preguntas (Prioridad 6-7)
- [ ] Sistema de categorías completo (Épica 6)
- [ ] Generación masiva de preguntas (Épica 7)

#### Sprint F — Evaluaciones Dinámicas (Prioridad 8)
- [ ] Modo selección aleatorio/dificultad (Épica 8)

---

### 5.12 Definición de Hecho (DoD) — MVP v2

  - [ ] Todos los checklist de QA (Épica 1) marcados ✅
  - [x] Branding "Plataforma de Evaluaciones Técnicas" visible en toda la app
  - [x] UI pulida y responsive en flujo de sesiones
  - [x] Tab lock activo durante sesiones de candidato
  - [x] Grabación de audio funcional durante sesiones
- [ ] Transcripciones (sesión + entrevista) evaluables por IA
- [ ] Scoring combinado: respuestas + transcripción sesión + transcripción entrevista
- [ ] Umbral de aprobación 70% implementado
- [ ] Sistema de categorías de preguntas funcional
- [ ] Generación masiva de preguntas desde banco por categoría/dificultad
- [ ] Evaluaciones con selección aleatoria/por dificultad
- [x] Estado de evaluación gestionable (no se queda en Borrador)
- [x] `dotnet build` verde, `dotnet test` verde, `ng build` verde
- [ ] Repositorio Git del backend pusheado en GitHub

---

## Registro de avance

### 2025-07-11 — Sprint A + Sprint B implementados

**Sprint A — Épica 2: Rebranding**
- Renombrado el título del `index.html` a "Plataforma de Evaluaciones Técnicas" con meta description.
- Login page: kicker actualizado de "TechEval Frontend" → "Plataforma de Evaluaciones Técnicas".
- App Shell: header renombrado a "PET / Plataforma de Evaluaciones Técnicas".
- `Program.cs`: título y descripción OpenAPI/Scalar actualizados.
- Dashboard: kicker actualizado de "MVP Frontend Angular" → "Plataforma de Evaluaciones Técnicas".

**Sprint A — Épica 9: Fix Estado Evaluaciones**
- `CrearSesionCommandHandler`: auto-activa evaluación en `Borrador` al crear sesión (9.1). Persiste el cambio de estado vía `IRepositorioEvaluacion.ActualizarAsync`.
- `ActivarEvaluacionCommand` + `ActivarEvaluacionCommandHandler`: nuevo comando CQRS para activar evaluaciones explícitamente (9.2).
- `EvaluacionesController`: nuevo endpoint `PUT /api/evaluaciones/{id}/activar` (9.2).
- `EvaluacionesApiService`: nuevo método `activarEvaluacion(id)` (9.2).
- `form-detail-page`: badge de estado color-coded + botón "Activar evaluación" visible solo cuando `estado === 'Borrador'` (9.2, 9.3).

**Sprint B — Épica 3: UI Polish**
- Sessions list (3.1): badges de estado con color semántico: `badge-success` (Completada), `badge-warning` (EnProgreso), `badge-info` (NoIniciada), `badge-error` (Cancelada). Muestra `puntuacionObtenida` cuando disponible.
- Session creation (3.2): formulario multi-paso ya tenía feedback; se mantiene la UI existente de calidad.
- Candidate quiz (3.3): loading state mejorado con spinner centrado. Progreso y timer ya implementados previamente.
- Results (3.4): barra de progreso de puntuación con colores: verde ≥70%, warning 40-69%, error <40%.
- App Shell responsive (3.5): overlay móvil agregado (`mobile-overlay`) que cierra el sidebar al hacer clic.
- Loading states (3.6): ya implementados; quiz mejorado con spinner visual.
- Toast service (3.7): `ToastService` con signals + auto-dismiss 4s. `ToastComponent` standalone (DaisyUI, OnPush). Integrado en `AppShellComponent`.
- Empty states (3.8): ya implementados en todas las vistas de lista.

**Estado de builds:**
- `dotnet build`: ✅ GREEN (0 errors, 1 pre-existing warning)
- `dotnet test`: ✅ 69/69 passing
- `ng build --configuration=development`: ✅ GREEN (1.91 MB main.js)
- CodeQL: ✅ 0 alerts
