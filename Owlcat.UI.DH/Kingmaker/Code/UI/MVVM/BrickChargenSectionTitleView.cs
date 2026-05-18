using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickChargenSectionTitleView : BrickBaseView<BrickChargenSectionTitleVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_TitleText;

	[SerializeField]
	private TMP_Text m_InfoText;

	[SerializeField]
	private OwlcatMultiSelectable m_TitleTypeSelectable;

	[SerializeField]
	private Image m_TitleTooltipPlace;

	[SerializeField]
	private Image m_StatusTooltipPlace;

	protected override void OnBind()
	{
		base.OnBind();
		m_TitleText.text = GetGroupTitle();
		m_InfoText.text = GetModeText();
		m_TitleTypeSelectable.SetActiveLayer(base.ViewModel.TitleType.ToString());
		string description = base.ViewModel.TitleType switch
		{
			TitleType.InstantGain => UIStrings.Instance.CharGen.InstantGainHint, 
			TitleType.LevelupNeeded => UIStrings.Instance.CharGen.LevelupNeededHint, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		m_StatusTooltipPlace.SetTooltip(new TooltipTemplateSimple(GetModeText(), description)).AddTo(this);
		m_TitleTooltipPlace.SetTooltip(GetTooltip()).AddTo(this);
	}

	private string GetGroupTitle()
	{
		UICharGen charGen = UIStrings.Instance.CharGen;
		return base.ViewModel.FeatureGroup switch
		{
			FeatureGroup.BackgroundFeature => charGen.BackgroundFeatures, 
			FeatureGroup.Attribute => charGen.BackgroundStatsBonuses, 
			FeatureGroup.Skill => charGen.BackgroundSkillsBonuses, 
			FeatureGroup.ActiveAbility => UIStrings.Instance.CharacterSheet.Abilities, 
			FeatureGroup.Talent => charGen.BackgroundTalentsForLevelUp, 
			FeatureGroup.AbilityUpgrade => charGen.LevelUpUpgrade, 
			FeatureGroup.KeystoneFeature => UIStrings.Instance.CharacterSheet.KeystoneFeaturesHeader, 
			FeatureGroup.Modifier => UIStrings.Instance.AbilityModifications.ModificationsLabel, 
			FeatureGroup.Specialization => UIStrings.Instance.CharGen.LevelUpSpecialization, 
			FeatureGroup.ChargenPsyker => UIStrings.Instance.CharGen.LevelUpSpecialization, 
			FeatureGroup.None => UIStrings.Instance.CharacterSheet.Features, 
			_ => string.Empty, 
		};
	}

	private TooltipBaseTemplate GetTooltip()
	{
		FeatureGroup featureGroup = ((base.ViewModel.FeatureGroup != FeatureGroup.ChargenPsyker) ? base.ViewModel.FeatureGroup : FeatureGroup.Specialization);
		FeatureGroup featureGroup2 = featureGroup;
		return new TooltipTemplateGlossary($"{featureGroup2}_Chargen_Selection");
	}

	private string GetModeText()
	{
		UICharGen charGen = UIStrings.Instance.CharGen;
		return base.ViewModel.TitleType switch
		{
			TitleType.InstantGain => charGen.InstantGainLabel.Text, 
			TitleType.LevelupNeeded => charGen.LevelupNeededLabel.Text, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
