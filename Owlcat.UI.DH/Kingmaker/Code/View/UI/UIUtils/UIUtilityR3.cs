using System;
using R3;

namespace Kingmaker.Code.View.UI.UIUtils;

public static class UIUtilityR3
{
	public static void ClearDisposableValue<T>(this ReactiveProperty<T> vm) where T : IDisposable
	{
		if (vm == null)
		{
			return;
		}
		IDisposable disposable = vm.CurrentValue;
		if (disposable == null)
		{
			return;
		}
		vm.Value = default(T);
		try
		{
			disposable.Dispose();
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
		}
	}
}
