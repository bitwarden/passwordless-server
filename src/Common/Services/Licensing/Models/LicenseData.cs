namespace Passwordless.Common.Services.Licensing.Models;

public sealed class LicenseData : BaseLicenseData
{
    public Dictionary<string, Plan> Plans { get; set; }
}