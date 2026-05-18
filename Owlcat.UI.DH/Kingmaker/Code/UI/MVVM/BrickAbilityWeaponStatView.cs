using Code.View.UI.Helpers;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityWeaponStatView : BrickBaseView<BrickAbilityWeaponStatVM>
{
	[SerializeField]
	private TMP_Text m_NameText;

	[SerializeField]
	private TMP_Text m_ValueText;

	[SerializeField]
	private Image m_IconImage;

	protected override void OnBind()
	{
		base.OnBind();
		m_TextHelper = new AccessibilityTextHelper(m_NameText, m_ValueText).AddTo(this);
		m_NameText.SetText(base.ViewModel.StatName);
		m_ValueText.SetText(base.ViewModel.StatValue);
		m_IconImage.sprite = base.ViewModel.StatIcon;
		m_TextHelper.UpdateTextSize();
	}
}
