using Bogus;
using Fido2NetLib.Objects;
using Passwordless.Common.MagicLinks.Models;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Models;

namespace Passwordless.Api.IntegrationTests.Helpers.App;

public static class RequestHelpers
{
    public static Faker<SendMagicLinkRequest> GetMagicLinkRequestRules() => new Faker<SendMagicLinkRequest>()
        .RuleFor(x => x.UserId, () => Guid.NewGuid().ToString())
        .RuleFor(x => x.EmailAddress, faker => faker.Person.Email)
        .RuleFor(x => x.UrlTemplate, faker => $"{faker.Internet.Url()}?token={SendMagicLinkRequest.TokenTemplate}");

    public static Faker<RegisterToken> GetRegisterTokenGeneratorRules() => new Faker<RegisterToken>()
        .RuleFor(x => x.UserId, Guid.NewGuid().ToString())
        .RuleFor(x => x.DisplayName, x => x.Person.FullName)
        .RuleFor(x => x.Username, x => x.Person.Email)
        .RuleFor(x => x.Attestation, "None")
        .RuleFor(x => x.Discoverable, true)
        .RuleFor(x => x.UserVerification, "Preferred")
        .RuleFor(x => x.Aliases, x => [x.Person.FirstName])
        .RuleFor(x => x.AliasHashing, false)
        .RuleFor(x => x.ExpiresAt, DateTime.UtcNow.AddDays(1))
        .RuleFor(x => x.TokenId, Guid.Empty);

    public static Faker<SetAuthenticationConfigurationRequest> GetSetAuthenticationConfigurationRequest() =>
        new Faker<SetAuthenticationConfigurationRequest>()
            .RuleFor(x => x.Purpose, faker => faker.Lorem.Word())
            .RuleFor(x => x.UserVerificationRequirement, GetRandomEnumValue<UserVerificationRequirement>())
            .RuleFor(x => x.TimeToLive, faker => faker.Date.Timespan())
            .RuleFor(x => x.PerformedBy, faker => faker.Person.UserName);

    private static T GetRandomEnumValue<T>() where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        return values[Random.Shared.Next(values.Length)];
    }
}