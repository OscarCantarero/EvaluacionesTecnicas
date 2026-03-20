# Backlog Frontend - Plataforma de Evaluaciones Técnicas (Angular)

## Estado General
- Fase 1: ✅ Completada
- Fase 2: ✅ Completada
- Fase 3: ✅ Completada
- Fase 4: ✅ Completada
- Fase 5 (MVP v2): 🔲 En planificación

## Prioridad Actual (Acordada — 2026-03-20)
1. QA end-to-end de todos los flujos del MVP
2. Rebranding → "Plataforma de Evaluaciones Técnicas"
3. Pulido UI (énfasis en flujo de sesiones)
4. Tab Lock + Grabación de sesión/audio
5. Transcripciones (sesión + entrevista) con scoring combinado
6. Categorías de preguntas + generación masiva
7. Evaluaciones con selección aleatoria/por dificultad
8. Fix estado evaluaciones (Borrador → Activa)

## Reglas de Seguimiento
- Todo avance se registra en este archivo.
- No se crean archivos de resumen separados.
- La documentacion de contrato API fuente es /workspaces/codespaces-blank/api-docs-frontend.md.

---

## FASE 1 - Fundacion Frontend (Auth + Layout + Base UX)

### Objetivo
Construir base Angular para autenticacion, layout principal, navegacion y cimientos de design system.

### Definition of Done
- [x] Proyecto Angular standalone creado en repo separado
- [x] Backlog unico por fases creado
- [x] Shell principal responsive con navegacion
- [x] Rutas base iniciales (`/dashboard`, `/formularios`, `/sesiones`, `/resultados`, `/roadmap`)
- [x] Configuracion base de API y estilos globales
- [x] Flujo de autenticacion (login + refresh + logout)
- [x] Guard funcional por rol
- [x] Manejo centralizado de errores RFC 7807

### Avance Registrado (2026-03-19)
- Se creo proyecto en carpeta raiz `TechEval-Frontend`.
- Se inicializo repositorio Git independiente para frontend.
- Se agrego shell visual y paginas iniciales por fase.
- Se dejo configuracion base de endpoints backend disponibles.
- Se implemento login real con API (`/api/auth/login`) y logout (`/api/auth/logout`).
- Se agregaron interceptores HTTP de autorizacion y captura de errores RFC 7807.
- Se implementaron guards de autenticacion y de rol para rutas protegidas.
- Se implemento sistema de tema corporativo claro/oscuro con persistencia (`localStorage`) y toggle global.
- Se estandarizo la paleta visual corporativa basada en: `#bc2f36`, `#1d386e`, `#923d4e`, `#443f69`, `#af3740`.
- Se agrego modulo de Formularios y Preguntas con ruta dedicada (`/formularios`) para listar evaluaciones, crear formularios, agregar preguntas y opciones.
- Se ajusto linea visual a estilo sobrio/empresarial sin degradados en layout y botones principales.
- Se consolido uso de paleta corporativa: `#bc2f36`, `#1d386e`, `#923d4e`, `#443f69`, `#af3740`.
- Estado del sistema de componentes: **parcial** (tokens y estilos base listos; faltan componentes UI reutilizables tipo Button/Card/Input en `shared/ui`).
- Se integro Tailwind CSS + DaisyUI en Angular y se configuro tema corporativo `light/dark` con paleta oficial.
- Se creo sistema de componentes UI base en `styles.scss` via clases reutilizables: `ui-card`, `ui-card-body`, `ui-btn-primary`, `ui-btn-secondary`, `ui-btn-ghost`, `ui-input`, `ui-select`, `ui-textarea`, `ui-label`.
- Se migraron a DaisyUI las vistas base de `app shell` y `login`.
- Build validado en verde tras migracion (`ng build`).

---

## FASE 2 - Sesiones En Vivo (Integracion Backend Fase 2)

### Objetivo
Conectar frontend con sesiones y SignalR para experiencia de evaluacion en tiempo real.

### Backlog
- [x] Pantalla crear sesion (evaluador)
- [x] Pantalla candidato para iniciar con codigo
- [x] Cliente SignalR para ticks, avance de preguntas y alertas
- [x] Registro de respuestas y violaciones de pestana
- [x] Carga de adjuntos

