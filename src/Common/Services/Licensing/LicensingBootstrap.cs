using Passwordless.Common.Services.Licensing.Cryptography;
using Passwordless.Common.Services.Licensing.Interpreters;

namespace Passwordless.Common.Services.Licensing;

public static class LicensingBootstrap
{
    public static void AddLicensingWriter(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<FileCryptographyConfiguration>().BindConfiguration("License:Providers:File");
        builder.Services.AddSingleton<ILicenseInterpreterFactory, LicenseInterpreterFactory>();
        builder.Services.AddSingleton<ICryptographyProvider, FileCryptographyProvider>();
        builder.Services.AddSingleton<ILicenseWriter, LicenseWriter>();
    }

    public static void AddLicensingReader(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<FileCryptographyConfiguration>().BindConfiguration("License:Providers:File");
        builder.Services.AddSingleton<ICryptographyProvider, FileCryptographyProvider>();
        builder.Services.AddSingleton<ILicenseReader, LicenseReader>();
    }
}