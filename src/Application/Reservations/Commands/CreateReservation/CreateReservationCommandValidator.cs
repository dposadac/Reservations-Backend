using FluentValidation;

namespace Ceiba.LiveEvent.Reservations.Application.Reservations.Commands.CreateReservation;

public sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("El evento es obligatorio.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("La cantidad de entradas debe ser 1 o más.");

        RuleFor(x => x.PurchaserName)
            .NotEmpty().WithMessage("El nombre del comprador es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre del comprador no puede superar los 50 caracteres.");

        RuleFor(x => x.PurchaserEmail)
            .NotEmpty().WithMessage("El correo del comprador es obligatorio.")
            .EmailAddress().WithMessage("El correo del comprador no tiene un formato válido.")
            .MaximumLength(150).WithMessage("El correo del comprador no puede superar los 150 caracteres.");

        RuleFor(x => x.City)
            .MaximumLength(50).WithMessage("La ciudad no puede superar los 50 caracteres.");
    }
}
