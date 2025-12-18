using System;
using System.Text;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Utility;
using Kingmaker.Visual.Sound;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class EscMenuVM : ViewModel, IGameModeHandler, ISubscriber, INetLobbyRequest
{
	private readonly Action m_CloseAction;

	private readonly ReactiveProperty<bool> m_IsInSpace = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsInCutscene = new ReactiveProperty<bool>();

	public bool InternalWindowOpened;

	private readonly ReactiveCommand<Unit> m_UpdateButtonsInteractable = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_UpdateButtonsFocus = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<string> m_AreaName = new ReactiveProperty<string>();

	public bool IsSavingAllowed { get; private set; }

	public bool IsOptionsAllowed { get; private set; }

	public bool IsFormationAllowed { get; private set; }

	public ReadOnlyReactiveProperty<bool> IsInSpace => m_IsInSpace;

	public ReadOnlyReactiveProperty<bool> IsInCutscene => m_IsInCutscene;

	public Observable<Unit> UpdateButtonsInteractable => m_UpdateButtonsInteractable;

	public Observable<Unit> UpdateButtonsFocus => m_UpdateButtonsFocus;

	public ReadOnlyReactiveProperty<string> AreaName => m_AreaName;

	public EscMenuVM(Action closeAction)
	{
		m_IsInSpace.Value = Game.Instance.IsModeActive(GameModeType.SpaceCombat) || Game.Instance.IsModeActive(GameModeType.StarSystem) || Game.Instance.IsModeActive(GameModeType.GlobalMap);
		m_IsInCutscene.Value = Game.Instance.IsModeActive(GameModeType.Cutscene);
		IsOptionsAllowed = !IsInCutscene.CurrentValue;
		IsFormationAllowed = !IsInCutscene.CurrentValue && !Game.Instance.IsModeActive(GameModeType.Dialog);
		m_CloseAction = closeAction;
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			IsSavingAllowed = Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Manual) && !SettingsRoot.Difficulty.OnlyOneSave;
		}));
		m_AreaName.Value = Game.Instance.State.LoadedAreaState.Area.Blueprint.AreaDisplayName;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void OnClose()
	{
		m_CloseAction?.Invoke();
	}

	public void OnSave()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(ISaveLoadUIHandler h)
		{
			h.HandleOpenSaveLoad(SaveLoadMode.Save, singleMode: false);
		});
	}

	public void OnLoad()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(ISaveLoadUIHandler h)
		{
			h.HandleOpenSaveLoad(SaveLoadMode.Load, !IsSavingAllowed);
		});
	}

	public void OnQuickSave()
	{
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			m_CloseAction?.Invoke();
			Game.Instance.MakeQuickSave();
		}));
	}

	public void OnQuickLoad()
	{
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			OnClose();
			Game.Instance.QuickLoadGame();
		}));
	}

	public void OpenFormation()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
		{
			h.HandleOpenFormation();
		});
	}

	public void OnSettings()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(ISettingsUIHandler h)
		{
			h.HandleOpenSettings();
		});
	}

	public void OnMods()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(IDlcManagerUIHandler h)
		{
			h.HandleOpenDlcManager(inGame: true);
		});
	}

	public void OnBugReport()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportOpen(IsInCutscene.CurrentValue);
		});
	}

	public void OnMainMenu()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(UIStrings.Instance.CommonTexts.QuitToMainMenuLabel);
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			UtilityMessageBox.ShowMessageBox(stringBuilder.ToString(), DialogMessageBoxType.Dialog, OnQuitToMainMenuAction);
			return;
		}
		stringBuilder.Append(" ");
		stringBuilder.Append(UIStrings.Instance.CommonTexts.ProgressWillBeLost);
		UtilityMessageBox.ShowMessageBox(stringBuilder.ToString(), DialogMessageBoxType.Dialog, OnQuitToMainMenuAction);
	}

	public void TriggerUpdateButtonsFocus()
	{
		m_UpdateButtonsFocus.Execute();
	}

	private void OnQuitToMainMenuAction(DialogMessageBoxButton button)
	{
		if (button != DialogMessageBoxButton.Yes)
		{
			return;
		}
		OnClose();
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
			{
				LoadingProcess.Instance.StartLoadingProcess("SaveRoutine (IronMan)", Game.Instance.SaveManager.SaveRoutine(Game.Instance.SaveManager.GetNextAutoslot(), forceAuto: true), delegate
				{
					Game.Instance.ResetToMainMenu();
				}, LoadingProcessTag.Save);
			}));
		}
		else
		{
			Game.Instance.ResetToMainMenu();
		}
	}

	public void OnQuit()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(UIStrings.Instance.CommonTexts.QuitToDesctopLabel);
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			UtilityMessageBox.ShowMessageBox(stringBuilder.ToString(), DialogMessageBoxType.Dialog, OnQuitAction);
			return;
		}
		stringBuilder.Append(" ");
		stringBuilder.Append(UIStrings.Instance.CommonTexts.ProgressWillBeLost);
		UtilityMessageBox.ShowMessageBox(stringBuilder.ToString(), DialogMessageBoxType.Dialog, OnQuitAction);
	}

	private void OnQuitAction(DialogMessageBoxButton button)
	{
		SoundState.Instance.MusicStateHandler.StartMusicStopEvent();
		if (button != DialogMessageBoxButton.Yes)
		{
			return;
		}
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
			{
				LoadingProcess.Instance.StartLoadingProcess("SaveRoutine (On quit)", Game.Instance.SaveManager.SaveRoutine(Game.Instance.SaveManager.GetNextAutoslot(), forceAuto: true), SystemUtil.ApplicationQuit, LoadingProcessTag.Save);
			}));
		}
		else
		{
			SystemUtil.ApplicationQuit();
		}
	}

	public void OnMultiplayer()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(INetLobbyRequest h)
		{
			h.HandleNetLobbyRequest();
		});
	}

	public void OnMultiplayerRoles()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(INetRolesRequest h)
		{
			h.HandleNetRolesRequest();
		});
	}

	public void HandleBeginTrading()
	{
		OnClose();
	}

	public void HandleEndTrading()
	{
	}

	public void HandleVendorAboutToTrading()
	{
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		m_IsInSpace.Value = Game.Instance.IsModeActive(GameModeType.SpaceCombat) || Game.Instance.IsModeActive(GameModeType.StarSystem) || Game.Instance.IsModeActive(GameModeType.GlobalMap);
		m_IsInCutscene.Value = Game.Instance.IsModeActive(GameModeType.Cutscene);
		IsOptionsAllowed = !IsInCutscene.CurrentValue;
		IsFormationAllowed = !IsInCutscene.CurrentValue && !Game.Instance.IsModeActive(GameModeType.Dialog);
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			IsSavingAllowed = Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Manual) && !SettingsRoot.Difficulty.OnlyOneSave;
		}));
		m_UpdateButtonsInteractable.Execute();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
		InternalWindowOpened = true;
		OnClose();
	}

	public void HandleNetLobbyClose()
	{
	}
}
