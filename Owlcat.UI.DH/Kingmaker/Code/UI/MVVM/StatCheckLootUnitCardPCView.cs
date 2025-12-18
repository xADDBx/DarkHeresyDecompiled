using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class StatCheckLootUnitCardPCView : StatCheckLootUnitCardBaseView
{
	[SerializeField]
	private OwlcatButton m_CheckStatButton;

	[SerializeField]
	private TextMeshProUGUI m_CheckStatButtonLabel;

	[SerializeField]
	private OwlcatButton m_SwitchUnitButton;

	protected override void InitializeImpl()
	{
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_CheckStatButton.OnLeftClickAsObservable().Subscribe(base.OnCheckStat).AddTo(this);
		m_SwitchUnitButton.OnLeftClickAsObservable().Subscribe(base.OnSwitchUnit).AddTo(this);
		m_CheckStatButton.OnConfirmClickAsObservable().Subscribe(base.OnCheckStat).AddTo(this);
		m_SwitchUnitButton.OnConfirmClickAsObservable().Subscribe(base.OnSwitchUnit).AddTo(this);
	}
}
