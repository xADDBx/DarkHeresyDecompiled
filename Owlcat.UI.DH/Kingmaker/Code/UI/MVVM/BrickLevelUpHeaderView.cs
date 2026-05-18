using Owlcat.Plugins.DotNetExtensions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpHeaderView : BrickBaseView<BrickLevelUpHeaderVM>
{
	[Header("Elements")]
	[SerializeField]
	private TextValueTupleView m_Title;

	[SerializeField]
	private TextValueTupleView m_Subtitle;

	[SerializeField]
	private TMP_Text m_Attribute;

	[SerializeField]
	private TMP_Text m_Acronym;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private OwlcatMultiSelectable m_IconDecorSelectable;

	[SerializeField]
	private TalentGroupView m_TalentGroupView;

	[SerializeField]
	private BrickLevelUpHeaderView m_SubheaderView;

	[SerializeField]
	private WidgetList m_WidgetList;

	[Header("Views")]
	[SerializeField]
	private TooltipElementStatValueView m_RequiredStatPrefab;

	[SerializeField]
	private BrickIconPatternView m_AbilityPatternView;

	protected override void OnBind()
	{
		base.OnBind();
		m_Title.Bind(base.ViewModel.UIData.Title);
		m_Subtitle.Bind(base.ViewModel.UIData.Subtitle);
		m_Acronym.transform.parent.gameObject.SetActive(!base.ViewModel.UIData.Acronym.IsNullOrEmpty());
		m_Attribute.transform.parent.gameObject.SetActive(!base.ViewModel.UIData.Attribute.IsNullOrEmpty());
		m_Icon.gameObject.SetActive(base.ViewModel.UIData.Icon != null);
		m_TalentGroupView.gameObject.SetActive(base.ViewModel.UIData.TalentIconInfo != null);
		m_IconDecorSelectable.SetActiveLayer(base.ViewModel.UIData.IconDecor.ToString());
		m_SubheaderView.gameObject.SetActive(base.ViewModel.SubheaderVM != null);
		m_SubheaderView.Bind(base.ViewModel.SubheaderVM);
		GameObject obj = m_WidgetList.gameObject;
		TooltipLevelUpAbilityData abilityData = base.ViewModel.AbilityData;
		obj.SetActive(abilityData != null && abilityData.RequiredStats != null);
		if (base.ViewModel.AbilityData?.RequiredStats != null)
		{
			m_WidgetList.DrawEntries(base.ViewModel.AbilityData.RequiredStats, m_RequiredStatPrefab).AddTo(this);
		}
		this.SetTooltip(base.ViewModel.UIData.Tooltip).AddTo(this);
		m_Attribute.SetText(base.ViewModel.UIData.Attribute);
		m_Acronym.SetText(base.ViewModel.UIData.Acronym);
		m_Icon.sprite = base.ViewModel.UIData.Icon;
		m_Icon.color = base.ViewModel.UIData.IconColor;
		m_TalentGroupView.SetupView(base.ViewModel.UIData.TalentIconInfo);
		GameObject obj2 = m_AbilityPatternView.gameObject;
		abilityData = base.ViewModel.AbilityData;
		obj2.SetActive(abilityData != null && abilityData.BrickIconPattern != null);
		m_AbilityPatternView.Bind(base.ViewModel.AbilityData?.BrickIconPattern);
	}
}
