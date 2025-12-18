using System;
using System.Runtime.CompilerServices;

namespace ObservableCollections;

public class SynchronizedViewFilter<T, TView> : ISynchronizedViewFilter<T, TView>
{
	private class NullViewFilter : ISynchronizedViewFilter<T, TView>
	{
		public bool IsMatch(T value, TView view)
		{
			return true;
		}

		public void WhenFalse(T value, TView view)
		{
		}

		public void WhenTrue(T value, TView view)
		{
		}

		public void OnCollectionChanged(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
		{
		}

		void ISynchronizedViewFilter<T, TView>.OnCollectionChanged(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
		{
			OnCollectionChanged(in eventArgs);
		}
	}

	[CompilerGenerated]
	private Func<T, TView, bool> _003CisMatch_003EP;

	[CompilerGenerated]
	private Action<T, TView>? _003CwhenTrue_003EP;

	[CompilerGenerated]
	private Action<T, TView>? _003CwhenFalse_003EP;

	[CompilerGenerated]
	private Action<SynchronizedViewChangedEventArgs<T, TView>>? _003ConCollectionChanged_003EP;

	public static readonly ISynchronizedViewFilter<T, TView> Null = new NullViewFilter();

	public SynchronizedViewFilter(Func<T, TView, bool> isMatch, Action<T, TView>? whenTrue, Action<T, TView>? whenFalse, Action<SynchronizedViewChangedEventArgs<T, TView>>? onCollectionChanged)
	{
		_003CisMatch_003EP = isMatch;
		_003CwhenTrue_003EP = whenTrue;
		_003CwhenFalse_003EP = whenFalse;
		_003ConCollectionChanged_003EP = onCollectionChanged;
		base._002Ector();
	}

	public bool IsMatch(T value, TView view)
	{
		return _003CisMatch_003EP(value, view);
	}

	public void WhenFalse(T value, TView view)
	{
		_003CwhenFalse_003EP?.Invoke(value, view);
	}

	public void WhenTrue(T value, TView view)
	{
		_003CwhenTrue_003EP?.Invoke(value, view);
	}

	public void OnCollectionChanged(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
	{
		_003ConCollectionChanged_003EP?.Invoke(eventArgs);
	}

	void ISynchronizedViewFilter<T, TView>.OnCollectionChanged(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
	{
		OnCollectionChanged(in eventArgs);
	}
}
