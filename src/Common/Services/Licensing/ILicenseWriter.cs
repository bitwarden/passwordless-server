using Microsoft.IdentityModel.Tokens;
using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing;

public interface ILicenseWriter
{
    Task<SecurityToken> WriteAsync(LicenseParameters parameters);
}