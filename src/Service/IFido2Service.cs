using Fido2NetLib;
using Passwordless.Service.Models;

namespace Passwordless.Service;

public interface IFido2Service
{
    Task<string> CreateRegisterToken(RegisterToken tokenInput);
    Task<SessionResponse<CredentialCreateOptions>> RegisterBegin(FidoRegistrationBeginDTO request);
    Task<TokenResponse> RegisterComplete(RegistrationCompleteDTO request, string deviceInfo, string country);

    Task<string> CreateSigninToken(SigninTokenRequest request);
    Task<string> CreateMagicLinkToken(MagicLinkTokenRequest request);
    Task<SessionResponse<AssertionOptions>> SignInBegin(SignInBeginDTO request);
    Task<TokenResponse> SignInComplete(SignInCompleteDTO request, string device, string country);
    Task<VerifySignInToken> SignInVerify(SignInVerifyDTO payload);

    Task<List<AliasPointer>> GetAliases(string userId);
    Task SetAlias(AliasPayload data);
}