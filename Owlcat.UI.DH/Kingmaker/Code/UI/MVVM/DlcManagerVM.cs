using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerVM : ViewModel, INetLobbyRequest, ISubscriber, INetInviteHandler
{
	private readonly List<DlcManagerMenuEntityVM> m_MenuEntitiesList = new List<DlcManagerMenuEntityVM>();

	private readonly ReactiveCommand<Unit> m_ChangeTab = new ReactiveCommand<Unit>();

	public readonly SelectionGroupRadioVM<DlcManagerMenuEntityVM> MenuSelectionGroup;

	private readonly ReactiveProperty<DlcManagerMenuEntityVM> m_SelectedMenuEntity = new ReactiveProperty<DlcManagerMenuEntityVM>();

	private readonly Action m_OnClose;

	private DlcManagerMenuEntityVM m_PreviousSelectedMenuEntity;

	public readonly bool InGame;

	public readonly bool IsConsole;

	private readonly ReactiveProperty<bool> m_IsModsWindow = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsSwitchOnDlcsWindow = new ReactiveProperty<bool>();

	public DlcManagerTabDlcsVM DlcsVM { get; }

	public DlcManagerTabModsVM ModsVM { get; }

	public DlcManagerTabSwitchOnDlcsVM SwitchOnDlcsVM { get; }

	public Observable<Unit> ChangeTab => m_ChangeTab;

	public ReadOnlyReactiveProperty<DlcManagerMenuEntityVM> SelectedMenuEntity => m_SelectedMenuEntity;

	public ReadOnlyReactiveProperty<bool> IsModsWindow => m_IsModsWindow;

	public ReadOnlyReactiveProperty<bool> IsSwitchOnDlcsWindow => m_IsSwitchOnDlcsWindow;

	public DlcManagerVM(Action closeAction, bool inGame = false)
	{
		m_OnClose = closeAction;
		InGame = inGame;
		IsConsole = false;
		if (!inGame)
		{
			DlcsVM = new DlcManagerTabDlcsVM().AddTo(this);
		}
		else
		{
			SwitchOnDlcsVM = new DlcManagerTabSwitchOnDlcsVM().AddTo(this);
		}
		if (!IsConsole)
		{
			ModsVM = new DlcManagerTabModsVM(!inGame).AddTo(this);
		}
		if (!inGame)
		{
			CreateMenuEntity(UIStrings.Instance.DlcManager.DlcManagerLabel, DlcsVM, OnDlcsMenuSelect);
		}
		else
		{
			CreateMenuEntity(UIStrings.Instance.DlcManager.DlcManagerLabel, SwitchOnDlcsVM, OnSwitchOnDlcsMenuSelect);
		}
		if (!IsConsole)
		{
			CreateMenuEntity(UIStrings.Instance.DlcManager.ModsLabel, ModsVM, OnModsMenuSelect);
		}
		MenuSelectionGroup = new SelectionGroupRadioVM<DlcManagerMenuEntityVM>(m_MenuEntitiesList, m_SelectedMenuEntity);
		m_SelectedMenuEntity.Value = m_MenuEntitiesList.FirstOrDefault();
		MenuSelectionGroup.AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	private void CreateMenuEntity(LocalizedString localizedString, DlcManagerTabBaseVM dlcManagerTab, Action callback)
	{
		DlcManagerMenuEntityVM item = new DlcManagerMenuEntityVM(localizedString, dlcManagerTab, callback).AddTo(this);
		m_MenuEntitiesList.Add(item);
	}

	private void OnDlcsMenuSelect()
	{
		CheckToChangeTab(delegate
		{
			if (!InGame)
			{
				DlcsVM.SetEnabled(value: true);
			}
			else
			{
				SwitchOnDlcsVM.SetEnabled(value: false);
				m_IsSwitchOnDlcsWindow.Value = false;
			}
			if (!IsConsole)
			{
				ModsVM.SetEnabled(value: false);
				m_IsModsWindow.Value = false;
			}
			m_ChangeTab.Execute(Unit.Default);
		});
	}

	private void OnModsMenuSelect()
	{
		CheckToChangeTab(delegate
		{
			if (!InGame)
			{
				DlcsVM.SetEnabled(value: false);
			}
			else
			{
				SwitchOnDlcsVM.SetEnabled(value: false);
				m_IsSwitchOnDlcsWindow.Value = false;
			}
			if (!IsConsole)
			{
				ModsVM.SetEnabled(value: true);
				m_IsModsWindow.Value = true;
			}
			m_ChangeTab.Execute(Unit.Default);
		});
	}

	private void OnSwitchOnDlcsMenuSelect()
	{
		CheckToChangeTab(delegate
		{
			if (!InGame)
			{
				DlcsVM.SetEnabled(value: false);
			}
			else
			{
				SwitchOnDlcsVM.SetEnabled(value: true);
				m_IsSwitchOnDlcsWindow.Value = true;
			}
			if (!IsConsole)
			{
				ModsVM.SetEnabled(value: false);
				m_IsModsWindow.Value = false;
			}
			m_ChangeTab.Execute(Unit.Default);
		});
	}

	public void OnClose()
	{
		CheckToReloadGame(delegate
		{
			m_OnClose?.Invoke();
		});
	}

	private void CheckToChangeTab(Action closeAction)
	{
		if (m_PreviousSelectedMenuEntity != SelectedMenuEntity.CurrentValue)
		{
			m_PreviousSelectedMenuEntity = SelectedMenuEntity.CurrentValue;
			CheckToReloadGame(closeAction);
		}
	}

	public void CheckToReloadGame(Action closeAction)
	{
		if (!IsConsole && ModsVM.CheckNeedToReloadGame() && IsModsWindow.CurrentValue)
		{
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.DlcManager.RestartChangeModConfirmation, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				RestartGame(button, closeAction);
			}, null, UIStrings.Instance.DlcManager.RestartGame, UIStrings.Instance.SettingsUI.DialogRevert);
		}
		else if (InGame && SwitchOnDlcsVM.CheckNeedToResaveGame() && IsSwitchOnDlcsWindow.CurrentValue)
		{
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.DlcManager.YouSwitchDlcOnAndCantDoItBack, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				ChangeDlcsStates(button, closeAction);
			}, null, null, UIStrings.Instance.SettingsUI.DialogRevert);
		}
		else
		{
			closeAction?.Invoke();
		}
	}

	public void RestoreAllToPreviousState()
	{
		if (!IsConsole && ModsVM.CheckNeedToReloadGame() && IsModsWindow.CurrentValue)
		{
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.DlcManager.ResetAllModsToPreviousState, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				if (button == DialogMessageBoxButton.Yes && !IsConsole && IsModsWindow.CurrentValue)
				{
					ModsVM.ResetModsCurrentState();
				}
			});
		}
		else
		{
			if (!InGame || !SwitchOnDlcsVM.CheckNeedToResaveGame() || !IsSwitchOnDlcsWindow.CurrentValue)
			{
				return;
			}
			UtilityMessageBox.ShowMessageBox(UIStrings.Instance.DlcManager.ResetAllDlcsToPreviousState, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				if (button == DialogMessageBoxButton.Yes && InGame && IsSwitchOnDlcsWindow.CurrentValue)
				{
					SwitchOnDlcsVM.ResetDlcsCurrentState();
				}
			});
		}
	}

	private void RestartGame(DialogMessageBoxButton button, Action closeBoxAction)
	{
		if (button == DialogMessageBoxButton.No)
		{
			if (!IsConsole)
			{
				ModsVM.ResetModsCurrentState();
			}
			closeBoxAction?.Invoke();
			return;
		}
		ModsVM.SetModsCurrentState();
		if (InGame)
		{
			Game.Instance.MakeQuickSave(delegate
			{
				SystemUtil.ApplicationRestart();
			});
		}
		else
		{
			SystemUtil.ApplicationRestart();
		}
	}

	private void ChangeDlcsStates(DialogMessageBoxButton button, Action closeBoxAction)
	{
		if (!InGame || IsModsWindow.CurrentValue || !IsSwitchOnDlcsWindow.CurrentValue)
		{
			return;
		}
		if (button == DialogMessageBoxButton.No)
		{
			if (InGame && !IsModsWindow.CurrentValue && IsSwitchOnDlcsWindow.CurrentValue)
			{
				SwitchOnDlcsVM.ResetDlcsCurrentState();
			}
			closeBoxAction?.Invoke();
		}
		else
		{
			SwitchOnDlcsVM.SetDlcsCurrentState();
			m_OnClose?.Invoke();
		}
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
		m_OnClose?.Invoke();
	}

	public void HandleNetLobbyClose()
	{
	}

	public void HandleInvite(Action<bool> callback)
	{
	}

	public void HandleInviteAccepted(bool accepted)
	{
		if (accepted)
		{
			m_OnClose?.Invoke();
		}
	}
}
