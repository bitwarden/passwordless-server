using System.Collections.ObjectModel;
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
            Plans = new ReadOnlyDictionary<string, BasePlan>(parameters.Plans.Select(p => new KeyValuePair<string, BasePlan>(p.Key, new Plan(p.Value.Seats, p.Value.SupportsAuditLogging))).ToDictionary())
        };
    }
}