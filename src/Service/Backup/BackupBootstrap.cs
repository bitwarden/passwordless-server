using Microsoft.Extensions.DependencyInjection;
using Passwordless.Common.Backup;

namespace Passwordless.Service.Backup;

public static class BackupBootstrap
{
    public static void AddBackup(this IServiceCollection services)
    {
        services.AddSingleton<IBackupSerializer, TsvBackupSerializer>();
        services.AddSingleton<IBackupUtility, BackupUtility>();
    }
}