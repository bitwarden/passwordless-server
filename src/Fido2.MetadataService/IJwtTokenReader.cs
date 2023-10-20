namespace Passwordless.Fido2.MetadataService;

public interface IJwtTokenReader
{
    /// <summary>
    /// Decodes & validates a JWT token.
    /// </summary>
    /// <param name="jwtToken">JWT token</param>
    /// <returns>Decoded JWT token in JSON format.</returns>
    string Read(string jwtToken);
}