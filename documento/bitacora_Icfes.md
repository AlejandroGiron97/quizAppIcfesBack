# 📊 Proyecto ICFES - Documento de Estado y Alcance

**Fecha de actualización:** 26 de octubre de 2026 (revisado contra el código del backend)  
**Stack Tecnológico:** Flutter (Web) + .NET 10 (API REST) + SQLite (Desarrollo) + Entity Framework Core  
**Estado General:** 🟡 MVP funcional para **Demo Institucional**. Tiene pendientes **críticos de seguridad** antes de usarse con datos reales o desplegarse en producción (ver sección 3).

> El análisis técnico detallado del backend está en `documento/bitacora-analisis-backend.md` (y su versión PDF).

---

## 🎯 1. Visión del Producto

Un **pre-ICFES al estilo Duolingo**: intuitivo, con aprendizaje en pasos cortos y motivación diaria. **No se basa en lecturas aburridas**, sino en **videos** y en un **tutor IA** que explica las temáticas.

*   🛤️ **Ruta de aprendizaje:** materias divididas en temas, unidades y lecciones cortas que se desbloquean en orden.
*   🔥 **Gamificación:** experiencia (XP), racha diaria, niveles, meta diaria y reto diario.
*   🎥 **Videos** cortos por tema.
*   🤖 **Tutor IA** que explica cada tema y responde "¿por qué?" cuando el estudiante se equivoca.
*   📝 **Evaluación:** práctica con feedback inmediato y simulacro completo cronometrado.

Diseñado para escalar bajo **dos modelos de negocio**:
1.  **B2C (Particular):** Estudiantes que se registran individualmente y pagan una suscripción (Stripe/Wompi).
2.  **B2B (Institucional):** Colegios que adquieren una licencia anual. Sus estudiantes acceden mediante códigos de invitación sin pagar individualmente.

> 📌 **Estado frente a la visión:** la base (autenticación, práctica, administración, códigos y planes) está avanzada, pero la ruta de aprendizaje, la gamificación, los videos y el tutor IA **todavía no se han empezado**. El tutor IA aún no está especificado en ningún documento.

---

## ✅ 2. Lo que ya está Implementado (Core Actual)

### 2.1 Autenticación y Seguridad (Auth)
*   Login y registro con Email/Password.
*   JWT (Access Token de 15 min) + Refresh Token (14 días) con rotación.
*   Recuperación de contraseña y confirmación de correo (vía SMTP en desarrollo).
*   Cambio de contraseña autenticado (con revocación de sesiones previas).
*   Bloqueo de cuenta tras 5 intentos fallidos (15 minutos).
*   Autenticación de dos pasos (2FA) **completa** en el backend: configuración con app autenticadora (QR), activación, desactivación, verificación en el login y 10 códigos de recuperación. ⚠️ **El front todavía no la maneja:** si un usuario activa el 2FA, no podrá entrar desde la app.
*   ⚠️ Al recargar la página, el front pierde el nombre y el colegio del usuario porque `/api/auth/session` los devuelve vacíos (corrección pendiente en el backend).
*   Login con Google: el endpoint existe (`/api/auth/google`), pero **no funciona todavía** porque falta configurar el `ClientId`. Pendiente decidir si se usa.
*   Roles base: `Admin`, `Teacher`, `Student`.

### 2.2 Gestión Institucional (B2B)
*   Entidad de Instituciones (Colegios).
*   Generación de **Códigos de Invitación** (con límite de usos y fecha de expiración).
*   Registro de estudiantes mediante código: vincula automáticamente al estudiante con la institución (`SchoolId`).
*   Panel de administración para crear, listar y revocar códigos.

