namespace ObservableCollections;

public interface ISynchronizedViewFilter<T, TView>
{
	bool IsMatch(T value, TView view);

	void WhenTrue(T value, TView view);

	void WhenFalse(T value, TView view);

	void OnCollectionChanged(in SynchronizedViewChangedEventArgs<T, TView> eventArgs);
}
