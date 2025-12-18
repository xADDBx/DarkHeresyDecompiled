using System;
using System.Collections;
using System.Collections.Generic;

namespace ObservableCollections;

public interface ISortableSynchronizedView<T, TView> : ISynchronizedView<T, TView>, IReadOnlyCollection<(T Value, TView View)>, IEnumerable<(T Value, TView View)>, IEnumerable, IDisposable
{
	void Sort(IComparer<T> comparer);

	void Sort(IComparer<TView> viewComparer);
}
