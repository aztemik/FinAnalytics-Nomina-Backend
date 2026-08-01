# FinAnalytics OS · Módulo de Nómina y Seguridad

Producto 03 · Desarrollo Web Integral · UTP · Entrega individual

Sistema de nómina con backend en ASP.NET Web API 2 (.NET Framework 4.7.2) y frontend web en JavaScript vanilla. Cuatro roles (ADMIN, RH, FINANZAS, EMPLEADO), autenticación con JWT, autorización por roles, y un motor de cálculo de nómina que integra una API externa de tipo de cambio para contratistas facturados en USD.

Este README cubre instalación y ejecución. Para el detalle de arquitectura y decisiones de diseño ver `PLAN_BACKEND.md` y `PLAN_FRONTEND.md`; para la explicación de cada archivo pensada para la evaluación oral, ver `ESTUDIO_BACKEND.md` y `ESTUDIO_FRONTEND.md`.

---

## Estado del proyecto

- **Backend:** Fases A–D completas (cimientos, seguridad, CRUDs, motor de nómina), con una excepción deliberada: **BE-30 sigue pendiente** — `ReciboDAO.GuardarRecibos` inserta sin `SqlTransaction`. Es un checkpoint del plan, aún sin aprobar. Fase E (evidencia, Swagger, dashboard) pendiente — ver `PLAN_BACKEND.md` §8.
- **Frontend:** no iniciado. `frontend/` todavía no existe; toda la Fase A–E de `PLAN_FRONTEND.md` sigue en ⬜.

Por eso, hasta que exista el frontend, la API se prueba con Thunder Client o Postman.

---

## Requisitos

- Visual Studio 2022 (Community o superior), carga de trabajo **ASP.NET y desarrollo web**
- .NET Framework 4.7.2 Developer Pack
- SQL Server Express (o LocalDB) + SQL Server Management Studio
- Thunder Client (extensión de VS Code) o Postman, para probar la API sin frontend

---

## Instalación

### 1. Base de datos

1. Abrir SSMS y conectarse al servidor local.
2. Ejecutar `bd/NominaDB.sql` completo. Crea la base `NominaDB`, sus 7 tablas, los roles y parámetros de nómina semilla, y un `INSERT` del usuario `admin` con un placeholder `<<PEGAR_HASH_BCRYPT>>` en `password_hash`.
3. Ese placeholder debe sustituirse por un hash BCrypt real antes de ejecutar la línea del `INSERT` (o ejecutarla aparte). El hash se genera con `Security/PasswordHelper.cs`; ver `PLAN_BACKEND.md` BE-06/BE-07.

### 2. Backend

1. Si no existe, copiar `FinAnalytics-Nomina/Web.config.example` a `FinAnalytics-Nomina/Web.config` y ajustar el `connectionString` (`NominaDB`) según tu instancia de SQL Server.
2. Abrir `FinAnalytics-Nomina.slnx` con Visual Studio 2022.
3. Visual Studio restaura los paquetes NuGet automáticamente al compilar (ya están en `packages/`).
4. Compilar y ejecutar con **F5** (IIS Express). La API queda disponible en:
   ```
   https://localhost:44334/api
   ```
5. **Aceptar el certificado HTTPS de localhost** la primera vez que se abre en el navegador. Si no se acepta, las peticiones desde cualquier cliente fallan en silencio (ver `PLAN_BACKEND.md` §11, gotcha 4).

### 3. Frontend

Pendiente de implementación (ver `PLAN_FRONTEND.md`). Cuando exista, se sirve con la extensión Live Server de VS Code en `http://127.0.0.1:5500` y apunta a la API mediante la constante `API_URL` en `frontend/js/app.js`.

---

## Probar la API sin frontend

Con Thunder Client o Postman:

1. `POST https://localhost:44334/api/auth/login`
   ```json
   { "username": "admin", "password": "<la contraseña que definiste al generar el hash>" }
   ```
2. Copiar el `token` de la respuesta y enviarlo en las peticiones siguientes con el header:
   ```
   Authorization: Bearer <token>
   ```

Así se probó BE-17 (login): con Thunder Client y no desde el navegador, porque sin frontend no hay quien resuelva CORS/CSP del lado cliente.

---

## Usuarios de prueba

| Usuario | Rol | Cómo se crea |
|---|---|---|
| `admin` | ADMIN | Ya sembrado por `bd/NominaDB.sql` (ver Instalación §1) |
| `rh` | RH | `POST /api/usuarios` autenticado como `admin` |
| `finanzas` | FINANZAS | `POST /api/usuarios` autenticado como `admin` |
| `empleado1` | EMPLEADO | `POST /api/usuarios` y luego vincular con `PUT /api/empleados/{id}` (`usuarioId`) |

Las contraseñas de `rh`, `finanzas` y `empleado1` las define quien las crea al momento del alta: no se documentan contraseñas fijas aquí, porque nunca se guardan en texto plano y el objetivo es que cada quien las conozca solo por haberlas tecleado.

---

## Contrato de la API

26 endpoints en total. Tabla completa de método, ruta y rol permitido en `PLAN_BACKEND.md` §6. Todas las respuestas usan el mismo envoltorio:

```json
{ "exito": true,  "mensaje": "Operacion exitosa", "datos": { } }
{ "exito": false, "mensaje": "Datos invalidos", "errores": ["..."] }
```

Códigos: `200` OK · `201` creado · `400` validación · `401` sin token o inválido · `403` rol/dato sin permiso · `404` no existe · `409` conflicto de estado · `503` tipo de cambio no disponible.

---

## Pendientes conocidos

- **BE-30** (checkpoint, sin aprobar): envolver `ReciboDAO.GuardarRecibos` en una `SqlTransaction`. Sin ella, si el cálculo falla a la mitad, el periodo puede quedar con recibos parciales hasta el próximo recálculo — no corrompe datos, pero no es atómico.
- **BE-31:** capturas de evidencia (200 vs 403, 400 de validación, inyección SQL fallida, fallback de tipo de cambio) para el PDF de entrega.
- **BE-33 / BE-34** (checkpoints, sin aprobar): Swagger y `GET /api/dashboard/resumen` para Finanzas.
- **Frontend completo** (FE-01 a FE-29 de `PLAN_FRONTEND.md`).
