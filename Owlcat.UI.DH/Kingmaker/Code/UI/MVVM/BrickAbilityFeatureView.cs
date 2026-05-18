using Code.View.UI.Helpers;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityFeatureView : BrickBaseView<BrickAbilityFeatureVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_Text;

	[SerializeField]
	private TMP_Text m_Acronym;

	[SerializeField]
	private TalentGroupView m_TalentGroupView;

	[SerializeField]
	private MonoBehaviour m_TooltipSource;

	protected override void OnBind()
	{
		base.OnBind();
		m_TextHelper = new AccessibilityTextHelper(m_Text).AddTo(this);
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.color = base.ViewModel.IconColor;
		m_Acronym.SetText(base.ViewModel.Acronym);
		m_TooltipSource.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		m_TalentGroupView.SetupView(base.ViewModel.TalentIconsInfo);
		m_Text.SetText(base.ViewModel.Text);
		m_TextHelper.UpdateTextSize();
	}
}
