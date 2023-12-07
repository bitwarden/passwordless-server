namespace Passwordless.Common.Services.Licensing.Models;

public abstract record BaseLicenseData<TPlan>(
    ushort ManifestVersion,
    Guid InstallationId,
    IDictionary<string, TPlan> Plans) where TPlan : BasePlan;