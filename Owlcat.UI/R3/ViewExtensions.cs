using System;
using Owlcat.UI;

namespace R3;

public static class ViewExtensions
{
	public static T AddTo<T, TSource>(this T disposable, View<TSource> view) where T : IDisposable
	{
		view.Add(disposable);
		return disposable;
	}

	public static IDisposable SubscribeToView<TViewModel>(this TViewModel data, IBindable<TViewModel> bindable) where TViewModel : ViewModel
	{
		bindable.Bind(data);
		return new DisposableAction(bindable.Unbind);
	}

	public static IDisposable SubscribeToView<T>(this Observable<T> source, IBindable<T> view)
	{
		return new CompositeDisposable(2)
		{
			source.Subscribe(view, Bind),
			Disposable.Create(view.Unbind)
		};
	}

	private static void Bind<T>(T source, IBindable<T> view)
	{
		view.Bind(source);
	}
}
