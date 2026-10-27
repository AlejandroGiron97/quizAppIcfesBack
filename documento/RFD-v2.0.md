# Documento de Requerimientos — Sistema de Preparación ICFES (Web + Mobile)

| Campo | Valor |
|---|---|
| Versión | **2.0** (reemplaza a `Posible RFD.docx.pdf`, versión 1.0) |
| Fecha | 2026-10-26 |
| Stack | Flutter (Web; móvil en fase posterior) + API REST en ASP.NET Core (.NET 10) + Entity Framework Core + SQLite en desarrollo (producción por definir) |
| Repositorios | Backend: `quizAppIcfesBack` · Front: `QuizAppFront/quizz_application` |
| Documentos relacionados | `bitacora-analisis-backend.md` (análisis técnico y plan por fases) · `bitacora_Icfes.md` (estado del equipo) |

**Cómo leer este documento:** se conservan **todos los IDs de la versión 1.0** para no romper la trazabilidad, y se agregan dos columnas:

- **Estado** según el código revisado el 2026-10-26: *Hecho*, *Parcial*, *Pendiente* o *Por decidir*.
- **Fase**: la fase del plan de trabajo (sección 13 de la bitácora técnica) en la que se construye.

Los requerimientos **nuevos** de la versión 2.0 están en los módulos 3.8 a 3.11 y en la sección 4.10.

---

## 1. Descripción general

Sistema para que estudiantes se preparen para las pruebas ICFES **al estilo Duolingo**: intuitivo, con aprendizaje en pasos cortos y motivación diaria. El contenido **no se basa en lecturas extensas**, sino en **lecciones cortas, videos y un tutor IA** que explica las temáticas, complementados con práctica con retroalimentación inmediata y simulacros.

Contempla dos modelos de acceso:

- **Particular (B2C):** el estudiante se registra solo y paga un plan.
- **Institucional (B2B):** el colegio compra una licencia anual y sus estudiantes entran con un código de invitación, sin pago individual.

### 1.1 Pilares del producto

| Pilar | Descripción |
|---|---|
| Ruta de aprendizaje | Materias divididas en temas, unidades y lecciones cortas que se desbloquean en orden. |
| Gamificación | Experiencia (XP), racha diaria, niveles, meta diaria y reto diario. |
| Videos | Videos cortos por tema dentro de las lecciones. |
| Tutor IA | Explica la temática y por qué una respuesta es incorrecta, dentro del temario ICFES. |
| Evaluación | Práctica con retroalimentación inmediata y simulacro completo cronometrado. |
| Institucional | Reportes del profesor y del colegio sobre el progreso de sus estudiantes. |

---

## 2. Actores del sistema

| Rol | Descripción | Acceso | Estado en el sistema |
|---|---|---|---|
| Visitante | Usuario no autenticado | Registro, login, ver planes | Hecho |
| Estudiante | Se prepara para el ICFES | Ruta de aprendizaje, práctica, simulacros, tutor IA, resultados | Hecho (rol `Student`) |
| Profesor | Docente de un colegio o independiente | Banco de preguntas y reportes de sus estudiantes | Parcial (rol `Teacher`; sin reportes) |
| Admin Institución | Gestiona su colegio (por ejemplo, el rector) | Códigos de invitación y usuarios **de su colegio** | Pendiente (no existe el rol) |
| Admin Servidor | Soporte y gestión global | Colegios, materias, banco de preguntas, planes, usuarios y configuración | Parcial (rol `Admin`) |
| Desarrollo | Equipo técnico | Datos maestros y cuentas iniciales | Fuera de la app |

> **Pendiente de decisión (D-11):** el RFD v1.0, `bitacora_Icfes.md` y la especificación del front (`SpecDriven_QuizApp.md`) asignan permisos distintos a cada rol; por ejemplo, si el Admin crea materias y preguntas. Debe acordarse una sola tabla de permisos.

---

## 3. Requerimientos funcionales

### 3.1 Autenticación y roles

