using Code.View.UI.Helpers;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityTextIconView : BrickBaseView<BrickAbilityTextIconVM>
{
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private Image m_Icon;

	protected override void OnBind()
	{
		base.OnBind();
		m_TextHelper = new AccessibilityTextHelper(m_Title, m_Description).AddTo(this);
		m_Title.SetText(base.ViewModel.Title);
		m_Title.SetLinkTooltip(null, null, base.ViewModel.TooltipConfig);
		bool flag = !string.IsNullOrEmpty(base.ViewModel.Description);
		m_Description.gameObject.SetActive(flag);
		if (flag)
		{
			m_Description.SetText(base.ViewModel.Description);
			m_Title.SetLinkTooltip(null, null, base.ViewModel.TooltipConfig);
		}
		m_TextHelper.UpdateTextSize();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.color = base.ViewModel.IconColor;
	}
}
