using System;
using Code.GameCore.Modding;
using Code.Utility.ExtendedModInfo;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerModEntityVM : SelectionGroupEntityVM
{
	private readonly ReactiveProperty<bool> m_ModSwitchState = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_WarningUpdateMod = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_WarningReloadGame = new ReactiveProperty<bool>();

	public readonly bool IsSaveAllowed;

	private readonly ReactiveCommand<bool> m_CheckModNeedToReloadCommand;

	private readonly ReactiveProperty<bool> m_ModSettingsAvailable = new ReactiveProperty<bool>();

	public readonly ExtendedModInfo ModInfo;

	public ReadOnlyReactiveProperty<bool> ModSwitchState => m_ModSwitchState;

	public ReadOnlyReactiveProperty<bool> WarningUpdateMod => m_WarningUpdateMod;

	public ReadOnlyReactiveProperty<bool> WarningReloadGame => m_WarningReloadGame;

	public ReadOnlyReactiveProperty<bool> ModSettingsAvailable => m_ModSettingsAvailable;

	public DlcManagerModEntityVM(ExtendedModInfo modInfo, bool isMainMenu, ReactiveCommand<bool> checkModNeedToReloadCommand)
		: base(allowSwitchOff: false)
	{
		ModInfo = modInfo;
		m_CheckModNeedToReloadCommand = checkModNeedToReloadCommand;
		m_ModSettingsAvailable.Value = modInfo.HasSettings;
		m_WarningUpdateMod.Value = modInfo.UpdateRequired;
		SetTempModState(GetActualModState());
		IsSaveAllowed = !LoadingProcess.Instance.IsLoadingInProcess && Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Manual, isMainMenu);
	}

	protected override void DisposeImplementation()
	{
	}

	public void ShowDescription(bool state)
	{
		if (state)
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleShowSettingsDescription(null, ModInfo.DisplayName + Environment.NewLine + ModInfo.Author + " - " + ModInfo.Version, ModInfo.Description ?? "");
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleHideSettingsDescription();
			});
		}
	}

	private bool GetActualModState()
	{
		return ModInfo.Enabled;
	}

	public void SetActualModState()
	{
		ModInitializer.EnableMod(ModInfo.Id, ModSwitchState.CurrentValue, forceUpdate: true);
	}

	public void ResetTempModState()
	{
		SetTempModState(GetActualModState());
	}

	public void ChangeValue()
	{
		if (IsSaveAllowed)
		{
			SetTempModState(!ModSwitchState.CurrentValue);
			return;
		}
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(UIStrings.Instance.DlcManager.CannotChangeModSwitchState, addToLog: true, WarningNotificationFormat.Attention);
		});
	}

	private void SetTempModState(bool state)
	{
		m_ModSwitchState.Value = state;
		bool flag = state != GetActualModState();
		m_WarningReloadGame.Value = flag;
		m_CheckModNeedToReloadCommand.Execute(flag);
	}

	public void SelectMe()
	{
		DoSelectMe();
	}

	protected override void DoSelectMe()
	{
	}

	public void OpenModSettings()
	{
		ModInitializer.OpenModInfoWindow(ModInfo.Id);
	}
}
