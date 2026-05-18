using Kingmaker.Blueprints.Root.Strings.GameLog;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickDamageNullifierView : BrickBaseView<BrickDamageNullifierVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_HeaderText;

	[SerializeField]
	private TMP_Text m_ResultValueText;

	[SerializeField]
	private TMP_Text m_ReasonsText;

	[SerializeField]
	private TMP_Text m_ResultText;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private RollSliderWidget RollSliderWidget;

	[SerializeField]
	private ReasonBuffItemWidget ReasonBuffItemWidget;

	protected override void OnBind()
	{
		m_HeaderText.text = GameLogStrings.Instance.TooltipBrickStrings.IncomingDamageNullifier;
		RollSliderWidget.Bind((base.ViewModel.ChanceRoll, base.ViewModel.ResultRoll));
		TMP_Text resultValueText = m_ResultValueText;
		int resultValue = base.ViewModel.ResultValue;
		resultValueText.text = "=" + resultValue;
		m_ReasonsText.text = base.ViewModel.ReasonText;
		m_WidgetList.DrawEntries(base.ViewModel.ReasonBuffItems, ReasonBuffItemWidget).AddTo(this);
		m_ResultText.text = base.ViewModel.ResultText;
	}
}
