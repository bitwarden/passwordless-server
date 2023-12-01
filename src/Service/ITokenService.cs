namespace Passwordless.Service;

public interface ITokenService
{
    Task InitAsync(CancellationToken cancellationToken);
    T DecodeToken<T>(string token, string prefix, bool contractless = false);
    string EncodeToken<T>(T token, string prefix, bool contractless = false);
}