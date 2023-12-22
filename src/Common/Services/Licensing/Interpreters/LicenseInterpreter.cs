using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing.Interpreters;

[ManifestVersion(1)]
public class LicenseInterpreter : ILicenseInterpreter
{
    public BaseLicenseData Generate(LicenseParameters parameters)
    {
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }
        return new LicenseData
        {
            InstallationId = parameters.InstallationId,
            ManifestVersion = parameters.ManifestVersion,
            Plans = parameters.Plans.Select(p => new KeyValuePair<string, Plan>(p.Key, new Plan(p.Value.Seats, p.Value.SupportsAuditLogging))).ToDictionary()
        };
    }
}