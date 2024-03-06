using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Common.Backup;
using Passwordless.Service.Backup.Mapping;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Models;

namespace Passwordless.Service.Backup;

public static class BackupBootstrap
{
    public static void AddBackup(this IServiceCollection services)
    {
        services.AddScoped<IBackupSerializer, CsvBackupSerializer>();
        services.AddScoped<IBackupUtility, BackupUtility>();

        services.AddTransient<ClassMap<AccountMetaInformation>, EntityFrameworkMap<AccountMetaInformation>>();
        services.AddTransient<ClassMap<ApiKeyDesc>, EntityFrameworkMap<ApiKeyDesc>>();
        services.AddTransient<ClassMap<AppFeature>, EntityFrameworkMap<AppFeature>>();
        services.AddTransient<ClassMap<Authenticator>, EntityFrameworkMap<Authenticator>>();
        services.AddTransient<ClassMap<AliasPointer>, EntityFrameworkMap<AliasPointer>>();
        services.AddTransient<ClassMap<EFStoredCredential>, EntityFrameworkMap<EFStoredCredential>>();
        services.AddTransient<ClassMap<ApplicationEvent>, EntityFrameworkMap<ApplicationEvent>>();
        services.AddTransient<ClassMap<PeriodicCredentialReport>, EntityFrameworkMap<PeriodicCredentialReport>>();
        services.AddTransient<ClassMap<PeriodicActiveUserReport>, EntityFrameworkMap<PeriodicActiveUserReport>>();
    }
}