using System;
using Owlcat.UI;

namespace R3;

public static class ViewModelExtensions
{
	public static T AddTo<T>(this T disposable, ViewModel viewModel) where T : IDisposable
	{
		viewModel.Add(disposable);
		return disposable;
	}
}
