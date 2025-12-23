using Assets.Code.View.UI.MVVM;
using Owlcat.Plugins.DotNetExtensions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpHeaderView : TooltipBaseBrickView<TooltipBrickLevelUpHeaderVM>
{
	[SerializeField]
	private TextMeshProUGUI m_CenteredHeader;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Subtitle;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_Value2;

	[SerializeField]
	private TextMeshProUGUI m_Attribute;

	[SerializeField]
	private TextMeshProUGUI m_Acronym;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private RectTransform m_IconRect;

	[SerializeField]
	private GameObject m_IconFrame;

	[SerializeField]
	private TalentGroupView m_TalentGroupView;

	[SerializeField]
	private TooltipBrickLevelUpHeaderView m_SubheaderView;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private TooltipElementStatValueView m_RequiredStatPrefab;

	[SerializeField]
	private TooltipBrickIconPatternView m_AbilityPatternView;

	private Vector2 m_DefaultIconSize;

	protected override void OnBind()
	{
		base.OnBind();
		if (m_DefaultIconSize == default(Vector2) && m_IconRect != null)
		{
			m_DefaultIconSize = m_IconRect.sizeDelta;
		}
		m_Title.gameObject.SetActive(!base.ViewModel.Data.Title.IsNullOrEmpty());
		m_CenteredHeader.gameObject.SetActive(!base.ViewModel.Data.CenteredHeader.IsNullOrEmpty());
		m_Subtitle.gameObject.SetActive(!base.ViewModel.Data.Subtitle.IsNullOrEmpty());
		m_Value.gameObject.SetActive(!base.ViewModel.Data.Value.IsNullOrEmpty());
		m_Value2.gameObject.SetActive(!base.ViewModel.Data.Value2.IsNullOrEmpty());
		m_Acronym.transform.parent.gameObject.SetActive(!base.ViewModel.Data.Acronym.IsNullOrEmpty());
		m_Attribute.transform.parent.gameObject.SetActive(!base.ViewModel.Data.Attribute.IsNullOrEmpty());
		m_IconRect.gameObject.SetActive(base.ViewModel.Data.Icon != null);
		m_IconFrame.SetActive(base.ViewModel.Data.IconWithFrame);
		m_TalentGroupView.gameObject.SetActive(base.ViewModel.Data.TalentIconInfo != null);
		m_SubheaderView?.gameObject.SetActive(base.ViewModel.SubheaderVM != null);
		m_SubheaderView?.Bind(base.ViewModel.SubheaderVM);
		m_WidgetList?.gameObject.SetActive(base.ViewModel.AbilityData != null && base.ViewModel.AbilityData.RequiredStats != null);
		m_AbilityPatternView?.gameObject.SetActive(base.ViewModel.AbilityData != null && base.ViewModel.AbilityData.BrickIconPattern != null);
		this.SetTooltip(base.ViewModel.Data.Tooltip).AddTo(this);
		if (!base.ViewModel.Data.CenteredHeader.IsNullOrEmpty())
		{
			m_CenteredHeader.text = base.ViewModel.Data.CenteredHeader;
		}
		if (!base.ViewModel.Data.Title.IsNullOrEmpty())
		{
			m_Title.text = base.ViewModel.Data.Title;
		}
		if (!base.ViewModel.Data.Subtitle.IsNullOrEmpty())
		{
			m_Subtitle.text = base.ViewModel.Data.Subtitle;
		}
		if (!base.ViewModel.Data.Value.IsNullOrEmpty())
		{
			m_Value.text = base.ViewModel.Data.Value;
		}
		if (!base.ViewModel.Data.Value2.IsNullOrEmpty())
		{
			m_Value2.text = base.ViewModel.Data.Value2;
		}
		if (!base.ViewModel.Data.Attribute.IsNullOrEmpty())
		{
			m_Attribute.text = base.ViewModel.Data.Attribute;
		}
		if (!base.ViewModel.Data.Acronym.IsNullOrEmpty())
		{
			m_Acronym.text = base.ViewModel.Data.Acronym;
		}
		if (base.ViewModel.Data.Icon != null)
		{
			m_Icon.sprite = base.ViewModel.Data.Icon;
		}
		if (base.ViewModel.Data.TalentIconInfo != null)
		{
			m_TalentGroupView.SetupView(base.ViewModel.Data.TalentIconInfo);
		}
		m_IconRect.sizeDelta = ((base.ViewModel.Data.IconSize != default(Vector2)) ? base.ViewModel.Data.IconSize : m_DefaultIconSize);
		if (base.ViewModel.AbilityData != null && base.ViewModel.AbilityData.RequiredStats != null)
		{
			m_WidgetList.DrawEntries(base.ViewModel.AbilityData.RequiredStats, m_RequiredStatPrefab).AddTo(this);
		}
		if (base.ViewModel.AbilityData != null && base.ViewModel.AbilityData.BrickIconPattern != null)
		{
			m_AbilityPatternView.Bind(base.ViewModel.AbilityData.BrickIconPattern.GetVM().AddTo(this) as TooltipBrickIconPatternVM);
		}
	}
}
