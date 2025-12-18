using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class StatCheckLootUnitsPagePCView : StatCheckLootUnitsPageBaseView<StatCheckLootUnitCardPCView, StatCheckLootSmallUnitCardPCView>
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_BackWithoutConfirmUnitButton;

	[SerializeField]
	private OwlcatButton m_ConfirmUnitButton;

	[SerializeField]
	private TextMeshProUGUI m_ConfirmUnitButtonLabel;

	protected override void InitializeImpl()
	{
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_BackWithoutConfirmUnitButton.OnLeftClickAsObservable().Subscribe(base.OnBackWithoutConfirmUnit).AddTo(this);
		m_ConfirmUnitButton.OnLeftClickAsObservable().Subscribe(base.OnConfirmUnit).AddTo(this);
		m_BackWithoutConfirmUnitButton.OnConfirmClickAsObservable().Subscribe(base.OnBackWithoutConfirmUnit).AddTo(this);
		m_ConfirmUnitButton.OnConfirmClickAsObservable().Subscribe(base.OnConfirmUnit).AddTo(this);
	}
}
