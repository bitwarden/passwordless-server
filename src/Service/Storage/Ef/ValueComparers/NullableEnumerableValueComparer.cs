using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Passwordless.Service.Storage.Ef.ValueComparers;

public sealed class NullableEnumerableValueComparer<T>() : ValueComparer<IEnumerable<T>?>(
    (c1, c2) => (c1 == null && c2 == null) || c1!.SequenceEqual(c2!),
    c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
    c => c != null ? c.ToArray() : null);