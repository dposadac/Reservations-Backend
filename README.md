# Ceiba.LiveEvent.Reservations

Backend de **reservas para eventos en vivo** construido en **.NET 10** sobre
**Clean Architecture + DDD + CQRS** con **MediatR**, validaciones con
**FluentValidation** y persistencia con **EF Core + PostgreSQL (Npgsql)** en modo
**Database First**. El dominio cubre tres áreas: **tablas maestras** (catálogos),
**eventos** y **reservas**, con sus reglas de negocio (RF/RN) encapsuladas en el dominio.

Incluye pruebas unitarias con **xUnit + Moq** y pruebas de integración de los
controladores con **Testcontainers** (PostgreSQL efímero).

## Tecnologías utilizadas

| Área              | Tecnología                                                              |
|-------------------|-------------------------------------------------------------------------|
| Runtime / lenguaje| .NET 10 / C#                                                            |
| API               | ASP.NET Core (controladores)                                            |
| Mediación / CQRS  | MediatR (comandos, consultas, eventos de dominio, pipeline behaviors)   |
| Validación        | FluentValidation                                                        |
| Persistencia      | EF Core + Npgsql (PostgreSQL), **Database First** (sin migraciones EF)  |
| Base de datos     | PostgreSQL 16                                                           |
| Documentación API | OpenAPI 3.1 nativo (`Microsoft.AspNetCore.OpenApi`) + Swagger UI        |
| Pruebas           | xUnit, Moq, Testcontainers                                              |
| Contenedores / CD | Docker, Docker Compose, GitHub Actions, Google Cloud Run + Cloud SQL    |

## Arquitectura elegida y justificación

Se eligió **Clean Architecture** combinada con **DDD** y **CQRS** porque el problema
es de **reglas de negocio** (aforos, ventanas horarias, límites de compra,
penalizaciones), no de simple CRUD. Estos patrones mantienen esas reglas en un núcleo
aislado y comprobable.

```
Api ──> Application ──> Domain
Api ──> Infrastructure ──> Application ──> Domain
```

- **Domain**: núcleo del negocio, **sin dependencias de framework**. Las raíces de
  agregado (`Event`, `Reservation`) tienen setters privados y métodos de comportamiento
  que protegen sus invariantes y lanzan `DomainException` / `BusinessRuleException`.
  Incluye *value objects* (`Email`, `ReservationCode`) y emite *eventos de dominio*.
- **Application**: orquesta los casos de uso (CQRS). Cada acción es un `Command` o
  `Query` con su `Handler`; las interfaces de repositorio (`IEventRepository`,
  `IReservationRepository`, …) se **definen aquí** (inversión de dependencias).
- **Infrastructure**: implementa los repositorios con EF Core y configura el mapeo
  Database First (`ApplicationDbContext`, `*Configuration`).
- **Api**: capa de presentación. Los controladores **solo orquestan**: validan la
  entrada y delegan en MediatR.

**Por qué este diseño:**

- **Las dependencias apuntan hacia adentro**: el dominio no conoce a EF Core ni a
  ASP.NET, por lo que las reglas se prueban sin base de datos ni servidor.
- **CQRS + MediatR** separan lectura y escritura y dejan un punto único para
  *cross-cutting concerns* mediante *pipeline behaviors* (p. ej. `ValidationBehaviour`).
- **DDD** evita modelos anémicos: la lógica vive junto a los datos que protege, lo que
  hace explícitas e inviolables las reglas de negocio.
- **Database First** encaja con un esquema versionado por SQL (catálogos maestros con
  FKs), independiente del ciclo de vida de la aplicación.

## Estructura de la solución

