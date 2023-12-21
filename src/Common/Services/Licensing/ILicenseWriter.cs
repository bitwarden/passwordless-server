using System.IdentityModel.Tokens.Jwt;
using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing;

public interface ILicenseWriter
{
    JwtSecurityToken Write(LicenseParameters parameters);
}