### Avance Registrado (2026-03-19)
- Fase 2 se mantiene en progreso, pero su continuidad queda temporalmente subordinada al cierre del flujo de Formularios/Preguntas.
- Se implemento formulario funcional para crear sesion conectado a `POST /api/sesiones`.
- Se muestra `sesionId` y `codigoAcceso` al completar creacion exitosa.
- Se implemento acceso candidato por `sesionId + codigo` conectado a `POST /api/sesiones/{sesionId}/iniciar`.
- Se implemento pantalla de cuestionario para candidato con envio de respuestas a `POST /api/sesiones/{sesionId}/respuestas`.
- Se agrego cliente SignalR para recibir `TickTemporizador` y mostrar contador en UI.
- Se agrego registro automatico de violacion de pestana via `POST /api/sesiones/{sesionId}/violaciones-pestana`.
- Se implemento carga de adjuntos por pregunta cuando `permiteAdjunto` es verdadero via `POST /api/sesiones/{sesionId}/adjuntos`.
- Se mejoro la pantalla de sesiones del evaluador con listado de sesiones y acceso directo a resultados.

---

## FASE 3 - Resultados, Revision, PDF e IA

### Objetivo
Implementar flujo de revision y resultados con endpoints de resultados ya existentes.

### Backlog
- [x] Vista resultados por sesion
- [x] Asignacion de puntaje manual
- [x] Generacion de PDF
- [x] Integracion endpoint IA (actualmente placeholder backend)
- [x] Aceptar/rechazar sugerencia IA
- [x] Estados de revision en UI

### Avance Registrado (2026-03-19)
- Se implemento portal candidato autenticado en ruta `mis-puntajes`.
- Se implemento listado de sesiones de candidato via `GET /api/sesiones/candidato`.
- Se implemento vista de detalle de resultado por sesion via `GET /api/resultados/{sesionId}`.
- Se implemento panel de evaluador para asignar puntaje manual via `PATCH /api/resultados/{sesionId}/puntuaciones`.
- Se implemento accion para solicitar evaluacion IA via `POST /api/resultados/{sesionId}/ia-evaluate`.
- Se implemento accion para generar PDF de resultado via `POST /api/resultados/{sesionId}/pdf`.

### Avance Registrado (2026-03-20)
- Se rediseño el contrato del endpoint de IA con campos: `contextoEvaluador`, `escalaPuntuacion` (ej: 10), `rubrica`.
- El evaluador define el rol/contexto que la IA asume y la escala de calificacion (N/10, N/100, etc.).
- Se implemento UI de aceptar/rechazar sugerencia IA con panel de resultado (puntaje/escala, justificacion, aspectos positivos/negativos).
- Se agregaron endpoints `POST /api/resultados/{sesionId}/ia-aceptar` y `POST /api/resultados/{sesionId}/ia-rechazar`.
- Se actualizo `IServicioEvaluacionIA` con contrato `SolicitudEvaluacionIA` estructurado.
- `ServicioEvaluacionIAPlaceholder` genera respuesta simulada respetando la escala.
- Validacion: `dotnet build` 0 errores, `dotnet test` 25/25 verde, `ng build` verde.

### Incidencias Registradas (2026-03-19)
- Se detecto error `500` al crear preguntas en `POST /api/evaluaciones/{evaluacionId}/preguntas`.
- Se confirmo stacktrace con `DbUpdateConcurrencyException` en `RepositorioEvaluacion.ActualizarAsync`.
- Se documento el payload que dispara la incidencia y se valido que los enums enviados (`SeleccionMultiple`, `Dificil`) son correctos.
- Se realizaron pruebas de smoke para diferenciar errores de negocio (`422`) vs errores de infraestructura (`500`).