| ID | Requerimiento | Prioridad | Estado | Fase |
|---|---|---|---|---|
| RF-AUTH-01 | Registro por formulario (email, contraseña, nombre y tipo de usuario) | Alta | Hecho | — |
| RF-AUTH-02 | Inicio de sesión con email y contraseña | Alta | Hecho | — |
| RF-AUTH-03 | Recuperación de contraseña por email con token que expira (máximo 24 h) | Alta | Hecho | 1 |
| RF-AUTH-04 | Cambio de contraseña autenticado | Alta | Hecho | — |
| RF-AUTH-05 | Autenticación con JWT y refresh token con rotación | Alta | Hecho | 2 |
| RF-AUTH-06 | Cierre de sesión (invalidar el refresh token) | Alta | Hecho | — |
| RF-AUTH-07 | Registro particular: sin código, pasa al flujo de planes | Alta | Parcial | 7 |
| RF-AUTH-08 | Registro institucional con código de invitación válido | Alta | Hecho | — |
| RF-AUTH-09 | Registro de menores con autorización del acudiente (Ley 1581) | Alta | Pendiente | 6 |
| RF-AUTH-10 | 2FA opcional para Admin Institución y Admin Servidor | Media | Parcial (backend sí, front no) | 6 |
| RF-AUTH-11 | Inicio de sesión con Google | Por decidir | Parcial (endpoint sin configurar) | 0 / 6 |
| RF-AUTH-12 | **Nuevo:** al recargar la app, la sesión devuelve los datos completos del usuario (nombre y colegio) | Alta | Pendiente | 1 |

Notas: el correo de confirmación es obligatorio antes del primer inicio de sesión (Hecho). La cuenta se bloquea 15 minutos después de 5 intentos fallidos (Hecho).

### 3.2 Suscripciones

| ID | Requerimiento | Prioridad | Estado | Fase |
|---|---|---|---|---|
| RF-SUB-01 | Planes FREE (7 días), STANDARD (30 días), SEMESTRAL (180 días) e INSTITUTIONAL (365 días) | Alta | Hecho (catálogo) | — |
| RF-SUB-02 | Estados: pending, active, expired, cancelled y past_due | Alta | Hecho | — |
| RF-SUB-03 | Gestión de la suscripción visible solo si el estudiante tiene una activa | Alta | Parcial | 7 |
| RF-SUB-04 | Página de planes para particulares sin plan | Alta | Hecho | — |
| RF-SUB-05 | Al expirar: downgrade a FREE o bloqueo total | Alta | Por decidir | 0 / 5 |
| RF-SUB-06 | Cancelación desde el perfil del usuario | Alta | Pendiente | 7 |
| RF-SUB-07 | Trial FREE único por usuario (anti-abuso por email o documento) | Alta | Parcial (solo en el front y con fallas) | 7 |
| RF-SUB-08 | Renovación automática para STANDARD y SEMESTRAL | Media | Pendiente | 7 |
| RF-SUB-09 | Notificación push o por email antes del vencimiento | Media | Pendiente | 7 |
| RF-SUB-10 | **Nuevo:** sin suscripción activa (propia o del colegio) no se accede al contenido de pago | Alta | Pendiente | 5 |

### 3.3 Pagos

| ID | Requerimiento | Prioridad | Estado | Fase |
|---|---|---|---|---|
| RF-PAY-01 | Integración con Stripe (tarjetas internacionales) | Alta | Pendiente | 7 |
| RF-PAY-02 | Integración con Wompi (Colombia) | Alta | Pendiente | 7 |
| RF-PAY-03 | Webhooks para sincronizar el estado de la suscripción | Alta | Pendiente | 7 |
| RF-PAY-04 | Idempotencia en la creación de pagos (evitar cobros duplicados) | Alta | Pendiente | 7 |
| RF-PAY-05 | Página de resumen antes de pagar | Alta | Pendiente | 7 |
| RF-PAY-06 | Página de resultado del pago (exitoso o rechazado) | Alta | Pendiente | 7 |
| RF-PAY-07 | Los usuarios institucionales no pasan por pagos | Alta | Hecho (por diseño) | — |
| RF-PAY-08 | Log de auditoría de transacciones | Alta | Pendiente | 7 |
| RF-PAY-09 | Cálculo de IVA (19 %, Colombia) | Alta | Pendiente | 7 |
| RF-PAY-10 | Facturación electrónica DIAN | Media | Pendiente | 7 |
| RF-PAY-11 | Manejo de reintentos ante pagos rechazados | Media | Pendiente | 7 |
| RF-PAY-12 | Recibos enviados por email | Media | Pendiente | 7 |

