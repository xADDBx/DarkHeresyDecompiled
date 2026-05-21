using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class PartyStatsOverviewCharacterView : View<PartyStatsOverviewCharacterVM>
{
	[SerializeField]
	private Image m_PortraitImage;

	[SerializeField]
	private TextMeshProUGUI m_NameText;

	[SerializeField]
	private TextMeshProUGUI m_ValueText;

	protected override void OnBind()
	{
		base.OnBind();
		if (m_PortraitImage != null)
		{
			m_PortraitImage.sprite = base.ViewModel.Portrait;
			m_PortraitImage.enabled = base.ViewModel.Portrait != null;
		}
		if (m_NameText != null)
		{
			m_NameText.text = base.ViewModel.Name;
		}
		if (m_ValueText != null)
		{
			TextMeshProUGUI valueText = m_ValueText;
			int statValue = base.ViewModel.StatValue;
			valueText.text = statValue.ToString();
		}
	}
}
