namespace Passwordless.Common.Services.Licensing.Models;

public record LicenseParameters(
    uint ManifestVersion,
    Guid InstallationId,
    DateTime ExpiresAt,
    IDictionary<string, PlanParameters> Plans);