### 3.4 Estudiantes

| ID | Requerimiento | Prioridad | Estado | Fase |
|---|---|---|---|---|
| RF-EST-01 | Estudiar preguntas con explicación de la respuesta | Alta | Hecho | — |
| RF-EST-02 | Simulacros cronometrados (reglas ICFES) | Alta | Pendiente | 5 |
| RF-EST-03 | Resultados de simulacros (puntaje, tiempo, aciertos) | Alta | Parcial (puntaje solo al finalizar) | 3 |
| RF-EST-04 | Historial de intentos y comparación | Alta | Parcial (sin puntajes en el historial) | 3 / 5 |
| RF-EST-05 | Analítica personal por materia, tema y evolución en el tiempo | Media | Pendiente | 5 |
| RF-EST-06 | Recomendaciones de temas a reforzar (las genera el tutor IA, ver RF-IA-04) | Media | Pendiente | 4 |
| RF-EST-07 | Contenido de estudio en videos o avatar (detallado en 3.10 y RF-IA-06) | **Alta** | Pendiente | 3 / 8 |
| RF-EST-08 | Simulacro completo (todas las áreas) | Alta | Pendiente | 5 |
| RF-EST-09 | Simulacro por materia | Media | Parcial (es la práctica actual) | — |
| RF-EST-10 | Modo offline en móvil (sincronización posterior) | Media | Pendiente | 8 |
| RF-EST-11 | Notificaciones push (recordatorios y resultados) | Media | Pendiente | 8 |
| RF-EST-12 | **Nuevo:** retomar o abandonar una práctica o lección a medias | Media | Pendiente | 3 |

Cambio respecto a v1.0: RF-EST-07 sube de Media a **Alta** porque los videos son un pilar del producto.

### 3.5 Profesores

| ID | Requerimiento | Prioridad | Estado | Fase |
|---|---|---|---|---|
| RF-PROF-01 | Buscar estudiantes por nombre, email o código | Alta | Pendiente | 5 |
| RF-PROF-02 | Ver resultados con filtros (materia, tema, fecha y grupo) | Alta | Pendiente (no existen los grupos) | 5 |
| RF-PROF-03 | Ordenar por porcentaje, de mayor a menor y viceversa | Alta | Pendiente | 5 |
| RF-PROF-04 | Ver la evolución de un estudiante en el tiempo | Media | Pendiente | 5 |
| RF-PROF-05 | Exportar reportes (PDF o Excel) | Media | Pendiente | 5 |
| RF-PROF-06 | Ver los resultados de su colegio (según permisos) | Alta | Pendiente | 5 |
| RF-PROF-07 | Ranking anónimo (opcional, con consentimiento) | Baja | Pendiente | 8 |
| RF-PROF-08 | **Nuevo:** ver el avance de sus estudiantes en la ruta de aprendizaje (lecciones, temas y racha) | Media | Pendiente | 5 |

### 3.6 Administración

| ID | Requerimiento | Prioridad | Estado | Fase |
|---|---|---|---|---|
| RF-ADM-01 | Admin Institución: generar códigos de invitación con expiración y cupo | Alta | Hecho (lo hace el Admin global) | 5 |
| RF-ADM-02 | Admin Institución: revocar códigos activos | Alta | Hecho (lo hace el Admin global) | 5 |
| RF-ADM-03 | Admin Institución: gestionar los usuarios de su colegio | Alta | Pendiente | 5 |
| RF-ADM-04 | Admin Servidor: gestión global de usuarios y roles | Alta | Parcial (solo crear profesores) | 5 |
| RF-ADM-05 | Admin Servidor: gestión del banco de preguntas | Alta | Hecho | — |
| RF-ADM-06 | Admin Servidor: soporte y tickets | Media | Pendiente | 8 |
| RF-ADM-07 | Admin Servidor: métricas del sistema (usuarios, pagos y uso, incluido el costo del tutor IA) | Media | Pendiente | 8 |
| RF-ADM-08 | Admin Servidor: configuración de planes y precios (incluido el límite del tutor IA por plan) | Media | Pendiente | 7 |
| RF-ADM-09 | **Nuevo:** gestión de colegios (crear, editar y desactivar sin borrar) | Alta | Parcial (editar borra el dominio; borrar elimina sus suscripciones) | 1 / 6 |

