using Kingmaker.Blueprints.Root.Strings.GameLog;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TooltipBrickDamageNullifierView : TooltipBaseBrickView<TooltipBrickDamageNullifierVM>
{
	[SerializeField]
	private TextMeshProUGUI m_HeaderText;

	[SerializeField]
	private RollSlider m_RollSlider;

	[SerializeField]
	private TextMeshProUGUI m_ResultValueText;

	[SerializeField]
	private TextMeshProUGUI m_ReasonsText;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private ReasonBuffItemView m_ReasonBuffItemView;

	[SerializeField]
	private TextMeshProUGUI m_ResultText;

	protected override void OnBind()
	{
		m_HeaderText.text = GameLogStrings.Instance.TooltipBrickStrings.IncomingDamageNullifier;
		m_RollSlider.SetData(base.ViewModel.ChanceRoll, base.ViewModel.ResultRoll);
		TextMeshProUGUI resultValueText = m_ResultValueText;
		int resultValue = base.ViewModel.ResultValue;
		resultValueText.text = "=" + resultValue;
		m_ReasonsText.text = base.ViewModel.ReasonText;
		m_WidgetList.DrawEntries(base.ViewModel.ReasonBuffItems, m_ReasonBuffItemView).AddTo(this);
		m_ResultText.text = base.ViewModel.ResultText;
	}
}
