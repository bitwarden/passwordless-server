using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Passwordless.Common.Backup;

public class TsvBackupSerializer : IBackupSerializer
{
    public string Serialize<TDbContext, TEntity>(TDbContext dbContext, IEnumerable<TEntity> entities) where TDbContext : DbContext
    {
        var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
        if (entityType == null) throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} not found in model.");
        var properties = entityType.GetProperties().ToImmutableList();

        var stringBuilder = new StringBuilder();

        // Write header row
        stringBuilder.AppendLine(string.Join("\t", properties.Select(p => p.Name)));

        // Write data rows
        foreach (var entity in entities)
        {
            var row = string.Join("\t", properties.Select(p => JsonSerializer.Serialize(p.PropertyInfo!.GetValue(entity))));
            stringBuilder.AppendLine(row);
        }
        
        return stringBuilder.ToString();
    }

    public string Serialize<TEntity>(ImmutableList<TEntity> entities)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T>? Deserialize<T>(string data)
    {
        throw new NotImplementedException();
    }
}