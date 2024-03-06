using CsvHelper.Configuration;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Backup.Mapping;

/// <summary>
/// Only maps the properties of the entity that are present in the database. We don't want to export properties that are
/// not in the database. If they are also named differently in the database than their CLR equivalent, we want to map
/// them to the CLR property name.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public sealed class EntityFrameworkMap<TEntity> : ClassMap<TEntity>
{
    public EntityFrameworkMap(DbGlobalContext dbContext)
    {
        var entity = dbContext.Model.FindEntityType(typeof(TEntity));
        if (entity == null)
        {
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} not found in model.");
        }

        var properties = entity.GetProperties().OrderBy(x => x.Name);

        foreach (var property in properties)
        {
            Map(m => property.PropertyInfo!.GetValue(m)).Name(property.Name);
        }
    }
}