using System.Collections.Generic;
using Code.View.UI.Helpers;
using Kingmaker.UnitLogic.Abilities;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickPortraitFeaturesView : BrickBaseView<BrickPortraitFeaturesVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_NameLabel;

	[SerializeField]
	private TMP_Text m_Available;

	[SerializeField]
	private Image m_Portrait;

	[SerializeField]
	private _2dxFX_GrayScale m_GrayScale;

	[Header("Icons")]
	[SerializeField]
	private List<Image> m_DesperateMeasureIcons;

	[SerializeField]
	private List<Image> m_HeroicActIcons;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_NameLabel, m_Available).AddTo(this);
		}
		m_NameLabel.SetText(base.ViewModel.Name);
		m_Available.SetText(base.ViewModel.AvailableText);
		m_Portrait.sprite = base.ViewModel.Portrait;
		m_GrayScale.EffectAmount = ((!base.ViewModel.Available) ? 1 : 0);
		BindAbilityIcons(base.ViewModel.DesperateMeasureAbilities, m_DesperateMeasureIcons);
		BindAbilityIcons(base.ViewModel.HeroicActAbilities, m_HeroicActIcons);
		m_TextHelper.UpdateTextSize();
	}

	private void BindAbilityIcons(IReadOnlyList<Ability> abilities, List<Image> icons)
	{
		for (int i = 0; i < abilities.Count && i < icons.Count; i++)
		{
			Ability ability = abilities[i];
			if (ability != null)
			{
				Image image = icons[i];
				image.sprite = ability.Icon;
				image.gameObject.SetActive(value: true);
				image.SetTooltip(new TooltipTemplateAbility(ability.Data)).AddTo(this);
			}
		}
	}
}
