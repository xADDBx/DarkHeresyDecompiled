using System;
using R3;

namespace Owlcat.UI;

[Obsolete("Use ViewModel instead")]
public abstract class VMBase : ViewModel, IBaseDisposable, IDisposable, IViewModel
{
	public new event Action OnDispose;

	public VMBase()
	{
		Add(new DisposableAction(delegate
		{
			this.OnDispose?.Invoke();
		}));
		Add(new DisposableAction(DisposeImplementation));
	}

	protected abstract void DisposeImplementation();

	protected void AddDisposable(IDisposable disposable)
	{
		Add(disposable);
	}

	protected void AddDisposable(Action action)
	{
		Add(new DisposableAction(action));
	}

	protected void DisposeAndRemove<T>(T disposable) where T : IDisposable
	{
		disposable.Dispose();
	}

	protected T AddDisposableAndReturn<T>(T disposable) where T : IDisposable
	{
		return disposable.AddTo(this);
	}

	public void DestroyViewRecursive()
	{
		Dispose();
	}
}
