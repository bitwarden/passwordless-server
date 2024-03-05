using Microsoft.EntityFrameworkCore;

namespace Passwordless.Common.Backup;

public interface IBackupSerializer
{
    string Serialize<TDbContext, TEntity>(TDbContext dbContext, IEnumerable<TEntity> entities)
        where TDbContext : DbContext;
    
    IEnumerable<T>? Deserialize<T>(string data);
}