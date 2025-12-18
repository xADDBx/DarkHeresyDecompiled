using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Code.UI.MVVM;

[Obsolete("Delete Me!")]
public class SettingsEntityStatisticsOptOutVM : SettingsEntityVM
{
	public SettingsEntityStatisticsOptOutVM()
		: base(null)
	{
	}

	public void OpenSettingsInBrowser()
	{
	}

	public void DeleteData()
	{
	}

	public void DeleteStatisticsData()
	{
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(UIStrings.Instance.SettingsUI.DeleteStatisticsDataDialogue, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton buttonPressed)
			{
				if (buttonPressed == DialogMessageBoxButton.Yes)
				{
					DeleteData();
				}
			});
		});
	}
}
