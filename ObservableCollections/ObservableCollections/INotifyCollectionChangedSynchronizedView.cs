using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ObservableCollections;

public interface INotifyCollectionChangedSynchronizedView<out TView> : IReadOnlyCollection<TView>, IEnumerable<TView>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
{
}
