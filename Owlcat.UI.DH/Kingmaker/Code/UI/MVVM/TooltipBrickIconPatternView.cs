using System.Collections.Generic;
using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickIconPatternView : TooltipBaseBrickView<TooltipBrickIconPatternVM>
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

	[Header("Title")]
	[SerializeField]
	private TextMeshProUGUI m_TitleText;

	[Header("Secondary")]
	[SerializeField]
	protected TextMeshProUGUI m_SecondaryText;

	[SerializeField]
	private TextMeshProUGUI m_SecondaryValue;

	[Header("Tertiary")]
	[SerializeField]
	protected TextMeshProUGUI m_TertiaryText;

	[SerializeField]
	private TextMeshProUGUI m_TertiaryValue;

	private readonly Dictionary<TextMeshProUGUI, TextFieldParams> m_DefaultParams = new Dictionary<TextMeshProUGUI, TextFieldParams>();

	private Color32? m_DefaultFrameColor;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		base.OnBind();
		m_TextHelper = new AccessibilityTextHelper(m_TitleText, m_SecondaryText, m_TertiaryText, m_SecondaryValue, m_TertiaryValue).AddTo(this);
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
		TextMeshProUGUI acronymText = m_AcronymText;
		TalentIconInfo talentIconInfo = base.ViewModel.TalentIconInfo;
		acronymText.color = ((talentIconInfo != null && talentIconInfo.HasGroups) ? UIConfig.Instance.GroupAcronymColor : UIConfig.Instance.SingleAcronymColor);
		m_FrameSelectable.Or(null)?.SetActiveLayer(base.ViewModel.IconMode.ToString());
		ApplyValues(m_TitleText, null, base.ViewModel.TitleValues);
		ApplyValues(m_SecondaryText, m_SecondaryValue, base.ViewModel.SecondaryValues);
		ApplyValues(m_TertiaryText, m_TertiaryValue, base.ViewModel.TertiaryValues);
		m_SecondaryText.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)).AddTo(this);
		m_TertiaryText.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)).AddTo(this);
		if (base.ViewModel.Tooltip != null)
		{
			m_SkillIcon.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		}
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_AbilityPatternView.Destroy();
		m_TextHelper.Dispose();
		m_TextHelper = null;
	}

	private void ApplyValues(TextMeshProUGUI text, TextMeshProUGUI value, TooltipBrickIconPattern.TextFieldValues textFieldValues)
	{
		if (text != null)
		{
			text.text = textFieldValues?.Text;
			text.gameObject.SetActive(!string.IsNullOrEmpty(textFieldValues?.Text));
		}
		if (value != null)
		{
			value.text = textFieldValues?.Value;
			value.gameObject.SetActive(!string.IsNullOrEmpty(textFieldValues?.Value));
		}
	}
}
