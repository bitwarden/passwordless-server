using System.Runtime.CompilerServices;

namespace Passwordless.Common.Services.Licensing;

public static class LicensingBootstrap
{
    public static void AddLicensingWriter(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ILicenseWriterFactory, LicenseWriterFactory>();
        builder.Services.AddSingleton<ISignatureProvider, SignatureProvider>();
    }
    
    public static void AddLicensingReader(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ISignatureProvider, SignatureProvider>();
    }
}