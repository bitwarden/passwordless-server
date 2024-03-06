using System.Collections.Immutable;

namespace Passwordless.Common.Backup;

public interface IBackupSerializer
{
    string Serialize<TEntity>(ImmutableList<TEntity> entities);
    
    IEnumerable<T>? Deserialize<T>(string data);
}