### Incidencias Registradas (2026-03-20)
- Se detecto que la vista de candidato (`mis-puntajes`) no mostraba puntuacion ni titulo de evaluacion.
- Causa raiz: `SesionResumenDto` del backend no incluia `evaluacionTitulo` ni `puntuacionObtenida`.
- Fix aplicado: se actualizo `SesionResumenDto` con campos `EvaluacionTitulo`, `PuntuacionObtenida`, `FechaCreacion`, `FechaFin`.
- Se inyecto `IRepositorioEvaluacion` y `IRepositorioResultadoEvaluacion` en `ListarSesionesCandidatoQueryHandler` para resolver titulos y scores.
- Se agrego `PreguntaId` a `PuntuacionPreguntaDto` del query `ObtenerResultados` (necesario para dropdowns de evaluador).
- Se agrego relacion explicita `HasMany(r => r.Puntuaciones)` con backing field en EF Configuration de `ResultadoEvaluacion`.
- Validacion: `dotnet build` 0 errores, `dotnet test` 25/25 verde, `ng build` verde.

### Avance Registrado (2026-03-20 — Fase 3 cierre + errores)
- Se implemento boton "Completar revision" en la vista de resultados del evaluador.
- Se integraron endpoints `POST /api/resultados/{sesionId}/completar-revision` para completar revision + generar PDF + enviar email.
- Se creo `CompletarRevisionResponse` en modelos de resultados.
- Se verifico que todos los SCSS de componentes son activos (no hay legacy residual para eliminar).
- Backlog de errores ERR-04/05/06 completados: 8 tests nuevos de Application + runbook de diagnóstico.
- Validacion: `dotnet build` 0 errores, `dotnet test` 33/33 verde, `ng build` verde.

---

## BACKLOG DE ERRORES Y CONFIABILIDAD

### Objetivo
Reducir errores `500` en operaciones de evaluaciones/preguntas/opciones y fortalecer trazabilidad de incidentes.

### Backlog
- [x] Diagnosticar `500` al crear pregunta y capturar stacktrace completo.
- [x] Identificar causa tecnica principal: conflicto de tracking/concurrencia en EF Core.
- [x] Registrar workaround aplicado durante la sesion para estabilizar la operacion de creacion de preguntas.
- [x] Definir solucion definitiva de persistencia para `ActualizarAsync` sin regresiones en preguntas y opciones.
- [x] Agregar pruebas de integracion para `POST preguntas` y `POST opciones` con asserts de no-concurrencia. (ERR-04/05 — 8 tests nuevos)
- [x] Estandarizar mapeo de `DbUpdateConcurrencyException` a respuesta controlada (`409` o `422`) y no `500`.
- [x] Agregar runbook de diagnostico rapido para incidentes `500` (query SQL, logs, pasos de reproduccion). (ERR-06)
- [ ] Crear checklist pre-release para CRUD de evaluaciones (preguntas/opciones) con validacion end-to-end.

### Registro de avance (2026-03-19 - cierre parcial confiabilidad)
- `RepositorioEvaluacion.ObtenerConPreguntasAsync` vuelve a tracking para comandos mutables.
- `RepositorioEvaluacion.ActualizarAsync` queda simplificado a `SaveChangesAsync` (con `Attach` defensivo solo si llega detached).
- Middleware global ahora mapea `DbUpdateConcurrencyException` a `409 Conflict` con mensaje controlado.
- Validacion tecnica ejecutada: `dotnet build` y `dotnet test` (Application.Tests) en verde.

---

## BACKLOG DE MIGRACION DAISYUI + COMPONENT SYSTEM

### Objetivo
Completar migracion visual del frontend a DaisyUI, manteniendo estilo sobrio/empresarial y consistencia de componentes reutilizables.

### Backlog
- [x] Integrar DaisyUI en build Angular.
- [x] Definir tema corporativo `light/dark` con paleta oficial (`#BD2F37`, `#1E386E`, `#963D4C`, `#32406C`, `#000`).
- [x] Definir capa de componentes UI base (`ui-*`) en estilos globales.
- [x] Migrar `App Shell` a clases DaisyUI.
- [x] Migrar `Login` a clases DaisyUI.
- [x] Migrar `Formularios` (`forms-page`, `form-create`, `form-detail`) a `ui-*` y DaisyUI.
- [x] Migrar `Sesiones` y flujo candidato (`candidate-access`, `candidate-quiz`) a `ui-*` y DaisyUI.
- [x] Migrar `Resultados` y `Dashboard` a `ui-*` y DaisyUI.
- [x] Migrar `Candidate Scores` y `Roadmap` a `ui-*` y DaisyUI.
- [x] Eliminar SCSS no usado detectado en `roadmap-page`.
- [x] Ajustar copy de pantallas a tono de producto final (sin mensajes de fases pendientes).
- [x] Eliminar SCSS legacy residual despues de una pasada final de consolidacion. (No hay legacy; todos los SCSS son component-scoped activos)

