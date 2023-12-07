using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing.V1;

[ManifestVersion(1)]
public record LicenseData(
    ushort ManifestVersion,
    Guid InstallationId,
    IDictionary<string, Plan> Plans)
    : BaseLicenseData<Plan>(ManifestVersion, InstallationId, Plans);