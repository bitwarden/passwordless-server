using System.IdentityModel.Tokens.Jwt;
using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing;

public interface ILicenseWriter
{
    /// <summary>
    /// Returns a signed JWT license.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    JwtSecurityToken Write(LicenseParameters parameters);
}