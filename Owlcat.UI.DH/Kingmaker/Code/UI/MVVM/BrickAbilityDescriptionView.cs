using Code.View.UI.Helpers;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityDescriptionView : BrickBaseView<BrickAbilityDescriptionVM>
{
	[SerializeField]
	private TMP_Text m_Text;

	[SerializeField]
	private Image m_Icon;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text).AddTo(this);
		}
		base.OnBind();
		m_Text.SetText(base.ViewModel.Text);
		m_Icon.gameObject.SetActive(base.ViewModel.HasIcon);
		m_TextHelper.UpdateTextSize();
		if (base.ViewModel.HasIcon)
		{
			m_Icon.sprite = base.ViewModel.Icon;
			m_Icon.color = base.ViewModel.Color;
		}
	}
}
