namespace System.Collections.Generic;

internal interface IReadOnlySet<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
{
}