### Registro de cierre de migracion (2026-03-19)
- Compilacion validada: `ng build` en verde con DaisyUI.
- Sin degradados en estilos (`linear-gradient` / `radial-gradient` removidos).
- Sin lenguaje de avance por fases en la UI principal; copy orientado a MVP final.

---

## PENDIENTES DE INTEGRACION API (VALIDADO 2026-03-19)

### Objetivo
Cerrar las brechas entre endpoints backend disponibles y funcionalidades visibles en UI.

### Backlog
- [x] Integrar `POST /api/auth/registrar` en una pantalla de administracion de usuarios (solo rol `Administrador`).
- [x] Agregar opcion visible en navegacion para administradores hacia la gestion de usuarios.
- [x] Integrar `PUT /api/evaluaciones/{id}` en UI para editar datos generales de evaluacion.
- [x] Integrar `PUT /api/evaluaciones/{evaluacionId}/preguntas/{preguntaId}` para editar preguntas existentes.
- [x] Integrar `PUT /api/evaluaciones/{evaluacionId}/preguntas/{preguntaId}/opciones/{opcionId}` para editar opciones existentes.
- [x] Conectar flujo de refresh automatico ante `401` usando `POST /api/auth/refresh` (actualmente solo existe metodo en servicio).

### Registro de avance (2026-03-19 - integracion auth/admin)
- Se agrego pantalla `usuarios` para alta administrativa conectada a `POST /api/auth/registrar`.
- Se agrego ruta protegida por rol `Administrador` y item de navegacion contextual.
- Se habilito refresh automatico de token en interceptor con reintento de request ante `401`.
- Se integraron endpoints `PUT` de evaluacion, pregunta y opcion en la pantalla de detalle de formularios.
- Se corrigio habilitacion del boton "Crear usuario" (estado ligado a Reactive Forms en lugar de `computed` no reactivo a `form.valid`).

### Registro de avance (2026-03-19 - flujo candidato completo)
- Se corrigio error 403 en `POST /api/sesiones`: endpoint ahora permite roles `Evaluador,Administrador`.
- Se corrigio error 400 en `POST /api/auth/logout`: se envia ahora `{ refreshToken }` en el cuerpo de la peticion.
- Se agrego enlace "Iniciar cuestionario" al menu del rol `Candidato` (apuntando a `/candidato/acceso`).
- Se protegió la ruta `/candidato/acceso` con `authGuard` (requiere login previo).
- Se cambio `POST /api/sesiones/{id}/iniciar` de `[AllowAnonymous]` a `[Authorize(Roles="Candidato")]`.
- Se mapeo `InvalidOperationException` a `422 Unprocessable Entity` en el middleware global de excepciones.
- Se corrigio `Include("_preguntas")` a `Include(s => s.Preguntas)` en el repositorio de sesiones.
- Se declaro relacion `HasMany/WithOne` y backing field `_preguntas` en la configuracion EF de `SesionEvaluacion`.
- Se suprimo `PendingModelChangesWarning` para evitar crash al arrancar la API con migraciones de metadata.
- Se creo migracion `FixSesionPreguntasRelacion` y `FixNavegacionBackingField`.
- Se hizo idempotente `IniciarSesionCommand`: si la sesion ya esta `EnProgreso`, devuelve el estado actual sin error (permite recarga de pagina).
- Se corrigio campo `Id` → `PreguntaId` en `PreguntaSesionDto` del backend (causa de "Pregunta no encontrada en la sesion").
- Se actualizo `IniciarSesionCommandHandler` y `RegistrarRespuestaCommandHandler` para usar `preguntaActual.PreguntaId`.
- Se simplificaron los modelos frontend `sesiones.models.ts` para que coincidan con el contrato real del backend.
- Se reescribio `candidate-quiz-page` con timer **local** (`setInterval`) que cuenta desde `limiteTiempoSegundos` y autoenvia la respuesta como expirada al llegar a 0.
- Se elimino dependencia de SignalR para el temporizador (SignalR sigue disponible para usos futuros).
- Se corrigio bug de boton deshabilitado en `sessions-page`, `user-admin-page` y `candidate-access-page` usando patron `toSignal(form.statusChanges)`.

