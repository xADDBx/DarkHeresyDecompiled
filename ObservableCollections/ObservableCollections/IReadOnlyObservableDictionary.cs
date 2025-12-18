using System.Collections;
using System.Collections.Generic;

namespace ObservableCollections;

public interface IReadOnlyObservableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IObservableCollection<KeyValuePair<TKey, TValue>>
{
}
