using AuthSystem.Application.Queries;
using FluentValidation;

namespace AuthSystem.Application.Validators
{
    public class GetEmailConfirmationStatusQueryValidator : AbstractValidator<GetEmailConfirmationStatusQuery>
    {
        public GetEmailConfirmationStatusQueryValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El formato del email no es válido");
        }
    }
}
