using AuthSystem.Application.Commands.Email;
using FluentValidation;

namespace AuthSystem.Application.Validators
{
    public class GenerateEmailConfirmationTokenCommandValidator : AbstractValidator<GenerateEmailConfirmationTokenCommand>
    {
        public GenerateEmailConfirmationTokenCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El formato del email no es válido");
        }
    }
}
