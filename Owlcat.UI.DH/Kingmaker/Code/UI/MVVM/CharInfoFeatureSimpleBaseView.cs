using System;
using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoFeatureSimpleBaseView : VirtualListElementViewBase<CharInfoFeatureVM>
{
	[FormerlySerializedAs("m_Icon")]
	[Header("Icon")]
	[SerializeField]
	protected Image m_FeatureIcon;

	[SerializeField]
	protected TalentGroupView m_GroupsView;

	[SerializeField]
	protected TextMeshProUGUI m_DisplayName;

	[SerializeField]
	protected TextMeshProUGUI m_AcronymText;

	protected AccessibilityTextHelper TextHelper;

	[Header("Tooltip")]
	[SerializeField]
	private bool m_HasTooltip;

	[ShowIf("m_HasTooltip")]
	[SerializeField]
	private Graphic m_TooltipRaycastObject;

	[SerializeField]
	private TooltipConfig m_TooltipTooltipConfig;

	private IDisposable m_Tooltip;

	protected override void BindViewImplementation()
	{
		if (TextHelper == null)
		{
			TextHelper = new AccessibilityTextHelper(m_DisplayName);
		}
		Clear();
		Show();
		SetupIcon();
		SetupName();
		SetupTooltip();
		TextHelper.UpdateTextSize();
	}

	private void SetupTooltip()
	{
		if (m_HasTooltip && base.ViewModel.TooltipTemplate() != null)
		{
			m_Tooltip = m_TooltipRaycastObject.SetTooltip(base.ViewModel.TooltipTemplate(), m_TooltipTooltipConfig).AddTo(this);
		}
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		TextHelper.Dispose();
	}

	protected virtual void Clear()
	{
		m_AcronymText.text = string.Empty;
		m_FeatureIcon.enabled = false;
		m_FeatureIcon.sprite = null;
		m_FeatureIcon.color = Color.white;
		m_Tooltip?.Dispose();
	}

	public void SetActiveState(bool state)
	{
		if (state)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		Clear();
		base.gameObject.SetActive(value: false);
	}

	public void SetupIcon()
	{
		m_FeatureIcon.enabled = true;
		if (base.ViewModel.Icon == null)
		{
			m_AcronymText.gameObject.SetActive(value: true);
			m_AcronymText.text = base.ViewModel.Acronym;
			m_AcronymText.color = (((bool)m_GroupsView && base.ViewModel.TalentIconsInfo.HasGroups) ? UIConfig.Instance.GroupAcronymColor : UIConfig.Instance.SingleAcronymColor);
			if (m_GroupsView != null && base.ViewModel.TalentIconsInfo.HasGroups)
			{
				m_GroupsView.SetupView(base.ViewModel.TalentIconsInfo);
				return;
			}
			m_FeatureIcon.color = UIUtilityText.GetColorByText(base.ViewModel.Acronym);
			m_FeatureIcon.sprite = UIUtilityText.GetIconByText(base.ViewModel.Acronym);
			m_GroupsView.Or(null)?.SetActiveState(state: false);
		}
		else
		{
			m_AcronymText.gameObject.SetActive(value: false);
			m_GroupsView.Or(null)?.SetActiveState(state: false);
			m_FeatureIcon.color = Color.white;
			m_FeatureIcon.sprite = base.ViewModel.Icon;
		}
	}

	public void SetupName()
	{
		if (!(m_DisplayName == null))
		{
			m_DisplayName.text = base.ViewModel.DisplayName;
		}
	}

	[ContextMenu("Show Random Icons")]
	private void Test()
	{
		int num = UnityEngine.Random.Range(1, 4);
		TalentGroup talentGroup = (TalentGroup)0;
		TalentGroup mainGroup = (TalentGroup)0;
		int length = Enum.GetValues(typeof(TalentGroup)).Length;
		for (int i = 0; i < num; i++)
		{
			int num2 = UnityEngine.Random.Range(0, length);
			TalentGroup talentGroup2 = (TalentGroup)Mathf.Pow(2f, num2);
			talentGroup |= talentGroup2;
			if (i == 0)
			{
				mainGroup = talentGroup2;
			}
		}
		m_GroupsView.Or(null)?.SetupView(talentGroup, mainGroup);
	}
}
