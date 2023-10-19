namespace Passwordless.Service.Features;

public interface IFeatureContextProvider
{
    Task<IFeaturesContext> UseContext();
    Task<IFeaturesContext> UseContext(string appId);
}