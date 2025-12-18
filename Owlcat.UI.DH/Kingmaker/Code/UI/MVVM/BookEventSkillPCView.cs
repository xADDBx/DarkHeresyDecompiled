using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventSkillPCView : View<CharInfoStatVM>
{
	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_HighlightedValue;

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		OnChangedValue();
	}

	public void Highlight()
	{
		m_Value.gameObject.SetActive(value: false);
		m_HighlightedValue.gameObject.SetActive(value: true);
		m_HighlightedValue.text = UIUtilityText.AddSign(base.ViewModel.StatValue.CurrentValue);
	}

	private void OnChangedValue()
	{
		if (!(m_Value.gameObject == null))
		{
			m_Value.gameObject.SetActive(value: true);
			m_HighlightedValue.gameObject.SetActive(value: false);
			m_Value.text = UIUtilityText.AddSign(base.ViewModel?.StatValue.CurrentValue);
		}
	}

	public void SetSelected(bool state)
	{
		m_Background.gameObject.SetActive(state);
	}
}
