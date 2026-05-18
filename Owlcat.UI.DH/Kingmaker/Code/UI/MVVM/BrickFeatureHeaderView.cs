using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickFeatureHeaderView : BrickBaseView<BrickFeatureVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Label;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_Acronym;

	[SerializeField]
	private TalentGroupView m_TalentGroupView;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Label, m_Acronym).AddTo(this);
		}
		base.OnBind();
		m_Label.text = base.ViewModel.Name;
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.color = base.ViewModel.IconColor;
		m_Acronym.text = base.ViewModel.Acronym;
		m_TalentGroupView.SetupView(base.ViewModel.TalentIconsInfo);
		m_Acronym.color = UIConfig.Instance.TooltipsConfig.GetAcronymColor(base.ViewModel.TalentIconsInfo?.HasGroups);
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