### 3.7 Banco de preguntas

| ID | Requerimiento | Prioridad | Estado | Fase |
|---|---|---|---|---|
| RF-QB-01 | Taxonomía: materia, tema, subtema, dificultad y año | Alta | Parcial (materia y dificultad) | 3 |
| RF-QB-02 | Versionado de preguntas | Media | Pendiente | 8 |
| RF-QB-03 | Preguntas con explicación de la respuesta correcta | Alta | Hecho | — |
| RF-QB-04 | Importación masiva (CSV o Excel) | **Alta** | Pendiente | 3 |
| RF-QB-05 | Banco compartido global o por colegio | Alta | Por decidir | 0 |

Cambio respecto a v1.0: RF-QB-04 sube de Media a **Alta**, porque una ruta de aprendizaje necesita mucho contenido.

### 3.8 Ruta de aprendizaje (nuevo)

| ID | Requerimiento | Prioridad | Estado | Fase |
|---|---|---|---|---|
| RF-APR-01 | Organizar cada materia en temas, unidades y lecciones | Alta | Pendiente | 3 |
| RF-APR-02 | Lecciones cortas (5 a 10 preguntas) con retroalimentación inmediata, reutilizando el motor de práctica actual | Alta | Pendiente | 3 |
| RF-APR-03 | Desbloqueo progresivo: la siguiente lección se habilita al completar la anterior | Alta | Pendiente | 3 |
| RF-APR-04 | Vista tipo "camino" con el progreso por lección y por tema | Alta | Pendiente | 3 |
| RF-APR-05 | Lección de repaso con las preguntas que el estudiante falló | Media | Pendiente | 3 |

### 3.9 Gamificación (nuevo)

| ID | Requerimiento | Prioridad | Estado | Fase |
|---|---|---|---|---|
| RF-GAM-01 | Experiencia (XP) por lección completada y por aciertos | Alta | Pendiente | 3 |
| RF-GAM-02 | Racha diaria (días seguidos con al menos una lección), calculada en la zona horaria America/Bogota | Alta | Pendiente | 3 |
| RF-GAM-03 | Niveles según la experiencia acumulada | Media | Pendiente | 3 |
| RF-GAM-04 | Meta diaria configurable por el estudiante | Media | Pendiente | 3 |
| RF-GAM-05 | Reto diario con una regla definida (hoy el front usa siempre la primera materia) | Media | Parcial | 3 |
| RF-GAM-06 | Ligas o ranking anónimo, con consentimiento (relacionado con RF-PROF-07) | Baja | Pendiente | 8 |
| RF-GAM-07 | Recordatorios para no perder la racha (relacionado con RF-EST-11) | Media | Pendiente | 8 |

### 3.10 Contenido de estudio en video (nuevo)

| ID | Requerimiento | Prioridad | Estado | Fase |
|---|---|---|---|---|
| RF-CON-01 | Videos cortos asociados a cada tema o lección | Alta | Pendiente | 3 |
| RF-CON-02 | Reproductor de video dentro de la lección | Alta | Pendiente | 3 |
| RF-CON-03 | Registro de los videos vistos por el estudiante | Media | Pendiente | 3 |
| RF-CON-04 | Administración de videos (agregar, editar y asociar a temas) | Media | Pendiente | 3 |

### 3.11 Tutor IA (nuevo)

| ID | Requerimiento | Prioridad | Estado | Fase |
|---|---|---|---|---|
| RF-IA-01 | Botón "¿Por qué?" en la retroalimentación, que explica por qué la respuesta es incorrecta y amplía la justificación | Alta | Pendiente | 4 |
| RF-IA-02 | Explicación de la temática al iniciar una lección | Alta | Pendiente | 4 |
| RF-IA-03 | Responder dudas del estudiante sobre el tema de la lección, sin salirse del temario ICFES | Media | Pendiente | 4 |
| RF-IA-04 | Recomendar temas a reforzar según los errores del estudiante | Media | Pendiente | 4 |
| RF-IA-05 | Límite de uso del tutor según el plan (mensajes por día) | Alta | Pendiente | 4 |
| RF-IA-06 | Avatar animado del tutor | Baja | Pendiente | 8 |

