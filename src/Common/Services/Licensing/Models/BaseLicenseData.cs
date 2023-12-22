namespace Passwordless.Common.Services.Licensing.Models;

public abstract class BaseLicenseData
{
    public ushort ManifestVersion { get; set; }
    public Guid InstallationId { get; set; }
    public DateTime ExpiresAt { get; set; }
}