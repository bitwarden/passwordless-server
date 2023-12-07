namespace Passwordless.Common.Services.Licensing;

public interface ILicenseReader
{
    Task<string> RequestLicenseAsync();
    Task ReadAsync();
}