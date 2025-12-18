using Code.View.UI.Helpers;
using Code.View.UI.MVVM.Tooltip.Bricks.Items;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.View.UI.MVVM.Tooltip.Bricks;

public class StatWidget : View<StatData>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Value;

	[SerializeField]
	private TMP_Text m_Label;

	[SerializeField]
	[ShowIf("m_HasIcon")]
	private Image m_Icon;

	[SerializeField]
	[ShowIf("m_HasIcon")]
	private GameObject m_IconContainer;

	[SerializeField]
	private OwlcatMultiSelectable m_ComparisonSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_HighlightSelectable;

	[Header("Values")]
	[SerializeField]
	private bool m_HasIcon;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		m_TextHelper = new AccessibilityTextHelper(m_Value, m_Label).AddTo(this);
		m_Value.text = base.ViewModel.Value;
		m_Label.text = base.ViewModel.Label;
		if (m_HasIcon)
		{
			m_Icon.sprite = base.ViewModel.Icon;
			m_IconContainer.SetActive(base.ViewModel.Icon != null);
		}
		m_ComparisonSelectable.gameObject.SetActive(base.ViewModel.Comparison != ComparisonResult.Equal);
		m_ComparisonSelectable.SetActiveLayer(base.ViewModel.Comparison.ToString());
		m_HighlightSelectable.SetActiveLayer(base.ViewModel.Highlight.ToString());
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_TextHelper = null;
	}
}