### Registro de avance (2026-03-20 - bugs UI y paleta)
- Se corrigio formulario "Editar opcion": faltaban campos `puntuacion` y `esRevisionManual` en el HTML (existian en el TS pero no se renderizaban).
- Se corrigio formulario "Editar pregunta": faltaban campos `limiteTiempoSegundos`, `permiteAdjunto` y `esRevisionManual` en el HTML.
- Se corrigio boton "Crear sesiones masivas" que nunca se habilitaba: `canSubmitBulk` computed usaba `form.controls.evaluacionId.value` (no reactivo); reemplazado por `toSignal(form.controls.evaluacionId.valueChanges)`.
- Se actualizo paleta corporativa a nuevos colores: `#BD2F37`, `#32406C`, `#000`, `#963D4C`, `#1E386E`.
- Tema dark actualizado con colores aclarados para contraste adecuado (primary `#4a6aaf`, accent `#c27585`, error `#e06070`).
- `ng build` en verde (536 kB).

### Registro de avance (2026-03-20 - UX sesiones y quiz)
- Resultado de sesion creada: reemplazado `alert alert-info` (sin contraste dark) por `ui-card` con borde success, grid de datos y botones "Copiar" para ID y codigo de acceso.
- Resultado masivo: misma mejora con tabla mejorada y boton copiar por fila.
- Formulario crear sesion: rediseñado con pasos numerados (1-Evaluacion, 2-Candidato/s), tarjetas `bg-base-200`, texto descriptivo y badge de seleccionados.
- Quiz por tipo de pregunta: ahora renderiza segun `tipoPregunta`:
  - `TextoLibre` → textarea
  - `SeleccionUnica` → radio buttons con borde resaltado al seleccionar
  - `SeleccionMultiple` → checkboxes con mismo estilo
- Boton "Enviar y continuar" ahora usa `canSubmitRespuesta()` computed (antes usaba `respuestaForm.valid` que no aplicaba a seleccion).
- Header del quiz: reemplazado texto plano por barra de progreso + badge de tiempo restante con color dinamico (rojo <=10s, amarillo <=30s).
- Resiliencia: agregado manejo de error en `cargarSesion` y `enviarRespuesta` con mensaje amigable y enlace para reintentar.
- `ng build` en verde (549 kB).

### Registro de avance (2026-03-20 - fix puntaje y vista candidato)
- **Backend**: `CalcularPuntuacionTotal` ya no hardcodea `PuntuacionMaxima = 100`. Ahora recibe `puntuacionMaximaEvaluacion` calculada sumando opciones reales de la evaluacion (max opcion por SeleccionUnica, suma positivas por SeleccionMultiple).
- **Backend**: Recalculo automatico de resultados legacy al consultarlos (`ObtenerResultadosQueryHandler` detecta y actualiza si max no coincide).
- **Backend**: Actualizado `AsignarPuntuacionCommand` y `EvaluarConIACommand` para recalcular con max real.
- **Backend**: 69/69 tests en verde (se actualizaron tests que usaban max=100 para pasar `100m` explicitamente).
- **Frontend**: Vista `candidate-scores` rediseñada con grid de puntaje/porcentaje, badges de estado con color dinamico, desglose por pregunta con puntuacion individual, tiempo total formateado.
- `ng build` en verde (552 kB).

---

## FASE 4 - Comparacion, Ranking y UX

### Objetivo
Implementar comparacion de candidatos, ranking, paginacion, filtros y mejoras de UX.