### 3.12 Páginas (frontend)

| ID | Página | Acceso | Estado | Fase |
|---|---|---|---|---|
| RF-PAG-01 | Inicio de sesión | Público | Hecho | — |
| RF-PAG-02 | Registro (con código de invitación opcional) | Público | Hecho | — |
| RF-PAG-03 | Olvido de contraseña | Público | Hecho | — |
| RF-PAG-04 | Planes y suscripciones | Particular | Hecho | — |
| RF-PAG-05 | Pago (resumen) | Particular | Pendiente | 7 |
| RF-PAG-06 | Resultado del pago (éxito o rechazo) | Particular | Pendiente | 7 |
| RF-PAG-07 | Perfil (cancelar suscripción y datos) | Autenticado | Parcial ("Mi suscripción" y cambio de contraseña) | 7 |
| RF-PAG-08 | Preguntas y estudio (videos y tutor IA) | Estudiante | Parcial (práctica sin videos ni tutor) | 3 / 4 |
| RF-PAG-09 | Simulacro | Estudiante | Pendiente | 5 |
| RF-PAG-10 | Resultados personales | Estudiante | Parcial | 3 |
| RF-PAG-11 | Reportes con filtros | Profesor | Pendiente | 5 |
| RF-PAG-12 | Gestión de la institución | Admin Institución | Pendiente | 5 |
| RF-PAG-13 | Panel de administración | Admin Servidor | Parcial | 5 |
| RF-PAG-14 | **Nuevo:** ruta de aprendizaje ("camino" de lecciones) | Estudiante | Pendiente | 3 |
| RF-PAG-15 | **Nuevo:** perfil de progreso (XP, racha, nivel y meta diaria) | Estudiante | Pendiente | 3 |
| RF-PAG-16 | **Nuevo:** plan vencido o sin acceso | Autenticado | Pendiente | 5 |

---

## 4. Requerimientos no funcionales

### 4.1 Seguridad

| ID | Requerimiento | Estado | Fase |
|---|---|---|---|
| RNF-SEG-01 | Hash de contraseñas con Argon2 o bcrypt | Parcial (PBKDF2 de ASP.NET Identity) | 8 |
| RNF-SEG-02 | JWT con expiración corta y refresh token con rotación | Hecho (15 min / 14 días) | 2 |
| RNF-SEG-03 | Rate limiting en login, registro y recuperación de contraseña | Pendiente | 2 |
| RNF-SEG-04 | HTTPS obligatorio en todos los entornos | Parcial (redirección a HTTPS) | 6 |
| RNF-SEG-05 | Validación y sanitización de todas las entradas | Parcial | 2 |
| RNF-SEG-06 | Webhooks firmados y verificados (Stripe y Wompi) | Pendiente | 7 |
| RNF-SEG-07 | 2FA disponible para roles administrativos | Parcial | 6 |
| RNF-SEG-08 | Logs de auditoría para acciones críticas (pagos y cambios de rol) | Pendiente | 7 |
| RNF-SEG-09 | Cifrado en reposo de datos sensibles | Pendiente | 6 |
| RNF-SEG-10 | Bloqueo tras N intentos fallidos de login | Hecho (5 intentos / 15 min) | — |
| RNF-SEG-11 | **Nuevo:** las respuestas correctas nunca se envían al estudiante antes de que responda | Pendiente (hoy `GET /api/questions` las expone) | 1 |
| RNF-SEG-12 | **Nuevo:** secretos (clave JWT, SMTP, proveedores de pago y de IA) fuera del repositorio | Pendiente | 1 |

### 4.2 Cumplimiento legal (Colombia)

| ID | Requerimiento | Estado | Fase |
|---|---|---|---|
| RNF-LEG-01 | Cumplimiento de la Ley 1581 de protección de datos | Pendiente | 6 |
| RNF-LEG-02 | Autorización del acudiente para el tratamiento de datos de menores | Pendiente | 6 |
| RNF-LEG-03 | Términos y condiciones publicados y aceptados en el registro | Pendiente | 6 |
| RNF-LEG-04 | Política de privacidad publicada (incluye el uso del tutor IA) | Pendiente | 6 |
| RNF-LEG-05 | Facturación electrónica DIAN | Pendiente | 7 |
| RNF-LEG-06 | Derecho al olvido: eliminación de la cuenta y sus datos | Pendiente | 6 |
| RNF-LEG-07 | Preguntas y **videos** con derechos verificados (propios o licenciados) | Pendiente | 0 |

