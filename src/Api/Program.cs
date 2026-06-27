using System.Text.Json.Serialization;
using Ceiba.LiveEvent.Reservations.Api.BackgroundJobs;
using Ceiba.LiveEvent.Reservations.Api.Common;
using Ceiba.LiveEvent.Reservations.Api.Cors;
using Ceiba.LiveEvent.Reservations.Api.Messages;
using Ceiba.LiveEvent.Reservations.Api.OpenApi;
using Ceiba.LiveEvent.Reservations.Application;
using Ceiba.LiveEvent.Reservations.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Capas de la arquitectura limpia.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API / presentación. Los enums se serializan como texto (p. ej. "Concierto").
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// CORS para peticiones desde clientes web.
builder.Services.AddApiCors(builder.Configuration);

// Documentación OpenAPI/Swagger (configuración transversal).
builder.Services.AddApiDocumentation(builder.Configuration);

// Catálogo de mensajes (Resources/messages.json) accesible por clave.
builder.Services.AddMessageCatalog(builder.Environment);

// Job diario que marca como "completado" los eventos finalizados (RN-06).
builder.Services.AddHostedService<EventCompletionBackgroundService>();

// Manejo global de excepciones -> ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseApiDocumentation();

app.UseExceptionHandler();

// CORS debe ir antes de la autorización y del enrutado de endpoints.
app.UseApiCors();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Necesario para exponer la clase Program a los tests de integración.
public partial class Program;