### Backlog
- [x] Vista comparativa multi-candidato
- [x] Ranking por evaluacion
- [x] Paginacion en listado de evaluaciones (forms-page)
- [x] Filtros (busqueda + estado) en listado de evaluaciones
- [x] Paginacion en listado de sesiones (sessions-page)
- [x] Filtros (busqueda + estado) en listado de sesiones
- [x] Asignacion masiva de evaluaciones a multiples candidatos (UI + backend)
- [x] Paginación en listado de resultados
- [ ] Exportables (PDF comparativo)

### Avance Registrado (2026-03-20 — Paginacion, filtros y bulk)
- Se creo `PaginacionDTOs.cs` con `PaginacionParams` y `ResultadoPaginado<T>` en backend Application.
- Se actualizaron `ListarEvaluacionesQuery` y `ListarSesionesCandidatoQuery` para retornar `ResultadoPaginado<T>` con filtros `Busqueda` y `Estado`.
- Se creo `CrearSesionesMasivasCommand` para asignacion masiva (backend).
- Se actualizo `EvaluacionesController` y `SesionesController` con parametros de paginacion y endpoint `POST /api/sesiones/masivas`.
- Se agregaron 4 tests de paginacion en `ListarEvaluacionesTests` (37/37 green).
- Se creo `pagination.models.ts` frontend con interfaz `ResultadoPaginado<T>`.
- Se actualizaron los servicios API frontend (`evaluaciones-api.service.ts`, `sesiones-api.service.ts`) con HttpParams para paginacion.
- Se rediseno `forms-page` con barra de busqueda, filtro por estado y controles de paginacion.
- Se rediseno `sessions-page` con paginacion, filtros, modo individual/masivo con checkboxes multi-candidato.
- Se corrigieron consumidores de `listarSesionesCandidato` en `results-page` y `candidate-scores-page` para extraer `.items` del `ResultadoPaginado`.
- Validacion: `dotnet test` 37/37, `ng build` verde.

### Avance Registrado (2026-03-20 — Comparacion y Ranking)
- Se creo `CompararCandidatosQuery` + handler en backend: compara N candidatos de una misma evaluacion con tabla comparativa por pregunta.
- Se creo `ObtenerRankingQuery` + handler: ranking ordenado por porcentaje, desempate por tiempo.
- Se agregaron endpoints `POST /api/evaluaciones/{id}/comparar` y `GET /api/evaluaciones/{id}/ranking` al controller.
- Se agregaron 6 tests nuevos (`ComparacionRankingTests`): 43/43 verde.
- Se crearon modelos frontend: `CompararCandidatosResponse`, `ObtenerRankingResponse`, `CandidatoRankingDto`, etc.
- Se agregaron metodos `compararCandidatos()` y `obtenerRanking()` al servicio `EvaluacionesApiService`.
- Se creo pagina `/comparar` con: selector de evaluacion, checkbox multi-sesion, tabla de ranking con medallas, tabla comparativa con detalle colapsable por pregunta.
- Se registro ruta `/comparar` con guard `roleGuard` para Evaluador/Administrador.
- Se agrego enlace `Comparar candidatos` al menu de navegacion.
- Validacion: `dotnet test` 43/43, `ng build` verde.

