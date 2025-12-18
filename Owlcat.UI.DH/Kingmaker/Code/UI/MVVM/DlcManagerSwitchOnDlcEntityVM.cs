using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerSwitchOnDlcEntityVM : SelectionGroupEntityVM
{
	public readonly string Title;

	public readonly BlueprintDlc BlueprintDlc;

	private readonly ReactiveProperty<bool> m_DlcSwitchState = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_WarningResaveGame = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_NeedSwitchOnWarning = new ReactiveProperty<bool>();

	public readonly bool IsSaveAllowed;

	private readonly ReactiveCommand<bool> m_CheckModNeedToResaveCommand;

	public readonly bool ItIsLateToSwitchDlcOn;

	public readonly string ToLateReason;

	public ReadOnlyReactiveProperty<bool> DlcSwitchState => m_DlcSwitchState;

	public ReadOnlyReactiveProperty<bool> WarningResaveGame => m_WarningResaveGame;

	public ReadOnlyReactiveProperty<bool> NeedSwitchOnWarning => m_NeedSwitchOnWarning;

	public DlcManagerSwitchOnDlcEntityVM(BlueprintDlc blueprintDlc, ReactiveCommand<bool> checkModNeedToResaveCommand)
		: base(allowSwitchOff: false)
	{
		BlueprintDlc = blueprintDlc;
		Title = blueprintDlc.GetDlcName();
		m_CheckModNeedToResaveCommand = checkModNeedToResaveCommand;
		SetTempDlcState(GetActualDlcState());
		IsSaveAllowed = !LoadingProcess.Instance.IsLoadingInProcess && Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Manual);
		if (ItIsLateToSwitchDlcOn = BlueprintDlc.CheckIsLateToSwitch())
		{
			ToLateReason = BlueprintDlc.ToLateReason;
		}
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
				h.HandleShowSettingsDescription(null, Title ?? "", BlueprintDlc.DlcDescription ?? "");
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

	public bool GetActualDlcState()
	{
		return BlueprintDlc?.GetDlcSwitchOnOffState() ?? false;
	}

	public void SetActualDlcState()
	{
	}

	public void ResetTempDlcState()
	{
		SetTempDlcState(GetActualDlcState());
	}

	public void ChangeValue()
	{
		if (IsSaveAllowed && !GetActualDlcState() && !ItIsLateToSwitchDlcOn)
		{
			SetTempDlcState(!DlcSwitchState.CurrentValue);
			return;
		}
		string message = ((ItIsLateToSwitchDlcOn && !GetActualDlcState()) ? ToLateReason : ((string)((!IsSaveAllowed && !GetActualDlcState()) ? UIStrings.Instance.DlcManager.CannotChangeDlcSwitchStateRightNowBecauseSaveNotAllowed : UIStrings.Instance.DlcManager.CannotChangeDlcSwitchState)));
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(message, addToLog: true, WarningNotificationFormat.Attention);
		});
	}

	private void SetTempDlcState(bool state)
	{
		m_DlcSwitchState.Value = state;
		bool flag = state != GetActualDlcState();
		m_WarningResaveGame.Value = flag;
		m_CheckModNeedToResaveCommand.Execute(flag);
	}

	public void SelectMe()
	{
		DoSelectMe();
	}

	protected override void DoSelectMe()
	{
	}
}
