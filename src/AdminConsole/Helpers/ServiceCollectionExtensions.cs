using AdminConsole.Pages.Organization;
using FluentValidation;

namespace Passwordless.AdminConsole.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services) => services
        .AddTransient<IValidator<CreateApplicationModel.CreateApplicationForm>, CreateApplicationModel.CreateApplicationFormValidator>()
        .AddTransient<IValidator<CreateModel>, CreateModelValidator>()
        .AddTransient<IValidator<InviteForm>, InviteFormValidator>();
}