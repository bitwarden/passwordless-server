using FluentValidation;
using Passwordless.Service.Models;

namespace Passwordless.Api.Validators;

public class RegistrationCompleteDTOValidator : AbstractValidator<RegistrationCompleteDTO>
{
    public RegistrationCompleteDTOValidator()
    {
        RuleFor(x => x.Nickname).MaximumLength(255);
    }
}