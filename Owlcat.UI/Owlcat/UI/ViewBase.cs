using System;

namespace Owlcat.UI;

[Obsolete("Use View instead")]
public abstract class ViewBase<TViewModel> : View<TViewModel>, IHasViewModel where TViewModel : class
{
	public bool IsBinded => base.ViewModel != null;

	public IViewModel GetViewModel()
	{
		return (IViewModel)base.ViewModel;
	}

	protected void AddDisposable(IDisposable disposable)
	{
		Add(disposable);
	}

	protected bool RemoveDisposable(IDisposable disposable)
	{
		return Remove(disposable);
	}

	protected sealed override void OnBind()
	{
		if (base.ViewModel is BaseDisposable baseDisposable)
		{
			baseDisposable.OnDispose += Unbind;
		}
		BindViewImplementation();
	}

	protected sealed override void OnUnbind()
	{
		if (base.ViewModel is BaseDisposable baseDisposable)
		{
			baseDisposable.OnDispose -= Unbind;
		}
		DestroyViewImplementation();
	}

	protected abstract void BindViewImplementation();

	protected abstract void DestroyViewImplementation();
}
