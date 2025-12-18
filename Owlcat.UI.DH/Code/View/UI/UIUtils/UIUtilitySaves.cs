using System;
using System.Collections;
using Kingmaker;
using R3;

namespace Code.View.UI.UIUtils;

public static class UIUtilitySaves
{
	public static IEnumerator WaitForSaveUpdated(Action finishAction)
	{
		while (!Game.Instance.SaveManager.AreSavesUpToDate)
		{
			PFLog.UI.Log("[WaitForSaveUpdated] waiting for saves...");
			yield return null;
		}
		finishAction();
	}

	public static IDisposable DoWhenSavesUpdated(Action finishAction)
	{
		return Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate).TakeWhile((Unit _) => !Game.Instance.SaveManager.AreSavesUpToDate).Subscribe(delegate
		{
			PFLog.UI.Log("[WaitForSaveUpdated] waiting for saves...");
		}, delegate
		{
			finishAction?.Invoke();
		});
	}
}
