using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class GameOverVM : ViewModel
{
	private SaveManager SaveManager => Game.Instance.SaveManager;

	private bool IsIronMan => SettingsRoot.Difficulty.OnlyOneSave;

	private bool HasDowngradedIronManSave => SaveManager.HasDowngradedIronManSave;

	public ModalWindowVM ModalWindowVM { get; }

	public GameOverVM()
	{
		UIGameOverScreen gameOverScreen = UIStrings.Instance.GameOverScreen;
		LocalizedString title = gameOverScreen.Title;
		if (IsIronMan)
		{
			ModalWindowVM = new ModalWindowVM(title, null, BuildIronManActions());
			return;
		}
		ModalWindowAction[] actions = new ModalWindowAction[3]
		{
			new ModalWindowAction
			{
				Name = gameOverScreen.QuickLoadLabel,
				Action = OnQuickLoad
			},
			new ModalWindowAction
			{
				Name = gameOverScreen.LoadLabel,
				Action = OnButtonLoadGame
			},
			new ModalWindowAction
			{
				Name = gameOverScreen.MainMenuLabel,
				Action = OnButtonMainMenu
			}
		};
		ModalWindowVM = new ModalWindowVM(title, null, actions);
		if (SaveManager.GetLatestSave() != null)
		{
			return;
		}
		ModalWindowVM.SetButtonEnabled(0, enabled: false);
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			if (SaveManager.GetLatestSave() != null)
			{
				ModalWindowVM.SetButtonEnabled(0, enabled: true);
			}
		}));
	}

	private ModalWindowAction[] BuildIronManActions()
	{
		UIGameOverScreen gameOverScreen = UIStrings.Instance.GameOverScreen;
		if (!HasDowngradedIronManSave)
		{
			return new ModalWindowAction[1]
			{
				new ModalWindowAction
				{
					Name = gameOverScreen.MainMenuLabel,
					Action = OnButtonMainMenu
				}
			};
		}
		return new ModalWindowAction[2]
		{
			new ModalWindowAction
			{
				Name = gameOverScreen.IronManDeleteSaveLabel,
				Action = OnIronManDeleteSave
			},
			new ModalWindowAction
			{
				Name = gameOverScreen.IronManContinueGameLabel,
				Action = OnIronManContinueGame
			}
		};
	}

	private void OnButtonLoadGame()
	{
		EventBus.RaiseEvent(delegate(ISaveLoadUIHandler h)
		{
			h.HandleOpenSaveLoad(SaveLoadMode.Load, singleMode: true);
		});
	}

	private void OnQuickLoad()
	{
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			SaveInfo latestSave = SaveManager.GetLatestSave();
			if (latestSave != null)
			{
				Game.Instance.LoadGame(latestSave);
			}
		}));
	}

	private void OnButtonMainMenu()
	{
		Game.Instance.ResetToMainMenu();
	}

	private void OnIronManDeleteSave()
	{
		if (HasDowngradedIronManSave)
		{
			SaveManager.DeleteDowngradedIronManSave();
		}
		OnButtonMainMenu();
	}

	private void OnIronManContinueGame()
	{
		if (HasDowngradedIronManSave)
		{
			SaveManager.LoadDowngradedIronManSave();
		}
	}
}
