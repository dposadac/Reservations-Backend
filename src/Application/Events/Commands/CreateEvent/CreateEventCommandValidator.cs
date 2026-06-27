using Ceiba.LiveEvent.Reservations.Domain.Events;
using FluentValidation;

namespace Ceiba.LiveEvent.Reservations.Application.Events.Commands.CreateEvent;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .MinimumLength(Event.TitleMinLength).WithMessage($"El título debe tener al menos {Event.TitleMinLength} caracteres.")
            .MaximumLength(Event.TitleMaxLength).WithMessage($"El título no puede superar los {Event.TitleMaxLength} caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MinimumLength(Event.DescriptionMinLength).WithMessage($"La descripción debe tener al menos {Event.DescriptionMinLength} caracteres.")
            .MaximumLength(Event.DescriptionMaxLength).WithMessage($"La descripción no puede superar los {Event.DescriptionMaxLength} caracteres.");

        RuleFor(x => x.VenueId)
            .NotEmpty().WithMessage("El lugar (venue) es obligatorio.");

        RuleFor(x => x.MaximumCapacity)
            .GreaterThan(0).WithMessage("La capacidad máxima debe ser un entero positivo.");

        RuleFor(x => x.TicketPrice)
            .GreaterThan(0).WithMessage("El precio de la entrada debe ser positivo.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("La fecha de finalización debe ser posterior a la de inicio.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("El tipo de evento no es válido.");
    }
}
