using Code.View.UI.MVVM.Tooltip.Bricks.Items;
using Kingmaker.Code.UI.MVVM;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.View.UI.MVVM.Tooltip.Bricks;

public class StatDataWidget : View<StatData>
{
	[Header("Elements")]
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private GameObject m_IconContainer;

	[SerializeField]
	private OwlcatMultiSelectable m_Comparison;

	[SerializeField]
	private OwlcatMultiSelectable m_Highlight;

	public TMP_Text[] GetTexts()
	{
		return new TMP_Text[2] { m_Label, m_Value };
	}

	protected override void OnBind()
	{
		m_Label.text = base.ViewModel.Label;
		m_Value.text = base.ViewModel.Value;
		m_Icon.sprite = base.ViewModel.Icon;
		m_IconContainer.SetActive(base.ViewModel.Icon != null);
		m_Comparison.gameObject.SetActive(base.ViewModel.Comparison != ComparisonResult.Equal);
		m_Comparison.SetActiveLayer(base.ViewModel.Comparison.ToString());
		m_Highlight.SetActiveLayer(base.ViewModel.Highlight.ToString());
	}
}
