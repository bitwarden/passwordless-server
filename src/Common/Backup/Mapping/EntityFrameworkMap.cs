using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using AngleSharp.Common;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Passwordless.Common.Backup.Mapping;

/// <summary>
/// Only maps the properties of the entity that are present in the database. We don't want to export properties that are
/// not in the database. If they are also named differently in the database than their CLR equivalent, we want to map
/// them to the CLR property name.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TDbContext"></typeparam>
public sealed class EntityFrameworkMap<TEntity, TDbContext> : ClassMap<TEntity> where TDbContext : DbContext
{
    public EntityFrameworkMap(TDbContext dbContext)
    {
        var entity = dbContext.Model.FindEntityType(typeof(TEntity));
        if (entity == null)
        {
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} not found in model.");
        }

        var properties = entity.GetProperties().OrderBy(x => x.Name).ToImmutableList();

        AutoMap(CultureInfo.InvariantCulture);

        foreach (var map in MemberMaps)
        {
            var efProperty = properties.SingleOrDefault(x => x.PropertyInfo!.Name == map.Data.Member.Name);
            if (efProperty == null)
            {
                map.Ignore();
                continue;
            }

            map.Name(efProperty.Name);
        }

        // Exclude any navigational properties.
        ReferenceMaps.Clear();
    }
}