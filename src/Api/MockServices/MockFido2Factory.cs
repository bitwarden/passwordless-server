using Fido2NetLib;
using Passwordless.Service;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.MockServices;

public class MockFido2ServiceFactory : IFido2ServiceFactory
{
    public Task<IFido2Service> CreateAsync()
    {
        return Task.FromResult<IFido2Service>(new MockFido2Service());
    }

    private class MockFido2Service : IFido2Service
    {
        public Task<string> CreateToken(RegisterToken tokenProps)
        {
            if (string.IsNullOrEmpty(tokenProps.UserId))
            {
                return Task.FromException<string>(new ApiException("Invalid UserId", 400));
            }

            return Task.FromResult("register_token");
        }

        public Task<List<AliasPointer>> GetAliases(string userId)
        {
            switch (userId)
            {
                case "has_aliases":
                    return Task.FromResult(new List<AliasPointer>
                    {
                        new AliasPointer
                        {
                            UserId = userId,
                            Alias = "test_alias_1",
                            Plaintext = "test_plaintext_1",
                            Tenant = "test_app"
                        },
                        new AliasPointer
                        {
                            UserId = userId,
                            Alias = "test_alias_1",
                            Plaintext = "test_plaintext_1",
                            Tenant = "test_app"
                        },
                    });
                default:
                    return Task.FromResult(new List<AliasPointer>());
            }
        }

        public Task<SessionResponse<CredentialCreateOptions>> RegisterBegin(FidoRegistrationBeginDTO request)
        {
            throw new NotImplementedException();
        }

        public Task<TokenResponse> RegisterComplete(RegistrationCompleteDTO request, string deviceInfo, string country)
        {
            return Task.FromResult(new TokenResponse("token"));
        }

        public Task SetAlias(AliasPayload data)
        {
            throw new NotImplementedException();
        }

        public Task<SessionResponse<AssertionOptions>> SignInBegin(SignInBeginDTO request)
        {
            throw new NotImplementedException();
        }

        public Task<TokenResponse> SignInComplete(SignInCompleteDTO request, string device, string country)
        {
            throw new NotImplementedException();
        }

        public Task<VerifySignInToken> SignInVerify(SignInVerifyDTO payload)
        {
            if (payload.Token == "verify_valid")
            {
                return Task.FromResult(new VerifySignInToken
                {
                    Success = true,
                    UserId = "test_user_id",
                    ExpiresAt = DateTime.UtcNow,
                    TokenId = Guid.NewGuid(),
                    Type = "type",
                });
            }
            else
            {
                return Task.FromException<VerifySignInToken>(new ApiException("Some sort of error", 400));
            }
        }
    }
}