### 4.3 Rendimiento

| ID | Requerimiento | Estado | Fase |
|---|---|---|---|
| RNF-REN-01 | Tiempo de respuesta de la API < 500 ms (percentil 95), sin contar el tutor IA | Sin medir | 6 |
| RNF-REN-02 | Carga inicial de la app < 3 s en 4G | Sin medir | 6 |
| RNF-REN-03 | Al menos 1.000 usuarios concurrentes en el MVP | Sin medir | 6 |
| RNF-REN-04 | Paginación obligatoria en listados | Pendiente | 2 |
| RNF-REN-05 | Caché para el banco de preguntas y los catálogos | Pendiente | 2 |

### 4.4 Disponibilidad y escalabilidad

| ID | Requerimiento | Estado | Fase |
|---|---|---|---|
| RNF-DIS-01 | Disponibilidad mínima de 99,5 % mensual | Pendiente | 6 |
| RNF-DIS-02 | Arquitectura escalable horizontalmente | Pendiente | 6 |
| RNF-DIS-03 | Backups automáticos diarios de la base de datos | Pendiente | 6 |
| RNF-DIS-04 | Plan de recuperación ante desastres (RTO < 4 h) | Pendiente | 6 |

### 4.5 Usabilidad

| ID | Requerimiento | Estado | Fase |
|---|---|---|---|
| RNF-USA-01 | Interfaz responsive (web) y adaptativa (móvil) | Parcial (web responsive) | — |
| RNF-USA-02 | Diseño accesible (WCAG 2.1 AA) | Sin verificar | 6 |
| RNF-USA-03 | Mensajes de error claros y en español | Hecho | — |
| RNF-USA-04 | Onboarding para nuevos usuarios | Pendiente | 3 |
| RNF-USA-05 | Consistencia visual entre web y móvil (Flutter) | Parcial | 8 |
| RNF-USA-06 | **Nuevo:** experiencia tipo Duolingo: sesiones cortas (menos de 5 minutos por lección), retroalimentación inmediata y celebración de logros | Pendiente | 3 |

### 4.6 Compatibilidad

| ID | Requerimiento | Estado | Fase |
|---|---|---|---|
| RNF-COM-01 | Web: Chrome, Firefox, Safari y Edge (últimas 2 versiones) | Sin verificar | 6 |
| RNF-COM-02 | Móvil: Android 8+ e iOS 13+ | Pendiente | 8 |
| RNF-COM-03 | Modo offline en móvil (SQLite local) | Pendiente | 8 |

### 4.7 Mantenibilidad

| ID | Requerimiento | Estado | Fase |
|---|---|---|---|
| RNF-MAN-01 | Cobertura de pruebas > 70 % | Pendiente (el front tiene 4 archivos de prueba; el backend ninguno) | 2 |
| RNF-MAN-02 | Documentación de la API (OpenAPI/Swagger) | Hecho (solo en desarrollo) | — |
| RNF-MAN-03 | Entornos separados: dev, staging y producción | Pendiente | 6 |
| RNF-MAN-04 | CI/CD automatizado | Pendiente | 6 |
| RNF-MAN-05 | Migraciones de base de datos versionadas | Hecho (5 migraciones) | — |

### 4.8 Observabilidad

| ID | Requerimiento | Estado | Fase |
|---|---|---|---|
| RNF-OBS-01 | Logs centralizados | Pendiente | 6 |
| RNF-OBS-02 | Métricas de uso (DAU, retención, conversión, rachas activas) | Pendiente | 8 |
| RNF-OBS-03 | Alertas ante fallos de pago, errores críticos o costo anormal del tutor IA | Pendiente | 6 |
| RNF-OBS-04 | Monitoreo de disponibilidad y latencia | Pendiente | 6 |

### 4.9 Internacionalización

