using System.Collections.ObjectModel;

namespace Passwordless.Common.Services.Licensing.Models;

public abstract class BaseLicenseData
{
    public ushort ManifestVersion { get; init; }
    public Guid InstallationId { get; init; }
    public ReadOnlyDictionary<string, BasePlan> Plans { get; init; }
};