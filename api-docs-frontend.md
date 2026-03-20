# TechEval - Documentación API Phase 1 + Phase 2

**Versión:** 2.0  
**Base URL:** `http://localhost:5000` (desarrollo) o `https://api.techeval.com` (producción)  
**Formato:** JSON  
**Autenticación:** JWT Bearer Token

---

## Tabla de Contenidos

1. [Autenticación](#autenticación)
2. [Evaluaciones](#evaluaciones) (Phase 1)
3. [Preguntas](#preguntas) (Phase 1)
4. [Opciones de Respuesta](#opciones-de-respuesta) (Phase 1)
5. [Sesiones](#sesiones) (Phase 2)
6. [WebSocket - SignalR](#websocket---signalr) (Phase 2)
7. [Resultados](#resultados) (Phase 3)
8. [Códigos de Error](#códigos-de-error)
9. [Enumeraciones](#enumeraciones)

---

## Autenticación

La API utiliza **JWT (JSON Web Token)** para la autenticación. Todos los endpoints protegidos requieren un header:

```
Authorization: Bearer {access_token}
```

Los tokens tienen las siguientes características:
- **Duración:** 60 minutos (access token)
- **Refresh token:** 7 días
- **Algoritmo:** HS256

### Estructura JWT

El token contiene los siguientes claims:
- `sub`: ID del usuario
- `email`: Email del usuario
- `role`: Rol del usuario (`Administrador`, `Evaluador`, `Candidato`)
- `name`: Nombre completo del usuario

---

## Autenticación - Endpoints

### 1. Login

**Endpoint:** `POST /api/auth/login`  
**Autenticación:** No requerida  
**Roles permitidos:** Ninguno (público)

**Request:**
```json
{
  "email": "evaluador@techeval.com",
  "contraseña": "Password123!"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "550e8400-e29b-41d4-a716-446655440000",
  "expiresIn": 3600,
  "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
  "nombre": "Juan Evaluador",
  "rol": "Evaluador"
}
```

**Errores:**
- `400 Bad Request`: Email o contraseña ausentes
- `401 Unauthorized`: Email o contraseña incorrectos
- `422 Unprocessable Entity`: Usuario inactivo

---

### 2. Refresh Token

**Endpoint:** `POST /api/auth/refresh`  
**Autenticación:** No requerida  
**Roles permitidos:** Ninguno

**Request:**
```json
{
  "refreshToken": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "660e8400-e29b-41d4-a716-446655440001",
  "expiresIn": 3600
}
```

**Errores:**
- `400 Bad Request`: Refresh token ausente
- `401 Unauthorized`: Refresh token inválido o revocado
- `404 Not Found`: Refresh token no encontrado

---

### 3. Logout

**Endpoint:** `POST /api/auth/logout`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** Todos autenticados

**Request:** (vacío)

**Response (204 No Content)**

**Errores:**
- `401 Unauthorized`: Token ausente o inválido

---

### 4. Registrar Usuario

**Endpoint:** `POST /api/auth/registrar`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Administrador`

**Request:**
```json
{
  "email": "nuevo.evaluador@techeval.com",
  "nombre": "Nuevo Evaluador",
  "contraseña": "Password123!",
  "rol": "Evaluador"
}
```

**Parámetros:**
- `email`: Email único del usuario (formato válido)
- `nombre`: Nombre completo (1-100 caracteres)
- `contraseña`: Mínimo 8 caracteres, 1 mayúscula, 1 número, 1 carácter especial
- `rol`: `Administrador`, `Evaluador`, `Candidato`

**Response (201 Created):**
```json
{
  "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "nuevo.evaluador@techeval.com",
  "nombre": "Nuevo Evaluador",
  "rol": "Evaluador"
}
```

**Errores:**
- `400 Bad Request`: Validación fallida
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Usuario no tiene rol `Administrador`
- `409 Conflict`: Email ya registrado
- `422 Unprocessable Entity`: Rol no permitido

---

## Evaluaciones

### 1. Crear Evaluación

**Endpoint:** `POST /api/evaluaciones`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Request:**
```json
{
  "nombre": "Evaluación Backend .NET",
  "descripcion": "Evaluación técnica para candidatos a desarrollador backend",
  "ordenAleatorio": false,
  "ordenPorDificultad": true
}
```

**Parámetros:**
- `nombre`: Nombre único (1-200 caracteres, requerido)
- `descripcion`: Descripción de la evaluación (máx 1000 caracteres, opcional)
- `ordenAleatorio`: Mostrar preguntas en orden aleatorio (booleano, default: false)
- `ordenPorDificultad`: Mostrar preguntas ordenadas por dificultad (booleano, default: false)

**Nota:** No se pueden activar `ordenAleatorio` y `ordenPorDificultad` simultáneamente.

**Response (201 Created):**
```json
{
  "evaluacionId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Errores:**
- `400 Bad Request`: Validación fallida
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no permitido
- `409 Conflict`: Nombre de evaluación ya existe
- `422 Unprocessable Entity`: Orden inválida

---

### 2. Obtener Evaluación por ID

**Endpoint:** `GET /api/evaluaciones/{evaluacionId}`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "nombre": "Evaluación Backend .NET",
  "descripcion": "Evaluación técnica para candidatos a desarrollador backend",
  "estado": "Activa",
  "ordenAleatorio": false,
  "ordenPorDificultad": true,
  "creadoPor": "550e8400-e29b-41d4-a716-446655440001",
  "creadoEn": "2024-12-15T10:30:00Z",
  "actualizadoEn": "2024-12-15T14:15:30Z",
  "preguntas": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440002",
      "texto": "¿Qué es SOLID?",
      "tipo": "TextoLibre",
      "nivelDificultad": "Medio",
      "limiteTiempoSegundos": 300,
      "permiteAdjunto": false,
      "esRevisionManual": true,
      "orden": 1,
      "opciones": []
    }
  ]
}
```

**Errores:**
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no permitido
- `404 Not Found`: Evaluación no encontrada

---

### 3. Listar Evaluaciones

**Endpoint:** `GET /api/evaluaciones`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Query Parameters:**
- `estado` (opcional): Filtrar por estado (`Borrador`, `Activa`, `Archivada`)
- `pagina` (opcional): Número de página (default: 1)
- `tamaño` (opcional): Evaluaciones por página (default: 10)

**Response (200 OK):**
```json
{
  "total": 5,
  "pagina": 1,
  "tamaño": 10,
  "evaluaciones": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "nombre": "Evaluación Backend .NET",
      "descripcion": "Evaluación técnica para candidatos a desarrollador backend",
      "estado": "Activa",
      "cantidadPreguntas": 12,
      "creadoEn": "2024-12-15T10:30:00Z",
      "creadoPor": "550e8400-e29b-41d4-a716-446655440001"
    }
  ]
}
```

**Errores:**
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no permitido

---

### 4. Actualizar Evaluación

**Endpoint:** `PUT /api/evaluaciones/{evaluacionId}`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Request:**
```json
{
  "nombre": "Evaluación Backend .NET - v2",
  "descripcion": "Evaluación técnica actualizada"
}
```

**Response (204 No Content)**

**Errores:**
- `400 Bad Request`: Validación fallida
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no permitido
- `404 Not Found`: Evaluación no encontrada
- `409 Conflict`: Nombre ya existe

---

### 5. Eliminar Evaluación

**Endpoint:** `DELETE /api/evaluaciones/{evaluacionId}`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Response (204 No Content)**

**Errores:**
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no permitido
- `404 Not Found`: Evaluación no encontrada
- `422 Unprocessable Entity`: Evaluación tiene sesiones activas

---

## Preguntas

### 1. Agregar Pregunta a Evaluación

**Endpoint:** `POST /api/evaluaciones/{evaluacionId}/preguntas`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Request:**
```json
{
  "texto": "¿Cuál es la diferencia entre clase e interfaz?",
  "tipoPregunta": "TextoLibre",
  "nivelDificultad": "Medio",
  "limiteTiempoSegundos": 300,
  "permiteAdjunto": false,
  "esRevisionManual": true
}
```

**Parámetros:**
- `texto`: Enunciado de la pregunta (requerido)
- `tipoPregunta`: `TextoLibre`, `SeleccionUnica`, `SeleccionMultiple` (requerido)
- `nivelDificultad`: `Facil`, `Medio`, `Dificil` (requerido)
- `limiteTiempoSegundos`: Tiempo en segundos, null = sin límite (opcional)
- `permiteAdjunto`: ¿Se pueden adjuntar archivos? (default: false)
- `esRevisionManual`: ¿Requiere revisión manual? (default: false)

**Response (201 Created):**
```json
{
  "preguntaId": "660e8400-e29b-41d4-a716-446655440002"
}
```

**Errores:**
- `400 Bad Request`: Validación fallida
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no permitido
- `404 Not Found`: Evaluación no encontrada
- `422 Unprocessable Entity`: Límite de tiempo negativo

---

### 2. Actualizar Pregunta

**Endpoint:** `PUT /api/evaluaciones/{evaluacionId}/preguntas/{preguntaId}`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Request:**
```json
{
  "texto": "¿Cuál es la diferencia entre clase e interfaz en C#?",
  "tipoPregunta": "TextoLibre",
  "nivelDificultad": "Medio",
  "limiteTiempoSegundos": 600,
  "permiteAdjunto": true,
  "esRevisionManual": false
}
```

**Response (204 No Content)**

**Errores:**
- `400 Bad Request`: Validación fallida
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no permitido
- `404 Not Found`: Evaluación o pregunta no encontrada

---

### 3. Eliminar Pregunta

**Endpoint:** `DELETE /api/evaluaciones/{evaluacionId}/preguntas/{preguntaId}`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Response (204 No Content)**

**Errores:**
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no permitido
- `404 Not Found`: Evaluación o pregunta no encontrada

---

## Opciones de Respuesta

Las opciones **solo se aplican a preguntas de tipo `SeleccionUnica` o `SeleccionMultiple`**.

### 1. Agregar Opción

**Endpoint:** `POST /api/evaluaciones/{evaluacionId}/preguntas/{preguntaId}/opciones`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Request:**
```json
{
  "texto": "Es un contrato que define un conjunto de métodos que una clase debe implementar",
  "puntuacion": 10,
  "esRevisionManual": false
}
```

**Parámetros:**
- `texto`: Texto de la opción (requerido)
- `puntuacion`: Puntos por seleccionar esta opción, null = puntuación manual (optional)
- `esRevisionManual`: ¿Requiere revisión manual para calificar? (default: false)

**Nota:** Si `esRevisionManual` es true, `puntuacion` debe ser null.

**Response (201 Created):**
```json
{
  "opcionId": "770e8400-e29b-41d4-a716-446655440003"
}
```

**Errores:**
- `400 Bad Request`: Validación fallida
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no permitido
- `404 Not Found`: Evaluación o pregunta no encontrada
- `422 Unprocessable Entity`: 
  - Opción en pregunta de texto libre
  - Puntuación y revisión manual simultáneamente

---

### 2. Actualizar Opción

**Endpoint:** `PUT /api/evaluaciones/{evaluacionId}/preguntas/{preguntaId}/opciones/{opcionId}`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Request:**
```json
{
  "texto": "Es un contrato que define métodos que una clase debe implementar",
  "puntuacion": 15,
  "esRevisionManual": false
}
```

**Response (204 No Content)**

**Errores:**
- `400 Bad Request`: Validación fallida
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no permitido
- `404 Not Found`: Evaluación, pregunta u opción no encontrada

---

### 3. Eliminar Opción

**Endpoint:** `DELETE /api/evaluaciones/{evaluacionId}/preguntas/{preguntaId}/opciones/{opcionId}`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Response (204 No Content)**

**Errores:**
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no permitido
- `404 Not Found`: Evaluación, pregunta u opción no encontrada

---

## Sesiones

### Flujo de Sesión

1. **Evaluador** crea sesión con `POST /api/sesiones` y obtiene código de acceso
2. **Candidato** recibe código (vía email u otro medio)
3. **Candidato** inicia sesión con `POST /api/sesiones/{id}/iniciar` usando código
4. **Candidato** conecta a WebSocket para eventos en tiempo real
5. **Candidato** responde preguntas con `POST /api/sesiones/{id}/respuestas`
6. Se detecta cambio de pestaña con `POST /api/sesiones/{id}/violaciones-pestana`
7. **Candidato** puede adjuntar archivos con `POST /api/sesiones/{id}/adjuntos`
8. **Evaluador** puede monitorear en `GET /api/sesiones/{id}`

### 1. Crear Sesión

**Endpoint:** `POST /api/sesiones`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Request:**
```json
{
  "evaluacionId": "550e8400-e29b-41d4-a716-446655440000",
  "candidatoId": "user@candidato.com"
}
```

**Response (201 Created):**
```json
{
  "sesionId": "660e8400-e29b-41d4-a716-446655440001",
  "codigoAcceso": "ABC12345",
  "fechaCreacion": "2026-03-19T10:30:00Z",
  "mensaje": "Sesión creada exitosamente"
}
```

**Errores:**
- `400 Bad Request`: Evaluación o candidato ausentes
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no permitido (solo Evaluador)
- `404 Not Found`: Evaluación o candidato no encontrado

---

### 2. Iniciar Sesión (Candidato)

**Endpoint:** `POST /api/sesiones/{sesionId}/iniciar`  
**Autenticación:** No requerida (solo código de acceso)  
**Roles permitidos:** Ninguno (público)

**Request:**
```json
{
  "codigoAcceso": "ABC12345"
}
```

**Response (200 OK):**
```json
{
  "sesionId": "660e8400-e29b-41d4-a716-446655440001",
  "estado": "EnProgreso",
  "tiempoMaximoMinutos": 30,
  "preguntaActual": {
    "preguntaId": "770e8400-e29b-41d4-a716-446655440003",
    "numero": 1,
    "tipo": "SeleccionUnica",
    "texto": "¿Qué es un interfaz?",
    "dificultad": "Media",
    "tiempoMaximoSegundos": 60,
    "opciones": [
      { "id": "880e8400...", "texto": "Un contrato que define métodos" },
      { "id": "990e8400...", "texto": "Una clase abstracta" }
    ]
  },
  "totalPreguntas": 5,
  "preguntasRespondidas": 0,
  "conectarEnWebSocket": "wss://api.techeval.com/hubs/sesiones?sesionId={sesionId}"
}
```

**Errores:**
- `400 Bad Request`: Código de acceso ausente
- `401 Unauthorized`: Código de acceso inválido
- `404 Not Found`: Sesión no encontrada

---

### 3. Obtener Sesión

**Endpoint:** `GET /api/sesiones/{sesionId}`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`, `Candidato` (solo su sesión)

**Response (200 OK):**
```json
{
  "sesionId": "660e8400-e29b-41d4-a716-446655440001",
  "evaluacionId": "550e8400-e29b-41d4-a716-446655440000",
  "candidatoId": "user@candidato.com",
  "estado": "EnProgreso",
  "tiempoMaximoMinutos": 30,
  "fechaInicio": "2026-03-19T10:45:00Z",
  "fechaFin": null,
  "contadorViolacionesPestana": 1,
  "preguntas": [
    {
      "preguntaId": "770e8400-e29b-41d4-a716-446655440003",
      "numero": 1,
      "tipo": "SeleccionUnica",
      "texto": "¿Qué es un interfaz?",
      "dificultad": "Media",
      "tiempoMaximoSegundos": 60,
      "respuesta": {
        "texto": "Un contrato que define métodos",
        "tiempoEmpleadoSegundos": 35,
        "fechaRegistro": "2026-03-19T10:46:00Z",
        "fueExpirado": false,
        "urlAdjunto": null
      }
    },
    {
      "preguntaId": "880e8400-e29b-41d4-a716-446655440004",
      "numero": 2,
      "tipo": "TextoLibre",
      "texto": "Explica SOLID",
      "dificultad": "Difícil",
      "tiempoMaximoSegundos": 120,
      "respuesta": null
    }
  ]
}
```

**Errores:**
- `401 Unauthorized`: Token ausente o inválido
- `404 Not Found`: Sesión no encontrada

---

### 4. Registrar Respuesta

**Endpoint:** `POST /api/sesiones/{sesionId}/respuestas`  
**Autenticación:** No requerida  
**Roles permitidos:** Ninguno (solo candidatos de la sesión)

**Request:**
```json
{
  "preguntaId": "770e8400-e29b-41d4-a716-446655440003",
  "texto": "Un contrato que define métodos",
  "tiempoEmpleadoSegundos": 35,
  "fueExpirado": false
}
```

**Response (200 OK):**
```json
{
  "sesionId": "660e8400-e29b-41d4-a716-446655440001",
  "preguntaId": "770e8400-e29b-41d4-a716-446655440003",
  "sesionCompletada": false,
  "preguntaSiguiente": {
    "preguntaId": "880e8400-e29b-41d4-a716-446655440004",
    "numero": 2,
    "tipo": "TextoLibre",
    "texto": "Explica SOLID"
  },
  "preguntasRespondidas": 1,
  "totalPreguntas": 5,
  "progreso": { "respondidas": 1, "total": 5 }
}
```

**Errores:**
- `400 Bad Request`: Pregunta o respuesta ausente
- `404 Not Found`: Sesión o pregunta no encontrada
- `422 Unprocessable Entity`: Sesión ya completada

---

### 5. Registrar Violación de Pestaña

**Endpoint:** `POST /api/sesiones/{sesionId}/violaciones-pestana`  
**Autenticación:** No requerida  
**Roles permitidos:** Ninguno

**Request:** (vacío)

**Response (200 OK):**
```json
{
  "mensaje": "Violación registrada",
  "sesionId": "660e8400-e29b-41d4-a716-446655440001",
  "contadorViolaciones": 2
}
```

---

### 6. Agregar Adjunto a Respuesta

**Endpoint:** `POST /api/sesiones/{sesionId}/adjuntos`  
**Autenticación:** No requerida  
**Content-Type:** `multipart/form-data`  
**Roles permitidos:** Ninguno

**Request:**
```
POST /api/sesiones/660e8400-e29b-41d4-a716-446655440001/adjuntos
Content-Type: multipart/form-data

preguntaId=770e8400-e29b-41d4-a716-446655440003&archivo=<archivo binario>
```

**Response (200 OK):**
```json
{
  "mensaje": "Adjunto registrado",
  "urlAdjunto": "https://storage.example.com/sesiones/660e8400.../adjuntos/file123.pdf"
}
```

**Errores:**
- `400 Bad Request`: Archivo vacío o ausente
- `404 Not Found`: Sesión no encontrada

---

### 7. Listar Sesiones del Candidato

**Endpoint:** `GET /api/sesiones/candidato`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Candidato`

**Response (200 OK):**
```json
[
  {
    "sesionId": "660e8400-e29b-41d4-a716-446655440001",
    "evaluacionTitulo": "Prueba Técnica .NET",
    "estado": "Completada",
    "fechaCreacion": "2026-03-19T10:30:00Z",
    "fechaFin": "2026-03-19T11:15:00Z",
    "puntuacionObtenida": 85
  },
  {
    "sesionId": "770e8400-e29b-41d4-a716-446655440002",
    "evaluacionTitulo": "React Avanzado",
    "estado": "EnProgreso",
    "fechaCreacion": "2026-03-20T14:00:00Z",
    "fechaFin": null,
    "puntuacionObtenida": null
  }
]
```

---

## WebSocket - SignalR

### Conexión

**URL:** `wss://api.techeval.com/hubs/sesiones?sesionId={sesionId}`

**Métodos cliente (servidor invoca al cliente):**

```javascript
// Servidor emite ticks del temporizador
connection.on("TickTemporizador", (data) => {
  console.log("Segundos restantes:", data.segundosRestantes);
});

// Servidor notifica que se agotó el tiempo
connection.on("TiempoSesionAgotado", (data) => {
  console.log("¡Tiempo agotado para la sesión!", data.sesionId);
});

// Servidor informa violación detectada
connection.on("ViolacionDetectada", (data) => {
  console.log("Violación detectada en sesión", data.sesionId);
});

// Respuesta del servidor a acciones del cliente
connection.on("RespuestaRegistrada", (data) => {
  console.log("Pregunta respondida:", data.preguntaSiguiente);
});

// Notificación de errores
connection.on("Error", (data) => {
  console.error("Error:", data.mensaje);
});
```

**Métodos servidor (cliente invoca al servidor):**

```javascript
// Enviar respuesta (también se puede hacer por HTTP POST)
connection.invoke("ResponderPregunta", {
  sesionId: "...",
  preguntaId: "...",
  texto: "Mi respuesta",
  tiempoEmpleadoSegundos: 45,
  fueExpirado: false
});

// Reportar cambio de pestaña
connection.invoke("ReportarCambiosPestana", sesionId);

// Notificar que el tiempo se agotó
connection.invoke("TiempoAgotado", sesionId, preguntaId);
```

---

## Resultados

Los resultados incluyen puntuaciones automáticas (preguntas de selección), puntuaciones manuales (asignadas por evaluador) y sugerencias de IA para respuestas abiertas.

### Flujo de Resultados

1. Sesión completada → Se generan resultados automáticos para preguntas de selección
2. Evaluador revisa preguntas de texto libre manualmente
3. (Opcional) Evaluador solicita evaluación IA para respuestas complejas
4. Evaluador acepta o ajusta la puntuación final
5. Evaluador genera PDF y lo comparte con el candidato

---

### 1. Obtener Resultados de Sesión

**Endpoint:** `GET /api/resultados/{sesionId}`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`, `Candidato` (solo su sesión)

**Response (200 OK):**
```json
{
  "resultadoId": "770e8400-e29b-41d4-a716-446655440003",
  "sesionId": "660e8400-e29b-41d4-a716-446655440001",
  "nombreCandidato": "Ana García",
  "tituloEvaluacion": "Prueba Técnica .NET",
  "puntuacionTotal": 82.5,
  "puntuacionMaxima": 100.0,
  "porcentajeObtenido": 82.5,
  "estadoGeneral": "Muy Bueno",
  "violacionesPestana": 1,
  "tiempoTotalSegundos": 1800,
  "estadoRevision": "PendienteRevision",
  "puntuaciones": [
    {
      "numeroPregunta": 1,
      "tipoPregunta": "SeleccionUnica",
      "respuesta": "Un contrato que define métodos",
      "puntuacionAutomatica": 10.0,
      "puntuacionManual": null,
      "puntuacionIASugerida": null,
      "justificacionIA": null,
      "observaciones": null,
      "fueExpirada": false,
      "urlAdjunto": null,
      "tiempoEmpleadoSegundos": 35
    },
    {
      "numeroPregunta": 2,
      "tipoPregunta": "TextoLibre",
      "respuesta": "SOLID es un acrónimo que...")...",
      "puntuacionAutomatica": null,
      "puntuacionManual": 18.0,
      "puntuacionIASugerida": 17.5,
      "justificacionIA": "Respuesta sólida, cubre los 5 principios aunque podría profundizar en DIP",
      "observaciones": "Buena explicación teórica",
      "fueExpirada": false,
      "urlAdjunto": null,
      "tiempoEmpleadoSegundos": 120
    }
  ]
}
```

**Errores:**
- `401 Unauthorized`: Token ausente o inválido
- `404 Not Found`: Resultados no encontrados para la sesión

---

### 2. Generar PDF con Resultados

**Endpoint:** `POST /api/resultados/{sesionId}/pdf`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Request:** (vacío)

**Response (200 OK):**
```json
{
  "exitoGeneration": true,
  "urlPDF": "https://api.techeval.com/uploads/resultados/{evaluacionId}/resultado_candidato_sesionId.pdf",
  "mensaje": "PDF generado exitosamente"
}
```

**Formato del PDF generado:**
- Header con nombre del candidato y evaluación
- Resumen de puntuación total y porcentaje
- Estado general (Excelente / Muy Bueno / Bueno / Aceptable / Insuficiente)
- Violaciones de pestaña e indicadores de integridad
- Tabla detallada por pregunta (tipo, respuesta, tiempo, puntaje)
- Estado de la revisión y evaluador asignado
- Fecha de generación

**Errores:**
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Rol no autorizado
- `404 Not Found`: Resultados no encontrados

---

### 3. Asignar Puntuación Manual

**Endpoint:** `PATCH /api/resultados/{sesionId}/puntuaciones`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Request:**
```json
{
  "preguntaId": "880e8400-e29b-41d4-a716-446655440004",
  "puntaje": 18.0,
  "observaciones": "Buena explicación teórica, falta profundidad práctica"
}
```

**Parámetros:**
- `preguntaId`: ID de la pregunta (Guid, requerido)
- `puntaje`: Puntuación numérica asignada (requerido)
- `observaciones`: Comentario del evaluador (opcional)

**Response (200 OK):**
```json
{
  "exitoAsignacion": true,
  "puntajeTotal": 82.5,
  "porcentajeObtenido": 82.5,
  "mensaje": "Puntuación asignada exitosamente"
}
```

**Errores:**
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Solo evaluadores pueden asignar puntajes
- `404 Not Found`: Resultados o pregunta no encontrada
- `422 Unprocessable Entity`: Puntaje fuera de rango

---

### 4. Evaluar con IA (Placeholder)

> ⚠️ **Estado:** Endpoint implementado y listo, pero el servicio de IA externo aún no está integrado. Devuelve respuesta placeholder hasta que se complete la integración.

**Endpoint:** `POST /api/resultados/{sesionId}/ia-evaluate`  
**Autenticación:** Requerida (Bearer token)  
**Roles permitidos:** `Evaluador`, `Administrador`

**Request:**
```json
{
  "preguntaId": "880e8400-e29b-41d4-a716-446655440004",
  "rubrica": "Evaluar si el candidato menciona los 5 principios SOLID con ejemplos prácticos. Puntuación máxima 20 puntos. 4 puntos por principio bien explicado."
}
```

**Parámetros:**
- `preguntaId`: ID de la pregunta a evaluar (Guid, requerido)
- `rubrica`: Criterios de evaluación (opcional, string)

**Response (200 OK):**
```json
{
  "preguntaId": "880e8400-e29b-41d4-a716-446655440004",
  "puntajeSugerido": 17.5,
  "justificacion": "El candidato explica correctamente SRP, OCP, LSP y ISP...",
  "requiereRevisionManual": false,
  "estado": "PLACEHOLDER_SIN_IMPLEMENTAR",
  "nota": "Este endpoint está listo pero requiere integración con servicio de IA externo"
}
```

> **Nota para integración futura:** El endpoint de IA externo está configurado:
> ```
> POST https://e763cf74fb63edb0b282183bc3c3a7.08.environment.api.powerplatform.com:443/powerautomate/...
> ```
> Body: JSON con `respuesta` y `rubrica`. Se implementará en Phase 3 completa.

**Errores:**
- `401 Unauthorized`: Token ausente o inválido
- `403 Forbidden`: Solo evaluadores pueden solicitar evaluación de IA
- `404 Not Found`: Resultados o pregunta no encontrada

---

## Códigos de Error

### Formato de Respuesta de Error

```json
{
  "tipo": "https://api.techeval.com/errors/validation",
  "titulo": "Error de Validación",
  "estado": 422,
  "detalles": "El campo nombre es obligatorio",
  "instancia": "/api/evaluaciones",
  "folio": "req-550e8400-e29b-41d4-a716-446655440000"
}
```

### Errores de Validación (400)

| Código | Mensaje |
|--------|---------|
| `NOMBRE_REQUERIDO` | El nombre es obligatorio |
| `NOMBRE_MAXIMO_LONGITUD` | El nombre no puede superar 200 caracteres |
| `DESCRIPCION_MAXIMO_LONGITUD` | La descripción no puede superar 1000 caracteres |
| `ORDEN_CONFLICTO` | No se puede activar orden aleatorio y progresivo al mismo tiempo |
| `LIMITE_TIEMPO_INVALIDO` | El límite de tiempo debe ser mayor a cero |

### Errores de Dominio (422)

| Código | Mensaje |
|--------|---------|
| `Evaluacion.OrdenConflicto` | No se puede activar orden aleatorio y progresivo al mismo tiempo |
| `Evaluacion.OpcionEnPreguntaLibre` | Las preguntas de texto libre no pueden tener opciones de respuesta |
| `Evaluacion.PuntuacionConRevisionManual` | No se puede asignar puntaje automático y revisión manual a la misma opción |
| `Evaluacion.LimiteTiempoInvalido` | El límite de tiempo debe ser mayor a cero |

### Errores de Autenticación (401)

| Código | Mensaje |
|--------|---------|
| `CREDENCIALES_INVALIDAS` | Email o contraseña incorrectos |
| `TOKEN_INVALIDO` | Token JWT inválido o expirado |
| `TOKEN_REQUERIDO` | Header Authorization con Bearer token es obligatorio |
| `REFRESH_TOKEN_INVALIDO` | Refresh token inválido o revocado |

### Errores de Autorización (403)

| Código | Mensaje |
|--------|---------|
| `ROL_NO_AUTORIZADO` | Tu rol no tiene permisos para esta operación |

### Errores de No Encontrado (404)

| Código | Mensaje |
|--------|---------|
| `EVALUACION_NO_ENCONTRADA` | La evaluación no fue encontrada |
| `PREGUNTA_NO_ENCONTRADA` | La pregunta no pertenece a esta evaluación |
| `OPCION_NO_ENCONTRADA` | La opción de respuesta no fue encontrada |
| `USUARIO_NO_ENCONTRADO` | El usuario no fue encontrado |
| `SESION_NO_ENCONTRADA` | La sesión no fue encontrada |
| `RESULTADO_NO_ENCONTRADO` | Los resultados de la sesión no están disponibles |

### Errores de Conflicto (409)

| Código | Mensaje |
|--------|---------|
| `NOMBRE_DUPLICADO` | El nombre de la evaluación ya existe |
| `EMAIL_DUPLICADO` | El email ya está registrado |

---

## Enumeraciones

### EstadoEvaluacion

```
Borrador = 1     // Evaluación en construcción
Activa = 2       // Evaluación disponible para candidatos
Archivada = 3    // Evaluación archivada (no disponible)
```

### TipoPregunta

```
TextoLibre = 1           // Respuesta libre (sin opciones)
SeleccionUnica = 2       // Una opción correcta (radio button)
SeleccionMultiple = 3    // Múltiples opciones correctas (checkbox)
```

### NivelDificultad

```
Facil = 1     // Pregunta fácil
Medio = 2     // Pregunta de dificultad media
Dificil = 3   // Pregunta difícil
```

### Rol

```
Administrador = "Administrador"   // Acceso completo
Evaluador = "Evaluador"           // Créar/editar evaluaciones
Candidato = "Candidato"           // Responder evaluaciones
```

### EstadoRevision (Phase 3)

```
PendienteRevision = 1   // Respuestas pendientes de revisión manual
EnRevision = 2          // Actualmente en proceso de revisión
Completada = 3          // Revisión finalizada
```

### EstadoGeneral (Phase 3)

```
Excelente    >= 90%
Muy Bueno    >= 80%
Bueno        >= 70%
Aceptable    >= 60%
Insuficiente < 60%
```

---

## Notas Importantes para Frontend

### Autenticación
- Almacena el `accessToken` en un lugar seguro (localStorage NO recomendado, preferir variable de sesión o cookie HttpOnly)
- Guarda el `refreshToken` para renovar el token cuando expire
- Implementa lógica para refrescar automáticamente el token 5 minutos antes de su expiración

### Manejo de Errores
- Todos los errores documentados siguen el formato estándar RFC 7807 (Problem Details)
- El campo `folio` permite rastrear errores en los logs del servidor
- Las mensajes de validación están en español para mejor UX

### Estados de Evaluación
- Solo evaluaciones en estado **Activa** pueden ser respondidas por candidatos
- Las evaluaciones en **Borrador** pueden editarse, pero no pueden ser asignadas a candidatos
- Las evaluaciones **Archivadas** no pueden editarse

### Orden de Preguntas
- `ordenAleatorio: true` → Las preguntas se muestran en orden aleatorio en cada sesión
- `ordenPorDificultad: true` → Las preguntas se ordenan por `nivelDificultad` (Fácil → Medio → Difícil)
- Ambos son mutuamente excluyentes

### Revisión Manual
- Si una pregunta o opción tiene `esRevisionManual: true`, la calificación debe ser manual
- Se puede combinar con puntuación automática (parcialmente automáticas)
- Las respuestas con revisión manual se marcan para revisión posterior

---
