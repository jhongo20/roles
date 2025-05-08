using AuthSystem.Application.Commands.Email;
using FluentValidation;

namespace AuthSystem.Application.Validators
{
    public class ResendEmailConfirmationCommandValidator : AbstractValidator<ResendEmailConfirmationCommand>
    {
        public ResendEmailConfirmationCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El formato del email no es válido");
        }
    }
}
