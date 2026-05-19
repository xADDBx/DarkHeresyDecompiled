using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityHeaderView : BrickBaseView<BrickAbilityHeaderVM>
{
	[SerializeField]
	private Image m_AbilityIcon;

	[SerializeField]
	private Image m_ModifierIcon;

	[SerializeField]
	private GameObject[] m_ActiveModifierObjects;

	[SerializeField]
	private GameObject m_Acronym;

	[SerializeField]
	private TMP_Text m_AcronymText;

	[SerializeField]
	private TMP_Text m_NameText;

	[SerializeField]
	private TMP_Text m_TypeText;

	[SerializeField]
	private TMP_Text m_APText;

	[SerializeField]
	private TMP_Text m_APValueText;

	[SerializeField]
	private GameObject m_APContainer;

	[SerializeField]
	private Slider m_APSlider;

	[SerializeField]
	private TalentGroupView m_TalentGroup;

	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	protected override void OnBind()
	{
		base.OnBind();
		m_TextHelper = new AccessibilityTextHelper(m_NameText, m_TypeText, m_APText, m_APValueText);
		m_NameText.SetText(base.ViewModel.AbilityName);
		m_TypeText.SetText(base.ViewModel.AbilityType);
		SetupAP();
		SetupIcon();
		SetupModifier();
		SetupVisualLayer();
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_TextHelper?.Dispose();
		m_TextHelper = null;
	}

	private void SetupAP()
	{
		bool flag = base.ViewModel.ActionPointCost > 0;
		m_APContainer.SetActive(flag);
		if (flag)
		{
			m_APSlider.value = base.ViewModel.ActionPointCost;
			m_APText.SetText(base.ViewModel.APText);
			TMP_Text aPValueText = m_APValueText;
			int actionPointCost = base.ViewModel.ActionPointCost;
			aPValueText.SetText(actionPointCost.ToString());
		}
	}

	private void SetupIcon()
	{
		bool flag = !string.IsNullOrEmpty(base.ViewModel.Acronym);
		m_Acronym.SetActive(flag);
		m_AbilityIcon.sprite = base.ViewModel.AbilityIcon;
		m_AbilityIcon.color = (flag ? ((Color)UIUtilityText.GetColorByText(base.ViewModel.Acronym)) : Color.white);
		if (flag)
		{
			m_TalentGroup.SetupView(base.ViewModel.TalentIconInfo);
			m_AcronymText.color = UIConfig.Instance.TooltipsConfig.GetAcronymColor(base.ViewModel.TalentIconInfo?.HasGroups);
		}
	}

	private void SetupModifier()
	{
		if (base.ViewModel.ToggleState == BrickAbilityHeaderVM.AbilityToggleState.None)
		{
			bool active = base.ViewModel.ModifierIcon != null;
			m_ModifierIcon.gameObject.SetActive(active);
			GameObject[] activeModifierObjects = m_ActiveModifierObjects;
			for (int i = 0; i < activeModifierObjects.Length; i++)
			{
				activeModifierObjects[i].SetActive(active);
			}
			m_ModifierIcon.sprite = base.ViewModel.ModifierIcon;
		}
	}

	private void SetupVisualLayer()
	{
		string activeLayer = base.ViewModel.ToggleState.ToString();
		m_Selectable.SetActiveLayer(activeLayer);
	}
}
