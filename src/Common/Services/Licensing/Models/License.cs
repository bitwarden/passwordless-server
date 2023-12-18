namespace Passwordless.Common.Services.Licensing.Models;

public class License<TData> where TData : BaseLicenseData
{
    public TData Data { get; set; }
    
    public byte[] Signature { get; set; }
}