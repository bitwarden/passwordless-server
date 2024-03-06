using System.Collections.Immutable;

namespace Passwordless.Common.Backup;

public interface IBackupSerializer
{
    string Serialize<TEntity>(IReadOnlyCollection<TEntity> entities);

    IEnumerable<TEntity>? Deserialize<TEntity>(string data);
}