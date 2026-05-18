using System;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Services;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Init;

internal static class RootUIContextInit
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void RegisterRootUIContext()
	{
		RootUIContextService.RootUIContextFactory = () => new RootUIContext();
		ObservableSystem.RegisterUnhandledExceptionHandler(delegate(Exception e)
		{
			PFLog.UI.Exception(e);
		});
	}
}
