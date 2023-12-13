using Passwordless.Common.Services.Licensing.Configuration;

namespace Passwordless.Common.Services.Licensing;

public static class LicensingBootstrap
{
    public static void AddLicensingWriter(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<FileSignatureConfiguration>().BindConfiguration("License:Providers:File");
        builder.Services.AddSingleton<ILicenseWriterFactory, LicenseWriterFactory>();
        builder.Services.AddSingleton<ISignatureProvider, FileSignatureProvider>();
    }
    
    public static void AddLicensingReader(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<FileSignatureConfiguration>().BindConfiguration("License:Providers:File");
        builder.Services.AddSingleton<ISignatureProvider, FileSignatureProvider>();
    }
}