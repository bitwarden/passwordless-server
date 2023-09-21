using Fido2NetLib;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public interface IFido2Service
{
    Task<SessionResponse<CredentialCreateOptions>> RegisterBegin(FidoRegistrationBeginDTO request);
    Task<VerifySignInToken> SignInVerify(SignInVerifyDTO payload);
    Task<string> CreateToken(RegisterToken tokenProps);
    Task<TokenResponse> RegisterComplete(RegistrationCompleteDTO request, string deviceInfo, string country);
    Task<SessionResponse<AssertionOptions>> SignInBegin(SignInBeginDTO request);
    Task<TokenResponse> SignInComplete(SignInCompleteDTO request, string device, string country);
    Task<List<AliasPointer>> GetAliases(string userId);
    Task SetAlias(AliasPayload data);
}