```
Reservations-Backend.slnx
├── db
│   ├── scripts           -> Scripts SQL Database First (esquema + seeds de catálogos).
│   ├── docker            -> initdb.sh: aplica los scripts al levantar Postgres en Docker.
│   └── README.md         -> Modelo de datos y cómo aplicar los scripts.
├── src
│   ├── Domain            -> Núcleo del dominio (DDD): Events, Reservations, Venues, Common.
│   ├── Application       -> Casos de uso (CQRS): Events, Reservations, Masters.
│   ├── Infrastructure    -> EF Core + PostgreSQL (DbContext, configuraciones, repositorios).
│   └── Api               -> Controladores ASP.NET Core + MediatR, OpenAPI, mensajes, jobs.
└── tests
    ├── Domain.Tests        -> Invariantes y reglas del dominio (xUnit).
    ├── Application.Tests   -> Handlers y validadores (xUnit + Moq).
    └── Api.IntegrationTests-> Controladores end-to-end (Testcontainers/PostgreSQL).
```

## Modelo de dominio

### Tablas maestras (catálogos, sin FKs)

| Tabla                | Descripción                                          |
|----------------------|------------------------------------------------------|
| `event_status`       | Estados de un evento (activo, cancelado, completado).|
| `event_type`         | Tipos de evento (catálogo).                          |
| `venue`              | Lugares y su capacidad.                              |
| `reservation_status` | Estados de una reserva (pendiente, confirmada, …).   |

Se exponen en una sola respuesta a través del `MasterController` para alimentar
formularios y selectores del frontend.

### Eventos (`event`)

Raíz de agregado `Event`. Encapsula la creación (RF-01) y las transiciones de estado
(RN-06). Reglas destacadas:

- **RN-01**: la capacidad del evento no puede superar la del lugar.
- **RN-03**: en fin de semana no puede iniciar después de las 22:00.
- Título 5–100 y descripción 10–500 caracteres; fechas coherentes y precio positivo.
- Estado inicial *activo*; puede **cancelarse** o pasar a **completado** cuando finaliza
  (job diario / endpoint `complete-finished`).

### Reservas (`reservation`)

Raíz de agregado `Reservation`. Encapsula creación (RF-03), confirmación de pago (RF-04)
y cancelación (RF-05). Reglas destacadas:

- **RN-04**: no se reserva si falta menos de 1 hora para el inicio.
- **RF-03**: a menos de 24 h del inicio, máximo 5 entradas por transacción.
- **RN-05**: eventos con precio > $100, máximo 10 entradas por transacción.
- **RF-04**: al confirmar el pago se asigna un **código único** con formato `EV-######`.
- **RN-07**: cancelar a menos de 48 h penaliza (las entradas se marcan como perdidas).

El detalle completo del esquema, columnas, FKs y orden de ejecución de los scripts está
en [`db/README.md`](db/README.md).

## Cómo ejecutar el proyecto localmente

### Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/) (para Postgres y para las pruebas de integración)
- Opcional: cliente `psql` si aplicas los scripts a mano

### Opción A — Todo con Docker Compose (recomendada)

Levanta la API y PostgreSQL con el esquema Database First ya aplicado:

```powershell
docker compose up --build
```

- API / Swagger: <http://localhost:8080/swagger>
- PostgreSQL: `localhost:5432` (db `ceiba_reservations`, usuario/clave `postgres`/`postgres`)

El esquema se aplica solo en el **primer** arranque (cuando el volumen está vacío). Para
reaplicarlo desde cero:

```powershell
docker compose down -v; docker compose up --build
```

### Opción B — Postgres en Docker + API con dotnet

```powershell
# 1. Levantar PostgreSQL
docker run --name ceiba-pg -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16-alpine

# 2. Crear la base y aplicar los scripts en orden (ver db/README.md)
$env:PGPASSWORD = "postgres"
psql -h localhost -U postgres -d postgres -f db/scripts/00_create_database.sql
Get-ChildItem db/scripts/*.sql | Where-Object Name -ne '00_create_database.sql' |
    Sort-Object Name |
    ForEach-Object { psql -h localhost -U postgres -d ceiba_reservations -f $_.FullName }

# 3. Compilar y ejecutar la API
dotnet build
dotnet run --project src/Api
```

La cadena de conexión se configura en `src/Api/appsettings.json` →
`ConnectionStrings:Postgres`.

