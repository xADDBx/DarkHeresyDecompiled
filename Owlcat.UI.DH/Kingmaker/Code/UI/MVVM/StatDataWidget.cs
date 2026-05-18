using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class StatDataWidget : View<StatData>
{
	[Header("Elements")]
	[SerializeField]
	private TextValueTupleView m_Text;

	[ShowIf("m_HasIcon")]
	[SerializeField]
	private Image m_Icon;

	[ShowIf("m_HasIcon")]
	[SerializeField]
	private GameObject m_IconContainer;

	[Header("Selectables")]
	[SerializeField]
	private OwlcatMultiSelectable m_Comparison;

	[SerializeField]
	private OwlcatMultiSelectable m_Highlight;

	[Header("Values")]
	[SerializeField]
	private bool m_HasIcon;

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_Text.Bind(base.ViewModel.Text);
		if (m_HasIcon)
		{
			m_Icon.sprite = base.ViewModel.Icon;
			m_IconContainer.SetActive(base.ViewModel.Icon != null && m_HasIcon);
		}
		m_Comparison.gameObject.SetActive(base.ViewModel.Comparison != ComparisonResult.Equal);
		m_Comparison.SetActiveLayer(base.ViewModel.Comparison.ToString());
		m_Highlight.SetActiveLayer(base.ViewModel.Highlight.ToString());
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
