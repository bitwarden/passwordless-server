using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing.Interpreters;

public interface ILicenseInterpreter<TData> where TData : BaseLicenseData
{
    /// <summary>
    /// Generates a license from the given parameters.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    Task<TData> GenerateAsync(LicenseParameters parameters);
}