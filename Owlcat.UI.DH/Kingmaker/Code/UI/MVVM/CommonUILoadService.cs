using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Code.UI.MVVM;

public class CommonUILoadService : IUILoadService
{
	private readonly Action m_DisposeAction;

	private bool m_EnterGameStarted;

	public CommonUILoadService(Action disposeAction)
	{
		m_DisposeAction = disposeAction;
	}

	public void Load(SaveInfo saveInfo)
	{
		Action load = delegate
		{
			m_DisposeAction?.Invoke();
			Game.Instance.LoadGame(saveInfo);
		};
		if (Game.Instance.CurrentModeType == GameModeType.GameOver)
		{
			load();
			return;
		}
		if (GameUIState.Instance.IsInMainMenu)
		{
			throw new InvalidOperationException("You should not use CommonUILoadService in main menu");
		}
		string loadWarning = string.Format(UIStrings.Instance.SaveLoadTexts.LoadSaveWarning, saveInfo.Name);
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
		{
			h.HandleOpen(loadWarning, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton buttonPressed)
			{
				if (buttonPressed == DialogMessageBoxButton.Yes)
				{
					load();
				}
			});
		});
	}
}
