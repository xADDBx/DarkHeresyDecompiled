using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickItemCostView : BrickBaseView<BrickItemCostVM>
{
	[Header("Elements")]
	[SerializeField]
	private TextWithParent m_Left;

	[SerializeField]
	private TextWithParent m_Right;

	[SerializeField]
	private TextWithParent m_Additional;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private OwlcatMultiSelectable m_CostTypeSelectable;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Left.Text, m_Right.Text, m_Additional.Text).AddTo(this);
		}
		m_Left.Text.SetText(base.ViewModel.LeftText);
		m_Left.Container.SetActive(!string.IsNullOrEmpty(base.ViewModel.LeftText));
		m_Right.Text.SetText(base.ViewModel.RightText);
		m_Right.Container.SetActive(!string.IsNullOrEmpty(base.ViewModel.RightText));
		m_Additional.Text.SetText(base.ViewModel.AdditionalLine);
		m_Additional.Container.SetActive(!string.IsNullOrEmpty(base.ViewModel.AdditionalLine));
		m_CostTypeSelectable.SetActiveLayer(base.ViewModel.CostType.ToString());
		m_CostTypeSelectable.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
