using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Levelup.Selections;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickFeatureHeaderView : TooltipBaseBrickView<TooltipBrickFeatureVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	protected TextMeshProUGUI m_AdditionalDescription1;

	[SerializeField]
	protected TextMeshProUGUI m_AdditionalDescription2;

	[SerializeField]
	private TextMeshProUGUI m_Acronym;

	[SerializeField]
	private TalentGroupView m_TalentGroupView;

	[Header("Font size")]
	[SerializeField]
	private float m_DefaultFontSize = 22f;

	[SerializeField]
	private float m_AdditionalDescDefaultFontSize = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 26f;

	[SerializeField]
	private float m_AdditionalDescDefaultConsoleFontSize = 20f;

	protected override void OnBind()
	{
		base.OnBind();
		m_Label.text = base.ViewModel.Name;
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.color = base.ViewModel.IconColor;
		m_Acronym.text = base.ViewModel.Acronym;
		m_TalentGroupView.SetupView(base.ViewModel.TalentIconsInfo);
		TextMeshProUGUI acronym = m_Acronym;
		TalentIconInfo talentIconsInfo = base.ViewModel.TalentIconsInfo;
		acronym.color = ((talentIconsInfo != null && talentIconsInfo.HasGroups) ? UIConfig.Instance.GroupAcronymColor : UIConfig.Instance.SingleAcronymColor);
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		m_AdditionalDescription1.text = base.ViewModel.AdditionalField1;
		m_AdditionalDescription2.text = base.ViewModel.AdditionalField2;
		m_AdditionalDescription1.gameObject.SetActive(!string.IsNullOrEmpty(base.ViewModel.AdditionalField1));
		m_AdditionalDescription2.gameObject.SetActive(!string.IsNullOrEmpty(base.ViewModel.AdditionalField2));
		m_Label.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * m_FontMultiplier;
		m_AdditionalDescription1.fontSize = (Game.Instance.IsControllerMouse ? m_AdditionalDescDefaultFontSize : m_AdditionalDescDefaultConsoleFontSize) * m_FontMultiplier;
		m_AdditionalDescription2.fontSize = (Game.Instance.IsControllerMouse ? m_AdditionalDescDefaultFontSize : m_AdditionalDescDefaultConsoleFontSize) * m_FontMultiplier;
	}
}
