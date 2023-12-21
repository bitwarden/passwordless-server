using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing.Interpreters;

public interface ILicenseInterpreterFactory
{
    /// <summary>
    /// Creates a license interpreter for the given parameters.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    ILicenseInterpreter Create(LicenseParameters parameters);
}