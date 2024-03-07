using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Passwordless.Common.Backup;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Backup;

public class BackupUtility : IBackupUtility
{
    private readonly IBackupSerializer _backupSerializer;
    private readonly DbTenantContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<BackupUtility> _logger;

    public BackupUtility(
        IBackupSerializer backupSerializer,
        DbTenantContext dbContext,
        TimeProvider timeProvider,
        ITenantProvider tenantProvider,
        ILogger<BackupUtility> logger)
    {
        _backupSerializer = backupSerializer;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task BackupAsync()
    {
        var groupId = Guid.NewGuid();
        await BackupEntityAsync<AccountMetaInformation>(groupId);
        await BackupEntityAsync<ApiKeyDesc>(groupId);
        await BackupEntityAsync<AppFeature>(groupId);
        await BackupEntityAsync<Authenticator>(groupId);
        await BackupEntityAsync<AliasPointer>(groupId);
        await BackupEntityAsync<EFStoredCredential>(groupId);
        await BackupEntityAsync<ApplicationEvent>(groupId);
        await BackupEntityAsync<PeriodicCredentialReport>(groupId);
        await BackupEntityAsync<PeriodicActiveUserReport>(groupId);
    }

    private async Task BackupEntityAsync<TEntity>(Guid groupId) where TEntity : class
    {
        var entities = await _dbContext.Set<TEntity>().ToListAsync();
        var data = _backupSerializer.Serialize(entities);
        _logger.LogInformation(data);
        var archive = new Archive
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            Entity = typeof(TEntity).Name,
            Data = data,
            Tenant = _tenantProvider.Tenant
        };
        //_dbContext.Archives.Add(archive);
        //await _dbContext.SaveChangesAsync();
    }
}