namespace Passwordless.Service;

public interface ITokenService
{
    // Ideally <T> should inherit from Token, but for now we also encode/decode arbitrary objects too
    Task<T> DecodeTokenAsync<T>(string token, string prefix, bool contractless = false);
    Task<string> EncodeTokenAsync<T>(T token, string prefix, bool contractless = false);
}