| ID | Requerimiento | Estado | Fase |
|---|---|---|---|
| RNF-I18N-01 | Idioma principal: español (Colombia) | Hecho | — |
| RNF-I18N-02 | Arquitectura preparada para varios idiomas (futuro) | Pendiente | 8 |
| RNF-I18N-03 | Zona horaria America/Bogota (necesaria para la racha diaria) | Pendiente (hoy todo se maneja en UTC) | 3 |

### 4.10 Tutor IA (nuevo)

| ID | Requerimiento | Estado | Fase |
|---|---|---|---|
| RNF-IA-01 | El tutor se consulta **solo desde el backend**; la clave del proveedor nunca va en la app | Pendiente | 4 |
| RNF-IA-02 | No se envían datos personales del estudiante al proveedor de IA (Ley 1581) | Pendiente | 4 |
| RNF-IA-03 | Respuestas limitadas al temario ICFES, con lenguaje apropiado para menores y filtro de contenido inapropiado | Pendiente | 4 |
| RNF-IA-04 | Control de costos: caché de explicaciones por pregunta y registro de uso y costo por usuario y por colegio | Pendiente | 4 |
| RNF-IA-05 | Si el proveedor falla o tarda, se muestra la justificación escrita de la pregunta como respaldo | Pendiente | 4 |
| RNF-IA-06 | Revisión de calidad pedagógica de las explicaciones generadas (muestreo por docentes) | Pendiente | 4 |

---

## 5. Modelo de datos

### 5.1 Entidades implementadas

| Entidad (nombre en el código) | Campos principales | Diferencia con v1.0 |
|---|---|---|
| `ApplicationUser` (User) | email, nombre, apellido, `Type` (Particular/Institutional), `SchoolId`, roles de Identity | Faltan `accepted_terms` y `guardian_consent` (Ley 1581). |
| `School` (Institution) | name, domain, city, isActive | En el código se llama `School`; en la API, `institutions`. |
| `InvitationCode` | code, institutionId, maxUses, uses, expiresAt, isActive | Igual (`status` se reemplazó por `isActive`). |
| `Plan` | code, name, target, durationDays, price, currency, isActive | Se agregó `target` (Particular/Institutional). |
| `Subscription` | userId **o** institutionId, planId, status, startedAt, expiresAt, autoRenew | Puede pertenecer a un usuario o a un colegio. Falta `payment_id`. |
| `Subject` | name, description | Igual. |
| `Question` + `QuestionOption` | texto, justificación, dificultad, materia; opciones con `isCorrect` | Las opciones son una entidad aparte. Faltan `topic_id` y `version`. |
| `PracticeSession` + `StudentAnswer` | estudiante, materia, estado, fechas; respuestas con opción elegida y acierto | Equivale a `Attempt` + `Answer`. Faltan `time_spent` y `score` guardado. |
| `RefreshToken` | token, userId, expiresAt, revokedAt | No estaba en v1.0. |

### 5.2 Entidades por crear

| Entidad | Para qué | Fase |
|---|---|---|
| `Topic` | Temas y subtemas de cada materia | 3 |
| `Unit` / `Lesson` / `LessonProgress` | Ruta de aprendizaje y avance del estudiante | 3 |
| `UserStats` | XP, nivel, racha, meta diaria | 3 |
| `StudyContent` / `ContentView` | Videos por tema y registro de visualización | 3 |
| `TutorInteraction` | Uso del tutor IA, costo y límites por plan | 4 |
| `Simulacro` / `Attempt` | Simulacro completo cronometrado (o extender `PracticeSession` con un tipo) | 5 |
| `Group` | Grupos o cursos dentro de un colegio | 5 |
| `ConsentRecord` | Consentimiento del acudiente y aceptación de términos | 6 |
| `Payment` / `AuditLog` | Pagos, IVA y auditoría | 7 |

---

## 6. Fases de implementación

Las fases de la versión 1.0 se reemplazan por el plan de trabajo de la bitácora técnica (sección 13), que pone primero la estabilización y el diferenciador del producto (estilo Duolingo, videos y tutor IA) y después los módulos de venta.

