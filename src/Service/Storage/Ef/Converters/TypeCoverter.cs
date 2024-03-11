using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Passwordless.Service.Storage.Ef.Converters;

/// <summary>
/// Converts a <see cref="Type"/> to a <see cref="string" /> and back.
/// </summary>
public sealed class TypeConverter() : ValueConverter<Type?, string?>(
    v => v != null ? v.FullName : null,
    v => v != null ? Type.GetType(v) : null);