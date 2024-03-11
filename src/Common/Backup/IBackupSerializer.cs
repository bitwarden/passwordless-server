namespace Passwordless.Common.Backup;

public interface IBackupSerializer
{
    byte[] Serialize<TEntity>(IReadOnlyCollection<TEntity> entities);

    IEnumerable<TEntity>? Deserialize<TEntity>(byte[] data);
}