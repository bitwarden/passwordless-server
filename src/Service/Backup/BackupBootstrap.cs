using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Passwordless.Common.Backup;
using Passwordless.Common.Backup.Mapping;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Backup;

public static class BackupBootstrap
{
    public static void AddBackup(this IServiceCollection services)
    {
        services.AddScoped<IBackupSerializer, CsvBackupSerializer>();
        services.AddScoped<IBackupUtility, BackupUtility>();

        services.AddTransient<ClassMap<AccountMetaInformation>, EntityFrameworkMap<AccountMetaInformation, DbGlobalContext>>();
        services.AddTransient<ClassMap<ApiKeyDesc>, EntityFrameworkMap<ApiKeyDesc, DbGlobalContext>>();
        services.AddTransient<ClassMap<AppFeature>, EntityFrameworkMap<AppFeature, DbGlobalContext>>();
        services.AddTransient<ClassMap<Authenticator>, EntityFrameworkMap<Authenticator, DbGlobalContext>>();
        services.AddTransient<ClassMap<AliasPointer>, EntityFrameworkMap<AliasPointer, DbGlobalContext>>();
        services.AddTransient<ClassMap<EFStoredCredential>, EntityFrameworkMap<EFStoredCredential, DbGlobalContext>>();
        services.AddTransient<ClassMap<ApplicationEvent>, EntityFrameworkMap<ApplicationEvent, DbGlobalContext>>();
        services.AddTransient<ClassMap<PeriodicCredentialReport>, EntityFrameworkMap<PeriodicCredentialReport, DbGlobalContext>>();
        services.AddTransient<ClassMap<PeriodicActiveUserReport>, EntityFrameworkMap<PeriodicActiveUserReport, DbGlobalContext>>();
    }
}