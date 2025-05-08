using System;
using AuthSystem.Application.Commands.Email;
using FluentValidation;

namespace AuthSystem.Application.Validators
{
    public class VerifyEmailConfirmationTokenCommandValidator : AbstractValidator<VerifyEmailConfirmationTokenCommand>
    {
        public VerifyEmailConfirmationTokenCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El ID de usuario es requerido");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("El token es requerido")
                .MinimumLength(6).WithMessage("El token debe tener al menos 6 caracteres");
        }
    }
}
