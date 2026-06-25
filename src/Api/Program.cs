using Ceiba.LiveEvent.Reservations.Api.Common;
using Ceiba.LiveEvent.Reservations.Api.OpenApi;
using Ceiba.LiveEvent.Reservations.Application;
using Ceiba.LiveEvent.Reservations.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Capas de la arquitectura limpia.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API / presentación.
builder.Services.AddControllers();

// Documentación OpenAPI/Swagger (configuración transversal).
builder.Services.AddApiDocumentation(builder.Configuration);

// Manejo global de excepciones -> ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseApiDocumentation();

app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Necesario para exponer la clase Program a los tests de integración.
public partial class Program;
