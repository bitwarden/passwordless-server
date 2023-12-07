namespace Passwordless.Common.Services.Licensing;

public interface ILicenseWriterFactory
{
    /// <summary>
    /// Returns a <see cref="ILicenseWriter"/> for the given manifest version.
    /// </summary>
    /// <param name="manifestVersion"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    ILicenseWriter Create(ushort manifestVersion);
}