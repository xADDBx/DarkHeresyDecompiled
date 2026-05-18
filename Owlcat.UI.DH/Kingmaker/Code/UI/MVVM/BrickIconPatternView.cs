using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickIconPatternView : BrickBaseView<BrickIconPatternVM>
{
	[Header("SkillBlock")]
	[SerializeField]
	private Image m_SkillIcon;

	[SerializeField]
	private Image m_AcronymBackground;

	[SerializeField]
	private TextMeshProUGUI m_AcronymText;

	[SerializeField]
	private TalentGroupView m_TalentGroup;

	[Header("IconBlock")]
	[SerializeField]
	private Image m_Icon;

	[Tooltip("Has one of states of IconPatternMode enum : SkillMode, IconMode, NoneMode")]
	[SerializeField]
	private OwlcatMultiSelectable m_FrameSelectable;

	[Header("Pattern")]
	[SerializeField]
	private AbilityPatternView m_AbilityPatternView;

	[Header("Text Block")]
	[SerializeField]
	private TextEntityWidget m_TitleText;

	[SerializeField]
	private TextValueTupleView m_Secondary;

	[SerializeField]
	private TextValueTupleView m_Tertiary;

	protected override void OnBind()
	{
		base.OnBind();
		m_AbilityPatternView.Initialize(base.ViewModel.PatternData);
		m_Icon.sprite = base.ViewModel.Icon;
		m_AcronymText.text = base.ViewModel.Acronym;
		bool flag = !string.IsNullOrEmpty(base.ViewModel.Acronym);
		(flag ? m_AcronymBackground : m_SkillIcon).sprite = base.ViewModel.Icon;
		m_AcronymBackground.gameObject.SetActive(flag);
		m_SkillIcon.gameObject.SetActive(!flag);
		m_AcronymBackground.color = UIUtilityText.GetColorByText(base.ViewModel.Acronym);
		if (m_TalentGroup != null)
		{
			m_TalentGroup.SetupView(base.ViewModel.TalentIconInfo);
		}
		m_AcronymText.color = UIConfig.Instance.TooltipsConfig.GetAcronymColor(base.ViewModel.TalentIconInfo?.HasGroups);
		m_FrameSelectable.Or(null)?.SetActiveLayer(base.ViewModel.IconMode.ToString());
		m_TitleText.Bind(base.ViewModel.Title)?.AddTo(this);
		m_Secondary.Bind(base.ViewModel.SecondaryValuesElement);
		m_Tertiary.Bind(base.ViewModel.TertiaryValuesElement);
		if (base.ViewModel.Tooltip != null)
		{
			m_SkillIcon.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		}
	}

	protected override void OnUnbind()
	{
		m_AbilityPatternView.Destroy();
	}
}
