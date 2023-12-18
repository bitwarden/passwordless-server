using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing;

public interface ILicenseReader
{
    /// <summary>
    /// Validate & read the JWT token and return the license data.
    /// It will always return the current LicenseData model, as the deployed instance should not have a concept of different versions.
    /// </summary>
    /// <param name="jwt"></param>
    /// <returns></returns>
    Task<LicenseData> ValidateAsync(string jwt);
}