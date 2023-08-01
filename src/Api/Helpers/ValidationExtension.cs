using FluentValidation;
using Passwordless.Api.Validators;
using Passwordless.Service.Models;

namespace Passwordless.Api.Helpers;

public static class ValidationExtension
{
    public static IServiceCollection AddValidators(this IServiceCollection services) =>
        services.AddTransient<IValidator<RegistrationCompleteDTO>, RegistrationCompleteDTOValidator>();
}