### Avance Registrado (2026-03-20 — Mejora UI/UX global)
- Se creo `LabelPipe` (`shared/pipes/label.pipe.ts`): pipe puro que convierte valores de enums (`TextoLibre`, `SeleccionUnica`, `NoIniciada`, `EnProgreso`, `PendienteRevision`, etc.) a etiquetas legibles en español con acentos.
- Se creo constante `ICONS` (`shared/icons/icons.ts`): 20+ íconos SVG inline Heroicons-style (plus, search, refresh, filter, trash, save, eye, users, chart, clipboard, play, star, home, document, logout, sun, moon, check, trophy).
- Se actualizó `app-shell`: íconos en cada enlace de navegación, íconos en toggle de tema y logout, acentos corregidos.
- Se actualizó `forms-page`: íconos en botones (crear, buscar, filtrar, refrescar, paginación), label pipe para estados, badges con colores por estado, spinner de carga, estado vacío con ícono, mejor espaciado.
- Se actualizó `form-detail-page`: íconos en secciones y botones, selects con labels legibles (tipo/nivel), label pipe en listado de preguntas con badges, acentos corregidos.
- Se actualizó `sessions-page`: íconos en modo individual/masivo, crear sesión, refrescar, filtrar, ver resultado; label pipe para estado de sesiones; spinners; estado vacío con ícono; acentos.
- Se actualizó `compare-page`: íconos en ranking/comparar/seleccionar; label pipe para estados/revisión/tipoPregunta; medallas con emoji; acentos.
- Se actualizó `results-page`: íconos en sesiones, puntaje manual, IA, PDF, completar revisión; label pipe para estados; spinners; estado vacío con ícono; acentos (revisión, evaluación, puntuación, rúbrica).
- Se actualizó `candidate-scores-page`: íconos en cabeceras; label pipe para estados; spinners; estado vacío; acentos (evaluación, revisión, sesión).
- Se actualizó `dashboard-page`: íconos por stat card; acentos (técnicas, revisión, últimos).
- Se aumentó espaciado general (gaps, paddings) en SCSS de todas las páginas para reducir sensación de amontonamiento.
- Validación: `ng build` verde (solo warning de budget 531kB > 500kB).

### Avance Registrado (2026-03-20 — Fix íconos y null safety)
- Se creó `SafeHtmlPipe` (`shared/pipes/safe-html.pipe.ts`): usa `DomSanitizer.bypassSecurityTrustHtml()` para renderizar SVG inline desde constantes ICONS sin que Angular los sanitize. Seguro porque los SVGs son constantes compiladas, no input de usuario.
- Se corrigió error `WARNING: sanitizing HTML stripped some content` en TODOS los templates que usaban `[innerHTML]="icons.xxx"` — ahora usan `[innerHTML]="icons.xxx | safeHtml"` (44 ocurrencias en 6 templates).
- Se importó `SafeHtmlPipe` en: app-shell, sessions-page, compare-page, results-page, candidate-scores-page, dashboard-page.
- Se corrigió `TypeError: Cannot read properties of undefined (reading 'length')` agregando `?? []` null safety en todas las asignaciones de datos de API: `res?.items ?? []`, `res?.totalPaginas ?? 0`, `candidatos ?? []` (6 componentes).
- Componentes corregidos: forms-page, sessions-page, compare-page, results-page, candidate-scores-page.
- Validación: `ng build` verde (534kB, budget warning).

### Avance Registrado (2026-03-20 — Paginación en resultados y scores)
- Se agregó paginación real (señales `pagina`, `totalPaginas`, `totalItems`) a `results-page` y `candidate-scores-page`.
- Se cambió carga de `listarSesionesCandidato(1, 100)` a `listarSesionesCandidato(pagina(), 10)` con controles de página.
- Se agregó controles Anterior/Siguiente en ambas vistas.
- Validación: `ng build` verde (535kB).

### Registro de avance (2026-03-20 — Fix evaluador no ve resultados + fix puntaje /100)
- **Backend**: `ListarSesionesCandidatoQueryHandler` ahora es role-aware: si el usuario es Evaluador o Administrador, usa `ObtenerTodasAsync` para sesiones y `ObtenerTodosAsync` para resultados; si es Candidato, filtra por `UsuarioId` como antes.
- **Backend**: Agregado `ObtenerTodasAsync` a `IRepositorioSesionEvaluacion` e implementado en `RepositorioSesionEvaluacion`.
- **Backend**: Agregado `ObtenerTodosAsync` a `IRepositorioResultadoEvaluacion` e implementado en `RepositorioResultadoEvaluacion`.
- **Backend**: Evaluador ahora ve las 7 sesiones del sistema (antes veía 0) desde el endpoint `GET /api/sesiones/candidato`.
- **Fix puntaje /100**: Verificado que la auto-recalculación funciona. Resultados de "Backend .NET" ahora muestran `2/3 (66.67%)` y `3/3 (100%)` en vez de `/100`. Evaluaciones sin opciones definidas (edge case) mantienen valor legacy por imposibilidad de cálculo.
- Validación: `dotnet build` verde, `dotnet test` 69/69 verde.

