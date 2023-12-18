namespace Passwordless.Common.Services.Licensing.Models;

public record LicenseParameters(
    ushort ManifestVersion,
    Guid InstallationId,
    DateTime ExpiresAt,
    IDictionary<string, PlanParameters> Plans);