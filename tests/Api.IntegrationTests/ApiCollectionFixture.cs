using Xunit;

namespace Passwordless.Api.IntegrationTests;

[CollectionDefinition(Fixture)]
public class ApiCollectionFixture : ICollectionFixture<PasswordlessApiFixture>
{
    internal const string Fixture = "Api";

}