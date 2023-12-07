namespace Passwordless.Common.Services.Licensing.Models;

public class License<TLicenseData> where TLicenseData : BaseLicenseData<BasePlan>
{
    public BaseLicenseData<BasePlan> Data { get; set; }
    public byte[] Signature { get; set; }
}