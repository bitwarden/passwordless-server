using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing.V1;

[ManifestVersion(1)]
public class LicenseWriter1 : ILicenseWriter
{
    public Task<License<T>> WriteAsync<T>(T license) where T : BaseLicenseData<BasePlan>
    {
        // create jwt from license

        return null;
    }
}