### 2.3 Suscripciones y Planes (B2C)
*   Catálogo de planes simulado (en COP): FREE (7 días), STANDARD (30 días), SEMESTRAL (180 días) e INSTITUTIONAL (365 días).
*   Modelo de suscripciones polimórfico (puede pertenecer a un usuario particular o a una institución).
*   Estados de suscripción: Pending, Active, Expired, Cancelled y PastDue.
*   UI: Pantalla de selección de planes y vista de "Mi Suscripción".
*   ⚠️ **Hoy las suscripciones son solo de consulta:** el backend **no bloquea** el acceso a quien no tiene un plan activo, y no hay endpoints para crear o cancelar suscripciones.
*   ⚠️ Regla "el plan FREE se oculta si el usuario ya consumió su prueba": está **solo en el front** (pantalla de planes) y revisa únicamente la suscripción más reciente. Si el usuario usó FREE y luego compró otro plan, el FREE vuelve a aparecer. Debe pasar al backend (ver roadmap).
*   Al elegir un plan de pago, el front muestra "Próximamente" (aún no hay pasarela).

### 2.4 Experiencia de Estudiante
*   Dashboard con resumen de progreso: muestra **conteos** (prácticas completas, respuestas enviadas y última materia), **no puntajes**, porque el backend no los envía en el listado (ver roadmap).
*   Práctica por materia (selección aleatoria de preguntas).
*   Feedback inmediato al responder (correcto/incorrecto + justificación).
*   Historial de prácticas ("x/y respondidas"). Una práctica a medias no se puede retomar: siempre se inicia una nueva.
*   Reto diario: por ahora toma siempre la primera materia con 10 preguntas.

### 2.5 Panel administrativo y docente (Front)
*   **Admin:** banco de preguntas, materias, instituciones, códigos de invitación y creación de profesores. No puede listar usuarios porque el backend no tiene ese endpoint.
*   **Profesor:** "Panel docente" con el banco de preguntas. **Aún no tiene reportes de estudiantes** (faltan los endpoints en el backend).

### 2.6 Infraestructura Técnica
*   Base de datos SQLite configurada. El código también admite SQL Server, pero las migraciones actuales son específicas de SQLite: pasar a PostgreSQL, MySQL o SQL Server requiere otro paquete y generar migraciones nuevas.
*   Migraciones y seeders automáticos al iniciar (materias, preguntas, usuarios de prueba, planes, códigos).
*   Contratos de API tipados y documentados con Swagger/OpenAPI (disponible **solo en entorno Development**).

---

## 🚧 3. Alcance Pendiente (Roadmap por Fases)

Las fases se hacen **en orden**, y dentro de cada fase las tareas también están en el orden recomendado. El detalle (referencias, dependencias e hitos de cierre) está en la sección 13 de `documento/bitacora-analisis-backend.md`.

**Por qué este orden:** primero se decide, luego se estabiliza lo que existe, después se construye **lo que diferencia al producto** (estilo Duolingo + videos + tutor IA) y al final los módulos de venta, que se apoyan en eso.

> 🎬 **Si hay una demo cercana:** Fase 1 completa + una materia con ruta de aprendizaje y gamificación + prototipo del tutor IA + versión simple del reporte del profesor.

### 🧭 Fase 0 — Decisiones de producto (equipo, sin código)
*   [ ] Una sola definición de **roles y permisos** (hoy el RFD, este documento y `SpecDriven_QuizApp.md` se contradicen).
*   [ ] Diseño de la **ruta de aprendizaje** (temas → unidades → lecciones, desbloqueo).
*   [ ] Reglas de **gamificación** (XP, racha, niveles, meta y reto diario).
*   [ ] Especificación del **tutor IA** (qué explica, proveedor, límite de uso por plan, costo, datos de menores, mantenerlo dentro del temario ICFES).
*   [ ] **Videos y contenido:** quién los produce, dónde se alojan y derechos de uso.
*   [ ] Decisiones pendientes del RFD (qué pasa al expirar el plan, profesor en uno o varios colegios, trial FREE por email o documento, banco global o por colegio, login con Google sí o no).

