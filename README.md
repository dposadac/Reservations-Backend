# Ceiba.LiveEvent.Reservations

Línea base en **.NET 10** que implementa **Clean Architecture + DDD + CQRS** con
**MediatR**, validaciones con **FluentValidation** y persistencia con
**EF Core + PostgreSQL (Npgsql)** en modo **Database First**. Incluye un ejemplo
completo de tipo **TODO** (`TodoItem`) como referencia, pruebas unitarias con
**xUnit + Moq** y pruebas de integración del controlador con **Testcontainers**.

## Estructura de la solución

```
Ceiba.LiveEvent.Reservations.slnx
├── db
│   ├── scripts           -> Scripts SQL Database First (creación de esquema, seed).
│   └── README.md         -> Cómo aplicar los scripts.
├── src
│   ├── Domain            -> Núcleo del dominio (DDD). Sin dependencias de framework.
│   ├── Application       -> Casos de uso (CQRS). Commands/Queries, handlers, validators.
│   ├── Infrastructure    -> EF Core + PostgreSQL (DbContext, repositorio). Database First.
│   └── Api               -> Presentación. Controladores ASP.NET Core + MediatR.
└── tests
    ├── Domain.Tests        -> Pruebas de invariantes del dominio (xUnit).
    ├── Application.Tests   -> Pruebas de handlers/validadores (xUnit + Moq).
    └── Api.IntegrationTests-> Pruebas del controlador end-to-end (Testcontainers/PostgreSQL).
```

### Regla de dependencias (Clean Architecture)

```
Api ──> Application ──> Domain
Api ──> Infrastructure ──> Application ──> Domain
```

El `Domain` no conoce a ninguna otra capa. Las dependencias apuntan siempre hacia
adentro; la inversión de control se logra con la interfaz `ITodoRepository`
(definida en `Application`, implementada en `Infrastructure`).

## Conceptos aplicados

- **DDD**: `TodoItem` es una *raíz de agregado* (`BaseEntity` / `BaseAuditableEntity`,
  `IAggregateRoot`) con setters privados, métodos de comportamiento
  (`Create`, `UpdateDetails`, `MarkAsComplete`, `Reopen`) e invariantes que lanzan
  `DomainException`. Emite *eventos de dominio* (`TodoItemCreatedEvent`,
  `TodoItemCompletedEvent`) que se despachan vía MediatR al guardar.
- **CQRS + MediatR**: comandos (`CreateTodoItem`, `UpdateTodoItem`,
  `CompleteTodoItem`, `DeleteTodoItem`) y consultas (`GetTodoItems`,
  `GetTodoItemById`), cada uno con su handler.
- **FluentValidation**: validadores por comando ejecutados automáticamente por el
  `ValidationBehaviour` del pipeline de MediatR.
- **Manejo de errores**: `GlobalExceptionHandler` traduce las excepciones a
  respuestas `ProblemDetails` (400 validación/dominio, 404 no encontrado, 500).

## Persistencia (EF Core + PostgreSQL, Database First)

El esquema se gestiona con los scripts SQL numerados de [`db/scripts`](db/README.md),
**no con migraciones de EF Core**. El modelo (tablas, campos y relaciones en inglés)
deriva del diagrama ER `db/Diagrama-ER-Events.jpg`:

- **Tablas maestras**: `event_status`, `venue`, `event_type`, `reservation_status`.
- **Tablas dependientes (FK)**: `event` (→ `venue`, `event_type`, `event_status`) y
  `reservation` (→ `event`, `reservation_status`).
- **Tabla de ejemplo**: `todo_items` (respalda el ejemplo TODO de la arquitectura).

Los scripts se ejecutan en orden numérico ascendente: primero la base de datos (`00`),
luego las maestras (`01`–`04`), luego las dependientes (`05`–`06`) y finalmente la
tabla de ejemplo (`07`–`08`). Detalle completo en [`db/README.md`](db/README.md).

El `ApplicationDbContext` / `TodoItemConfiguration` solo mapean el modelo a las tablas
existentes; el contexto añade auditoría (`created_at` / `last_modified_at`) y despacha
los eventos de dominio al guardar. La cadena de conexión se configura en
`src/Api/appsettings.json` → `ConnectionStrings:Postgres`.

## Cómo ejecutar

```powershell
# 1. Levantar PostgreSQL (ejemplo con Docker)
docker run --name ceiba-pg -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16-alpine

# 2. Aplicar los scripts Database First en orden (ver db/README.md)
$env:PGPASSWORD = "postgres"
psql -h localhost -U postgres -d postgres -f db/scripts/00_create_database.sql
Get-ChildItem db/scripts/*.sql | Where-Object Name -ne '00_create_database.sql' |
    Sort-Object Name |
    ForEach-Object { psql -h localhost -U postgres -d ceiba_reservations -f $_.FullName }

# 3. Compilar y ejecutar la API
dotnet build
dotnet run --project src/Api

# 4. Ejecutar las pruebas (los tests de integración requieren Docker)
dotnet test
```

Los ejemplos de peticiones están en `src/Api/Api.http`. Las pruebas de integración
levantan su propio contenedor PostgreSQL efímero con Testcontainers y aplican el
script Database First automáticamente, por lo que no usan tu base de datos local.

## Documentación de la API (Swagger / OpenAPI)

La API genera su documentación **OpenAPI 3.1** con el generador nativo de .NET 10
(`Microsoft.AspNetCore.OpenApi`) y la expone con **Swagger UI**
(`Swashbuckle.AspNetCore.SwaggerUI`). Con la API en ejecución:

| Recurso            | URL                                  |
|--------------------|--------------------------------------|
| Swagger UI         | `https://localhost:<puerto>/swagger` |
| Documento OpenAPI  | `https://localhost:<puerto>/openapi/v1.json` |

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

| Método | Ruta                              | Descripción                  |
|--------|-----------------------------------|------------------------------|
| GET    | `/api/todoitems`                  | Listar (filtro `onlyPending`) |
| GET    | `/api/todoitems/{id}`             | Obtener por id               |
| POST   | `/api/todoitems`                  | Crear                        |
| PUT    | `/api/todoitems/{id}`             | Actualizar                   |
| POST   | `/api/todoitems/{id}/complete`    | Completar                    |
| DELETE | `/api/todoitems/{id}`             | Eliminar                     |
