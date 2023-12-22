namespace Passwordless.Common.Services.Licensing;

public class ManifestVersionAttribute : Attribute
{
    public ManifestVersionAttribute(ushort manifestVersion)
    {
        ManifestVersion = manifestVersion;
    }

    /// <summary>
    /// The manifest version of the license.
    /// </summary>
    public ushort ManifestVersion { get; }
}