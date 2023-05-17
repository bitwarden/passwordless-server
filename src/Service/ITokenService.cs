namespace Passwordless.Service;

public interface ITokenService
{
    Task InitAsync();
    T DecodeToken<T>(string token, string prefix, bool contractless = false);
    string EncodeToken<T>(T token, string prefix, bool contractless = false);
}