using System;
using Kingmaker.Blueprints.Root.Strings;
using R3;
using R3.Triggers;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerSwitchOnDlcEntityPCView : DlcManagerSwitchOnDlcEntityBaseView
{
	private IDisposable m_Disposable;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_MultiButton.OnPointerClickAsObservable().Subscribe(delegate
		{
			SwitchValue();
		}));
		if (base.ViewModel.ItIsLateToSwitchDlcOn)
		{
			AddDisposable(m_MultiButton.SetHint(base.ViewModel.ToLateReason));
			return;
		}
		if (!base.ViewModel.IsSaveAllowed)
		{
			AddDisposable(m_MultiButton.SetHint(base.ViewModel.GetActualDlcState() ? UIStrings.Instance.DlcManager.CannotChangeDlcSwitchState : UIStrings.Instance.DlcManager.CannotChangeDlcSwitchStateRightNowBecauseSaveNotAllowed));
			return;
		}
		if (base.ViewModel.GetActualDlcState())
		{
			AddDisposable(m_MultiButton.SetHint(UIStrings.Instance.DlcManager.CannotChangeDlcSwitchState));
			return;
		}
		AddDisposable(base.ViewModel.DlcSwitchState.Subscribe(delegate(bool value)
		{
			SetWillTakeAffectHint(m_MultiButton, value);
		}));
		SetWillTakeAffectHint(m_MultiButton, base.ViewModel.DlcSwitchState.CurrentValue);
	}

	protected override void DestroyViewImplementation()
	{
		m_Disposable?.Dispose();
		m_Disposable = null;
		base.DestroyViewImplementation();
	}

	private void SetWillTakeAffectHint(MonoBehaviour selectable, bool state)
	{
		m_Disposable?.Dispose();
		m_Disposable = null;
		string text = UIStrings.Instance.DlcManager.ThisSettingWillAffectCurrentSave;
		bool shouldShow = !state;
		m_Disposable = selectable.SetHint(text, null, default(Color), shouldShow);
	}
}
