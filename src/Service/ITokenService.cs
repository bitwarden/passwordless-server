namespace Passwordless.Service;

public interface ITokenService
{
    Task<T> DecodeTokenAsync<T>(string token, string prefix, bool contractless = false);
    Task<string> EncodeTokenAsync<T>(T token, string prefix, bool contractless = false);
}