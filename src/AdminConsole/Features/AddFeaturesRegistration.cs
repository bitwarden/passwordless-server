namespace Passwordless.AdminConsole.Features;

public static class AddFeaturesRegistration
{
    public static IServiceCollection AddFeatures(this IServiceCollection service) =>
        service.AddTransient<IFeaturesContext, FeaturesContext>();
}