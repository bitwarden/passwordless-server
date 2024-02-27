using Fido2NetLib;
using Passwordless.Service.Models;

namespace Passwordless.Service;

public interface IFido2Service
{
    Task<string> CreateRegisterTokenAsync(RegisterToken tokenInput);
    Task<SessionResponse<CredentialCreateOptions>> RegisterBeginAsync(FidoRegistrationBeginDTO request);
    Task<TokenResponse> RegisterCompleteAsync(RegistrationCompleteDTO request, string deviceInfo, string country);

    Task<string> CreateSigninTokenAsync(SigninTokenRequest request);
    Task<string> CreateMagicLinkTokenAsync(MagicLinkTokenRequest request);
    Task<SessionResponse<AssertionOptions>> SignInBeginAsync(SignInBeginDTO request);
    Task<TokenResponse> SignInCompleteAsync(SignInCompleteDTO request, string device, string country);
    Task<VerifySignInToken> SignInVerifyAsync(SignInVerifyDTO payload);

    Task<List<AliasPointer>> GetAliases(string userId);
    Task SetAliasAsync(AliasPayload data);
}