| Fase | Objetivo |
|---|---|
| 0 | Decisiones de producto (sección 7) |
| 1 | Estabilizar y asegurar lo existente |
| 2 | Base técnica (pruebas, errores, paginación, rate limiting) |
| 3 | Experiencia de aprendizaje estilo Duolingo (temas, ruta, gamificación y videos) |
| 4 | Tutor IA |
| 5 | Producto institucional (reportes del profesor, simulacro y control por suscripción) |
| 6 | Piloto con colegios reales (Ley 1581 y producción) |
| 7 | Venta a particulares (suscripciones y pagos) |
| 8 | Crecimiento y escala (ligas, notificaciones, móvil offline y avatar) |

---

## 7. Decisiones pendientes

| # | Pregunta | Impacto | Estado |
|---|---|---|---|
| 1 | ¿Multi-tenant desde el inicio? | Arquitectura | Parcialmente resuelta: hoy hay separación lógica por colegio (`SchoolId`), sin aislamiento por Admin Institución. |
| 2 | ¿Banco de preguntas propio o licenciado? | Legal | Abierta |
| 3 | ¿Un profesor pertenece a uno o varios colegios? | Modelo de datos | Abierta (hoy: uno) |
| 4 | ¿Al expirar la suscripción: bloqueo total o downgrade a FREE? | UX / negocio | Abierta |
| 5 | ¿Offline en móvil desde el MVP o después? | Arquitectura móvil | Resuelta: Fase 8 |
| 6 | ¿Facturación DIAN en el MVP o después? | Legal / backend | Resuelta: Fase 7 |
| 7 | ¿Trial FREE único por email, por documento o ambos? | Anti-abuso | Abierta |
| 8 | **Nueva:** alcance del tutor IA (qué explica, chat libre o guiado), proveedor, límite por plan y presupuesto mensual | Producto / costos / legal | Abierta |
| 9 | **Nueva:** quién produce los videos y dónde se alojan (YouTube no listado, Vimeo o almacenamiento propio) | Contenido / costos | Abierta |
| 10 | **Nueva:** diseño de la ruta de aprendizaje y reglas de gamificación (XP, racha, niveles) | Producto | Abierta |
| 11 | **Nueva:** una sola tabla de roles y permisos (los documentos actuales se contradicen) | Seguridad / producto | Abierta |
| 12 | **Nueva:** ¿se mantiene el login con Google? | Producto | Abierta |
| 13 | **Nueva:** base de datos y hosting de producción | Infraestructura | Abierta |

---

## 8. Glosario

- **ICFES:** Instituto Colombiano para la Evaluación de la Educación (aplica el examen de Estado Saber 11).
- **Simulacro:** prueba cronometrada que replica las condiciones del examen real.
- **Particular (B2C):** usuario que accede de forma individual pagando un plan.
- **Institucional (B2B):** usuario vinculado a un colegio mediante un código de invitación.
- **Tenant:** institución aislada lógicamente dentro del sistema.
- **Idempotencia:** garantía de que una operación repetida no genera efectos duplicados.
- **Ruta de aprendizaje:** secuencia de temas, unidades y lecciones que el estudiante recorre en orden.
- **Lección:** bloque corto de preguntas (5 a 10) con retroalimentación inmediata.
- **XP (experiencia):** puntos que el estudiante gana al completar lecciones y acertar.
- **Racha:** cantidad de días seguidos en que el estudiante completó al menos una lección.
- **Tutor IA:** asistente basado en inteligencia artificial que explica temas y errores dentro del temario ICFES.

---

## 9. Historial de versiones

| Versión | Fecha | Cambios |
|---|---|---|
| 1.0 | Sin fecha | Versión inicial (`Posible RFD.docx.pdf`). |
| 2.0 | 2026-10-26 | Visión estilo Duolingo; estado y fase de cada requerimiento según el código; nuevos módulos de ruta de aprendizaje, gamificación, videos y tutor IA (3.8–3.11 y 4.10); nuevos RF-AUTH-12, RF-SUB-10, RF-EST-12, RF-PROF-08, RF-ADM-09, RF-PAG-14/15/16, RNF-SEG-11/12 y RNF-USA-06; RF-EST-07 y RF-QB-04 suben a prioridad Alta; Google pasa de "Omitido" a "Por decidir"; actores con su estado real; modelo de datos actualizado al código; fases alineadas con el plan de trabajo; 6 decisiones pendientes nuevas. |
