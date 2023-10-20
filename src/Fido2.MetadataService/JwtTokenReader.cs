using System.IdentityModel.Tokens.Jwt;

namespace Passwordless.Fido2.MetadataService;

public class JwtTokenReader : IJwtTokenReader
{
    public string Read(string jwtToken)
    {
        if (jwtToken == null) throw new ArgumentNullException();
        var handler = new JwtSecurityTokenHandler { MaximumTokenSizeInBytes = int.MaxValue };
        JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(jwtToken);
        var payload = jwtSecurityToken.Payload.SerializeToJson();
        return payload;
    }
}