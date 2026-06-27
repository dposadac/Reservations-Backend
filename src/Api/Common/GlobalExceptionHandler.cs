using Ceiba.LiveEvent.Reservations.Api.Messages;
using Ceiba.LiveEvent.Reservations.Application.Common.Exceptions;
using Ceiba.LiveEvent.Reservations.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ceiba.LiveEvent.Reservations.Api.Common;

/// <summary>
/// Traduce las excepciones de la aplicación y el dominio a respuestas
/// <see cref="ProblemDetails"/> con el código HTTP adecuado. Los títulos provienen del
/// catálogo de mensajes (<see cref="IMessageService"/>) y el detalle incluye el mensaje
/// del error para que el cliente sepa qué ocurrió.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly IMessageService _messages;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        IMessageService messages,
        IHostEnvironment environment,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _messages = messages;
        _environment = environment;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = MapException(exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Error no controlado: {Message}", exception.Message);
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private ProblemDetails MapException(Exception exception) => exception switch
    {
        ValidationException validation => new ValidationProblemDetails(validation.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = _messages.Get("error.validation")
        },
        NotFoundException notFound => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = _messages.Get("error.notFound"),
            Detail = notFound.Message
        },
        // Regla de negocio (RN) con clave de mensaje: el texto se resuelve del catálogo.
        BusinessRuleException rule => new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = _messages.Get("error.businessRule"),
            Detail = _messages.Get(rule.MessageKey, rule.Args)
        },
        DomainException domain => new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = _messages.Get("error.businessRule"),
            Detail = domain.Message
        },
        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = _messages.Get("error.unexpected"),
            // El mensaje técnico solo se expone en desarrollo para no filtrar detalles internos.
            Detail = _environment.IsDevelopment() ? exception.Message : null
        }
    };
}