### Pruebas

```powershell
dotnet test
```

Las pruebas de integración **requieren Docker**: levantan su propio contenedor
PostgreSQL efímero con Testcontainers y aplican el esquema Database First
automáticamente, por lo que no usan tu base de datos local. Los ejemplos de peticiones
están en `src/Api/Api.http`.

> Despliegue en CI/CD y Google Cloud Run: ver [`DEPLOYMENT.md`](DEPLOYMENT.md).

## Documentación de la API (Swagger / OpenAPI)

La API genera su documentación **OpenAPI 3.1** con el generador nativo de .NET 10
(`Microsoft.AspNetCore.OpenApi`) y la expone con **Swagger UI**
(`Swashbuckle.AspNetCore.SwaggerUI`). Con la API en ejecución:

| Recurso            | URL                                          |
|--------------------|----------------------------------------------|
| Swagger UI         | `http://localhost:<puerto>/swagger`          |
| Documento OpenAPI  | `http://localhost:<puerto>/openapi/v1.json`  |

### Configuración transversal, mantenible y escalable

Toda la configuración vive en `src/Api/OpenApi/`, fuera de `Program.cs`:

```
src/Api/OpenApi/
├── ApiDocumentationOptions.cs        -> Opciones bindeadas desde appsettings (título,
│                                        versión, contacto, licencia, seguridad...).
├── DocumentInfoTransformer.cs        -> Rellena la sección info del documento.
├── BearerSecuritySchemeTransformer.cs-> Declara el esquema de seguridad JWT (Authorize).
└── OpenApiExtensions.cs              -> AddApiDocumentation() / UseApiDocumentation().
```

`Program.cs` solo invoca dos métodos:

```csharp
builder.Services.AddApiDocumentation(builder.Configuration);
// ...
app.UseApiDocumentation();
```

Ventajas del diseño:

- **Mantenible**: textos, contacto, licencia y opciones se editan en
  `appsettings.json` (sección `ApiDocumentation`) sin recompilar lógica.
- **Escalable**: añadir nuevos documentos/versiones o metadatos se hace creando otro
  `IOpenApiDocumentTransformer` y registrándolo en `OpenApiExtensions`, sin tocar el
  arranque ni los controladores.
- **Rica**: los comentarios XML (`///`) de controladores y DTOs se incluyen en el
  documento (`GenerateDocumentationFile`), y se documenta el esquema **Bearer (JWT)**
  para dejar lista la autenticación.
- **Segura por defecto**: la UI solo se publica en `Development` (configurable con
  `ApiDocumentation:ExposeInProduction`).

## Endpoints

### Eventos

| Método | Ruta                              | Descripción                          |
|--------|-----------------------------------|--------------------------------------|
| GET    | `/api/event`                      | Listar eventos (filtros opcionales)  |
| GET    | `/api/event/{id}`                 | Obtener un evento por id             |
| GET    | `/api/event/{id}/occupancy`       | Reporte de ocupación (RF-06)         |
| POST   | `/api/event`                      | Crear evento (RF-01)                 |
| POST   | `/api/event/{id}/cancel`          | Cancelar evento (RN-06)              |
| POST   | `/api/event/complete-finished`    | Completar eventos finalizados (RN-06)|

### Reservas

| Método | Ruta                                | Descripción                          |
|--------|-------------------------------------|--------------------------------------|
| GET    | `/api/reservation`                  | Listar reservas (filtros opcionales) |
| POST   | `/api/reservation`                  | Reservar entradas (RF-03)            |
| POST   | `/api/reservation/{id}/confirm`     | Confirmar pago y emitir código (RF-04)|
| POST   | `/api/reservation/{id}/cancel`      | Cancelar reserva (RF-05 / RN-07)     |

### Maestras

| Método | Ruta            | Descripción                                              |
|--------|-----------------|---------------------------------------------------------|
| GET    | `/api/master`   | Tipos y estados de evento, estados de reserva y lugares |