### 🚨 Fase 1 — Estabilizar y asegurar lo existente
*   [ ] Ocultar las respuestas correctas a los estudiantes en `GET /api/questions`.
*   [ ] Impedir responder dos veces la misma pregunta.
*   [ ] Revisar el token del 2FA (probablemente permite saltárselo).
*   [ ] Usuarios de prueba solo en desarrollo.
*   [ ] Sacar la clave JWT y `icfesapp.db` del repositorio.
*   [ ] `/api/auth/session` con nombre, apellido y colegio (el front pierde el nombre al recargar).
*   [ ] Enlaces de correo funcionando (front en el puerto 4200 o URL configurable).
*   [ ] Error 500 al editar una pregunta con respuestas.
*   [ ] Editar institución sin borrar su dominio; mostrar bien las prácticas abandonadas.

### 🧱 Fase 2 — Base técnica
*   [ ] Pruebas automáticas del backend (y más pruebas de contrato en el front).
*   [ ] Manejo global de errores, rate limiting y CORS por entorno.
*   [ ] Paginación en los listados.
*   [ ] Endurecer la sesión (refresh tokens con hash, detección de reutilización).
*   [ ] Códigos de error en el login en vez de comparar textos.

### 🦉 Fase 3 — Experiencia de aprendizaje estilo Duolingo
*   [ ] **Temas y subtemas** en el banco de preguntas.
*   [ ] **Carga masiva** de preguntas (CSV/Excel).
*   [ ] **Ruta de aprendizaje**: unidades y lecciones con progreso y desbloqueo (pantalla tipo "camino").
*   [ ] **Puntajes y aciertos** en el historial y por lección.
*   [ ] **Gamificación**: XP, racha, niveles, meta diaria y reto diario real.
*   [ ] **Videos por tema** dentro de las lecciones.
*   [ ] Retomar y abandonar lecciones a medias.

### 🤖 Fase 4 — Tutor IA
*   [ ] Servicio del tutor **en el backend** (la clave del proveedor nunca en la app).
*   [ ] Botón **"¿Por qué?"** que explica cada error (con caché para bajar costos).
*   [ ] El tutor **explica el tema** al iniciar una lección y responde dudas.
*   [ ] Límite de mensajes por plan, registro de costos y sin datos personales hacia el proveedor.
*   [ ] Recomendaciones de temas a reforzar.

### 🏫 Fase 5 — Producto institucional (B2B)
*   [ ] **Panel de Profesor (Reportería):** estudiantes de su colegio, resultados y progreso por materia/tema/fecha, ordenar por porcentaje. Asignar colegio al crear profesores.
*   [ ] **Bloqueo por suscripción** y pantalla de "plan vencido".
*   [ ] **Simulacro completo cronometrado** (5 materias).
*   [ ] Evolución de cada estudiante, **grupos o cursos** y **exportar reportes** (PDF/Excel).
*   [ ] Rol **InstitutionAdmin** (el rector gestiona solo su colegio) y **gestión de usuarios** para el Admin.

### 🧪 Fase 6 — Piloto con colegios reales
*   [ ] **Ley 1581:** consentimiento del acudiente, términos y privacidad (incluye el tutor IA).
*   [ ] Integridad de datos (cupo de códigos, desactivar colegios en vez de borrarlos).
*   [ ] **2FA en el front** para administradores.
*   [ ] Login con Google: configurarlo o retirarlo.
*   [ ] Entornos dev/staging/producción, CI/CD, base de datos de producción y alojamiento de videos.
*   [ ] README del front y actualización de `SpecDriven_QuizApp.md` con esta visión.

### 💳 Fase 7 — Venta a particulares (B2C)
*   [ ] Trial FREE único controlado por el backend; crear y cancelar suscripción.
*   [ ] **Pagos con Stripe** (checkout, webhooks, IVA 19 %).
*   [ ] **Wompi** (PSE, Nequi, tarjetas).
*   [ ] Configuración de planes y precios (incluido el límite del tutor IA por plan).
*   [ ] Renovación automática, avisos de vencimiento y facturación DIAN.

### 🚀 Fase 8 — Crecimiento y escala
*   [ ] Ligas o ranking anónimo (con consentimiento).
*   [ ] Notificaciones para no perder la racha.
*   [ ] App móvil con modo offline.
*   [ ] Avatar animado para el tutor IA.
*   [ ] Versionado de preguntas, limpieza técnica y evaluación de Argon2/bcrypt.

