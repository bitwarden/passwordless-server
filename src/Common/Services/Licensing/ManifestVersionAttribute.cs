namespace Passwordless.Common.Services.Licensing;

public class ManifestVersionAttribute : Attribute
{
    public ManifestVersionAttribute(ushort manifestVersion)
    {
        ManifestVersion = manifestVersion;
    }

    public ushort ManifestVersion { get; }
}