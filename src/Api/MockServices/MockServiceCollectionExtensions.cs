using System.Diagnostics;
using Passwordless.Service;

namespace Passwordless.Api.MockServices;

public static class MockServiceCollectionExtensions
{
    [Conditional("MOCK_MODE")]
    public static void AddMockServices(this IServiceCollection services)
    {
        services.AddSingleton<IFido2ServiceFactory, MockFido2ServiceFactory>();
        services.AddSingleton<IUserCredentialsService, MockUserCredentialsService>();
    }
}
