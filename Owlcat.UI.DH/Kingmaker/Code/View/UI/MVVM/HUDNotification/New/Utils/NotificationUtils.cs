using System;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.GameConst;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New.Utils;

public static class NotificationUtils
{
	public static float Time => UIConsts.QuestNotificationTime / (float)((!Game.Instance.Controllers.TurnController.TurnBasedModeActive) ? 1 : 2);

	public static IDisposable DoActionAfterDelay(float second, Action action)
	{
		return ObservableSubscribeExtensions.Subscribe(Observable.Timer(TimeSpan.FromSeconds(second), UnityTimeProvider.UpdateIgnoreTimeScale), delegate
		{
			action?.Invoke();
		});
	}

	public static void OpenDetectiveJournal(BlueprintCase blueprintCase, BlueprintClue clueToFocus = null)
	{
		if (!(Game.Instance.CurrentModeType == GameModeType.GameOver))
		{
			EventBus.RaiseEvent(delegate(IDetectiveJournalUIHandler h)
			{
				h.HandleOpenDetectiveJournal(blueprintCase, clueToFocus);
			});
		}
	}

	public static void OpenJournal()
	{
		if (!(Game.Instance.CurrentModeType == GameModeType.GameOver))
		{
			EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
			{
				h.HandleOpenJournal();
			});
		}
	}
}