### Registro de avance (2026-03-20 — Seeder de evaluaciones)
- **Backend**: Creado `SeedEvaluaciones.cs` en `TechEval.API/Seeds/` — seeder idempotente que crea 7 evaluaciones temáticas con 64 preguntas y ~150 opciones.
- Evaluaciones seed (prefijo `[Seed]`): Backend .NET (10p), Frontend Angular (10p), Base de Datos (8p), Arquitectura Software (10p), DevOps/Cloud (8p), Seguridad (8p), Python/Data Science (10p).
- Mezcla de tipos: TextoLibre, SeleccionUnica, SeleccionMultiple con dificultades Facil/Medio/Dificil.
- Opciones con puntuaciones reales para cálculo automático de score.
- 6 evaluaciones en estado Activa, 1 en Borrador (variedad).
- Se invoca automáticamente en `Program.cs` al iniciar en modo Development.
- Validación: `dotnet build` verde, `dotnet test` 69/69 verde, seeder ejecutado con éxito.

---

## FASE 5 — MVP v2: Plataforma de Evaluaciones Técnicas (2026-03-20)

> Plan de acción completo en `/workspaces/codespaces-blank/mvp-plan.md` sección 5.

### Backlog Frontend — Sprint A (Estabilización + Branding)
- [ ] QA end-to-end: verificar todos los flujos del MVP en UI
- [ ] Rebranding: título en `index.html`, login, app-shell, dashboard → "Plataforma de Evaluaciones Técnicas"
- [ ] Actualizar favicon y meta tags
- [ ] UI: botón "Activar evaluación" en detalle (fix Borrador permanente)

### Backlog Frontend — Sprint B (Pulido UI)
- [ ] Mejorar UI lista de sesiones: tarjetas con estado visual, badges de color, info candidato
- [ ] Mejorar UI creación de sesión: wizard con pasos claros
- [ ] Mejorar UI quiz candidato: layout centrado, progreso visual
- [ ] Mejorar UI resultados: gráficos de progreso, desglose visual
- [ ] Responsive en móviles/tablet
- [ ] Loading states consistentes (skeleton loaders)
- [ ] Mensajes éxito/error unificados (toast/banner)
- [ ] Empty states con ícono descriptivo

### Backlog Frontend — Sprint C (Tab Lock + Grabación)
- [ ] Tab Lock: interceptar `visibilitychange` + `blur`, modal de advertencia, bloquear `beforeunload`
- [ ] Grabación de pantalla/pestaña con `MediaRecorder` + `getDisplayMedia()`
- [ ] Grabación de audio con `getUserMedia({ audio: true })`
- [ ] Subir grabaciones al completar sesión
- [ ] UI: indicadores de grabación activa durante quiz

### Backlog Frontend — Sprint D (Transcripciones + Scoring)
- [ ] UI: subir transcripción de entrevista (evaluador)
- [ ] UI: estado de transcripción de sesión (auto-generada)
- [ ] UI: botón "Evaluar transcripción con IA"
- [ ] UI: tabla scoring combinado (Sesión + Trans. Sesión + Trans. Entrevista = Total %)
- [ ] UI: badge "Aprobado" / "No aprobado" (umbral 70%)

### Backlog Frontend — Sprint E (Categorías + Banco de Preguntas)
- [ ] Página de gestión de categorías (CRUD)
- [ ] Selector de categoría al crear/editar pregunta
- [ ] Filtro por categoría en listado de preguntas
- [ ] Modal "Agregar del banco": filtros (categoría, dificultad, tipo) + cantidad
- [ ] Preview de preguntas seleccionadas antes de confirmar

### Backlog Frontend — Sprint F (Evaluaciones Dinámicas)
- [ ] UI: configuración modo selección (Fijas / Aleatorias por categoría / Por dificultad)
- [ ] UI: configuración distribución de dificultad (Fácil: N, Medio: N, Difícil: N)
- [ ] UI: cantidad de preguntas por sesión
