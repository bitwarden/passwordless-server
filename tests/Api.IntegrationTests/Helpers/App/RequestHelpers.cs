using Bogus;
using Passwordless.Service.MagicLinks.Models;
using Passwordless.Service.Models;

namespace Passwordless.Api.IntegrationTests.Helpers.App;

public static class RequestHelpers
{
    public static Faker<SendMagicLinkRequest> GetMagicLinkRequestRules() => new Faker<SendMagicLinkRequest>()
        .RuleFor(x => x.UserId, () => Guid.NewGuid().ToString())
        .RuleFor(x => x.UserEmail, faker => faker.Person.Email)
        .RuleFor(x => x.MagicLinkUrl, faker => $"{faker.Internet.Url()}?token=<token>");
    
    public static Faker<RegisterToken> GetRegisterTokenGeneratorRules() => new Faker<RegisterToken>()
        .RuleFor(x => x.UserId, Guid.NewGuid().ToString())
        .RuleFor(x => x.DisplayName, x => x.Person.FullName)
        .RuleFor(x => x.Username, x => x.Person.Email)
        .RuleFor(x => x.Attestation, "None")
        .RuleFor(x => x.Discoverable, true)
        .RuleFor(x => x.UserVerification, "Preferred")
        .RuleFor(x => x.Aliases, x => new HashSet<string> { x.Person.FirstName })
        .RuleFor(x => x.AliasHashing, false)
        .RuleFor(x => x.ExpiresAt, DateTime.UtcNow.AddDays(1))
        .RuleFor(x => x.TokenId, Guid.Empty);
}