using System;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.GameModes;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public abstract class HUDNotificationNewVM : ViewModel
{
	public Action OnNotificationShown { get; protected set; }

	public abstract bool ShouldShow { get; }

	public bool CanShowButton()
	{
		bool num = RootUIContext.Instance.FullScreenUIType == FullScreenUIType.Journal;
		bool flag = Game.Instance.CurrentModeType == GameModeType.Cutscene;
		bool flag2 = Game.Instance.CurrentModeType == GameModeType.Dialog;
		if (!num && !flag)
		{
			return !flag2;
		}
		return false;
	}
}