---

## 🔑 4. Credenciales de Prueba (Entorno de Desarrollo)

Para validar los diferentes flujos del sistema, utiliza las siguientes cuentas. ⚠️ **Solo para desarrollo:** hoy estas cuentas también se crearían en producción (ver Prioridad Crítica).

| Rol | Email | Password | Notas |
| :--- | :--- | :--- | :--- |
| **Super Admin** | `admin@icfesapp.com` | `Admin123!` | Acceso total al panel administrativo. |
| **Profesor** | `teacher1@test.com` | `Test12345!` | Asociado al Colegio San José. |
| **Profesor** | `teacher2@test.com` | `Test12345!` | Asociado al Liceo Moderno. |
| **Estudiante (Institucional)**| `student1@test.com` | `Test12345!` | Juan Pérez. Asociado al Colegio San José. |
| **Estudiante (Institucional)**| `student2@test.com` | `Test12345!` | Pepito Rodríguez. Asociado al Colegio San José. |
| **Estudiante (Institucional)**| `student3@test.com` | `Test12345!` | Asociado al Liceo Moderno. |
| **Estudiante (Particular)** | `student4@test.com` | `Test12345!` | Tiene un plan STANDARD activo. |
| **Estudiante (Expirado)** | `student5@test.com` | `Test12345!` | Su plan FREE ya expiró, pero **la API todavía no le bloquea el acceso**. |

**Códigos de invitación válidos (Seed):**
*   `COLEGIO-SJ-2026` (Colegio San José, 100 usos)
*   `LICEOMODERNO-2026` (Liceo Moderno, 50 usos)
*   `PRUEBA-ILIMITADO` (Colegio San José, usos ilimitados y sin expiración)

**Instituciones (Seed):** Colegio San José (con licencia activa), Liceo Moderno (con licencia activa) e I.E. La Esperanza (**sin** licencia, útil para probar una institución que no ha pagado).

---

## 🎬 5. Guion para la Demo Institucional

El enfoque de la presentación debe ser el **B2B (Institucional)**.

1.  **El problema:** "Rector, usted necesita saber cómo van a estar sus estudiantes antes del examen real".
2.  **El estudiante (Práctica):** Iniciar sesión con `student1@test.com`, mostrar una práctica y el feedback inmediato.
3.  **La institución (Control):** Iniciar sesión con `admin@icfesapp.com`, ir a **Códigos**, generar un código en vivo y explicar cómo el colegio invita a los estudiantes sin que paguen.
4.  **El valor (Reportes - Próximamente):** Mostrar el mockup o la idea del panel de profesor (la tabla de Juan 140/200, Pepito 10/200).

> ⚠️ **Evitar en la demo:** mostrar en vivo el caso de `student5@test.com` (plan vencido). La API aún no lo bloquea, así que el resultado podría no ser el esperado.

---

## ⚙️ 6. Cómo ejecutar el proyecto localmente

**Backend:**
```bash
cd src/IcfesApp.API
dotnet run
```

*   API: `http://localhost:5080` y `https://localhost:7126` (perfil `https`).
*   Swagger: `http://localhost:5080/swagger` (solo en Development).
*   Al arrancar, se aplican las migraciones y se cargan los datos de prueba automáticamente en `icfesapp.db` (SQLite).
*   Los correos usan credenciales SMTP de ejemplo: para que lleguen de verdad, hay que configurar la sección `Email` en `appsettings.Development.json` o en User Secrets.

**Frontend (Flutter)** — repositorio `QuizAppFront/quizz_application`:
```bash
flutter pub get
flutter run -d chrome --web-port 4200 --dart-define=API_BASE_URL=https://localhost:7126
```

*   `API_BASE_URL` ya vale `https://localhost:7126` por defecto, así que el `--dart-define` es opcional en local.
*   El puerto **4200** es necesario para que funcionen los enlaces de confirmación de correo y de recuperación de contraseña.
*   En desarrollo, el CORS del backend acepta cualquier puerto de `localhost`.
