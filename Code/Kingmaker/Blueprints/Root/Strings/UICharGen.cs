using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UICharGen
{
	public LocalizedString Skills;

	public LocalizedString ChooseName;

	public LocalizedString Complete;

	public LocalizedString Next;

	public LocalizedString Back;

	public LocalizedString Voice;

	public LocalizedString BodyType;

	public LocalizedString BodyConstitution;

	public LocalizedString Face;

	public LocalizedString SkinTone;

	public LocalizedString HairStyle;

	public LocalizedString HairColor;

	public LocalizedString TattooColor;

	public LocalizedString PrimaryClothColor;

	public LocalizedString SecondaryClothColor;

	public LocalizedString HitPoints;

	public LocalizedString Beard;

	public LocalizedString BeardColor;

	public LocalizedString Appearance;

	public LocalizedString Eyebrows;

	public LocalizedString EyebrowsColor;

	public LocalizedString Scars;

	public LocalizedString FacePaint;

	public LocalizedString Implant;

	public LocalizedString SoulMark;

	public LocalizedString Homeworld;

	public LocalizedString ImperialHomeworldChildSelection;

	public LocalizedString ForgeHomeworldChildSelection;

	public LocalizedString SanctionedPsykerSelection;

	public LocalizedString Occupation;

	public LocalizedString Navigator;

	public LocalizedString DarkestHour;

	public LocalizedString MomentOfTriumph;

	public LocalizedString Careers;

	public LocalizedString Attributes;

	public LocalizedString Hair;

	public LocalizedString Tattoo;

	public LocalizedString Implants;

	public LocalizedString Servoskull;

	public LocalizedString NavigatorMutations;

	public LocalizedString Summary;

	public LocalizedString SureWantClose;

	public LocalizedString CloseCoopChargenNotRt;

	public LocalizedString EnterSearchTextHere;

	public LocalizedString Pregen;

	public LocalizedString CustomCharacterPregen;

	public LocalizedString CreateCustomCharacter;

	public LocalizedString AvailableStatsPointLeft;

	public LocalizedString NoAvailableStatsPointLeft;

	public LocalizedString CannotAdvanceStatHint;

	public LocalizedString ShowVisualSettings;

	public LocalizedString HideVisualSettings;

	public LocalizedString Background;

	public LocalizedString BackgroundFeatures;

	public LocalizedString BackgroundStatsBonuses;

	public LocalizedString BackgroundSkillsBonuses;

	public LocalizedString BackgroundUnlockedFeaturesForLevelUp;

	public LocalizedString BackgroundStatsForLevelUp;

	public LocalizedString BackgroundSkillsForLevelUp;

	public LocalizedString BackgroundTalentsForLevelUp;

	public LocalizedString EditName;

	public LocalizedString SetRandomName;

	public LocalizedString EditNameButton;

	public LocalizedString SetRandomNameButton;

	public LocalizedString PhaseNotCompleted;

	public LocalizedString InspectCareer;

	public LocalizedString RespecWindowHeader;

	public LocalizedString RespecSelectCharacter;

	public LocalizedString RespecWindowWarning;

	public LocalizedString RespecCostPF;

	public LocalizedString SwitchPageSet;

	public LocalizedString PlayVoicePreview;

	public LocalizedString SwitchPortraitsCategoryTab;

	public LocalizedString ShouldSetAllAttributesPointsWarning;

	public LocalizedString CharacterSkillPoints;

	public LocalizedString SwitchToPantograph;

	public LocalizedString SwitchToAppearance;

	public LocalizedString NothingToChoose;

	public LocalizedString ToCharacterInfo;

	public LocalizedString LevelUpAbility;

	public LocalizedString LevelUpModification;

	public LocalizedString LevelUpPlanning;

	public LocalizedString LevelUpResult;

	public LocalizedString LevelUpSpecialization;

	public LocalizedString LevelUpSkill;

	public LocalizedString LevelUpTalent;

	public LocalizedString LevelUpUpgrade;

	public LocalizedString LevelUpCharacteristics;

	public LocalizedString LevelUpFeature;

	public LocalizedString LevelUpChooseAbility;

	public LocalizedString LevelUpChooseModification;

	public LocalizedString LevelUpChooseSpecialization;

	public LocalizedString LevelUpChooseSkill;

	public LocalizedString LevelUpChooseTalent;

	public LocalizedString LevelUpChooseUpgrade;

	public LocalizedString LevelUpChooseCharacteristics;

	public LocalizedString LevelUpChooseFeature;

	[Header("Portrait")]
	public LocalizedString Portrait;

	public LocalizedString UploadPortraitManual;

	public LocalizedString PortraitCategoryDefault;

	public LocalizedString PortraitCategoryWarhammer;

	public LocalizedString PortraitCategoryCustom;

	public LocalizedString PortraitCategoryNavigator;

	public LocalizedString ChangePortrait;

	public LocalizedString ChangePortraitDescription;

	public LocalizedString ChangePortraitDescriptionConsole;

	public LocalizedString CustomPortraitHeader;

	public LocalizedString OpenPortraitFolder;

	public LocalizedString RefreshPortrait;

	public LocalizedString AddPortrait;

	public LocalizedString WaitForDownloadingPortraits;

	[Header("Buttons Hints")]
	public LocalizedString SelectDoctrineHint;

	public LocalizedString SpreadOutPointsHint;

	public LocalizedString SkillPointsContainerHint;

	[Header("Pregens")]
	public LocalizedString CreateNewCompanion;

	public LocalizedString CreateNewCompanionDescription;

	public LocalizedString CreateNewNavigator;

	public LocalizedString CreateNewNavigatorDescription;

	[Header("Default LevelUp Tooltips")]
	public LocalizedString AbilityTitle;

	public LocalizedString AbilityDescription;

	public LocalizedString CharacteristicsTitle;

	public LocalizedString CharacteristicsDescription;

	public LocalizedString ModificationTitle;

	public LocalizedString ModificationDescription;

	public LocalizedString SpecializationTitle;

	public LocalizedString SpecializationDescription;

	public LocalizedString SkillTitle;

	public LocalizedString SkillDescription;

	public LocalizedString TalentTitle;

	public LocalizedString TalentDescription;

	public LocalizedString UpgradeTitle;

	public LocalizedString UpgradeDescription;

	public LocalizedString FeatureTitle;

	public LocalizedString FeatureDescription;

	public LocalizedString GroupByType;

	public LocalizedString GroupBySource;

	public LocalizedString CollapseAll;

	public LocalizedString SkillMaxValue;

	public string GetPageLabelByType(CharGenAppearancePageType pageType)
	{
		return pageType switch
		{
			CharGenAppearancePageType.Portrait => Portrait, 
			CharGenAppearancePageType.General => Appearance, 
			CharGenAppearancePageType.Hair => Hair, 
			CharGenAppearancePageType.Tattoo => Tattoo, 
			CharGenAppearancePageType.Implants => Implants, 
			CharGenAppearancePageType.Voice => Voice, 
			CharGenAppearancePageType.Servoskull => Servoskull, 
			CharGenAppearancePageType.NavigatorMutations => NavigatorMutations, 
			_ => string.Empty, 
		};
	}

	public string GetPhaseName(CharGenPhaseType type)
	{
		return type switch
		{
			CharGenPhaseType.Pregen => Pregen, 
			CharGenPhaseType.Appearance => Appearance, 
			CharGenPhaseType.SoulMark => SoulMark, 
			CharGenPhaseType.Homeworld => Homeworld, 
			CharGenPhaseType.Occupation => Occupation, 
			CharGenPhaseType.MomentOfTriumph => MomentOfTriumph, 
			CharGenPhaseType.DarkestHour => DarkestHour, 
			CharGenPhaseType.Career => Careers, 
			CharGenPhaseType.Attributes => Attributes, 
			CharGenPhaseType.Summary => Summary, 
			CharGenPhaseType.ImperialHomeworldChild => ImperialHomeworldChildSelection, 
			CharGenPhaseType.ForgeHomeworldChild => ForgeHomeworldChildSelection, 
			CharGenPhaseType.SanctionedPsyker => SanctionedPsykerSelection, 
			CharGenPhaseType.Navigator => Navigator, 
			CharGenPhaseType.LevelUpAbility => LevelUpAbility, 
			CharGenPhaseType.Characteristics => LevelUpCharacteristics, 
			CharGenPhaseType.LevelUpModification => LevelUpModification, 
			CharGenPhaseType.LevelUpPlanning => LevelUpPlanning, 
			CharGenPhaseType.LevelUpResult => LevelUpResult, 
			CharGenPhaseType.LevelUpSpecialization => LevelUpSpecialization, 
			CharGenPhaseType.LevelUpSkill => LevelUpSkill, 
			CharGenPhaseType.LevelUpTalent => LevelUpTalent, 
			CharGenPhaseType.LevelUpUpgrade => LevelUpUpgrade, 
			CharGenPhaseType.LevelUpFeature => LevelUpFeature, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public string GetChoosePhaseName(CharGenPhaseType type)
	{
		return type switch
		{
			CharGenPhaseType.LevelUpAbility => LevelUpChooseAbility, 
			CharGenPhaseType.Characteristics => LevelUpChooseCharacteristics, 
			CharGenPhaseType.LevelUpModification => LevelUpChooseModification, 
			CharGenPhaseType.LevelUpSpecialization => LevelUpChooseSpecialization, 
			CharGenPhaseType.LevelUpSkill => LevelUpChooseSkill, 
			CharGenPhaseType.LevelUpTalent => LevelUpChooseTalent, 
			CharGenPhaseType.LevelUpUpgrade => LevelUpChooseUpgrade, 
			CharGenPhaseType.LevelUpFeature => LevelUpChooseFeature, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public (string, string) GetDefaultTooltipStrings(CharGenPhaseType type)
	{
		return type switch
		{
			CharGenPhaseType.LevelUpAbility => (UIStrings.Instance.CharacterSheet.ActiveAbilityFeatureGroupHint, AbilityDescription), 
			CharGenPhaseType.Characteristics => (UIStrings.Instance.CharacterSheet.AttributeFeatureGroupHint, CharacteristicsDescription), 
			CharGenPhaseType.LevelUpModification => (ModificationTitle, ModificationDescription), 
			CharGenPhaseType.LevelUpSpecialization => (SpecializationTitle, SpecializationDescription), 
			CharGenPhaseType.LevelUpSkill => (UIStrings.Instance.CharacterSheet.SkillFeatureGroupHint, SkillDescription), 
			CharGenPhaseType.LevelUpTalent => (UIStrings.Instance.CharacterSheet.TalentFeatureGroupHint, TalentDescription), 
			CharGenPhaseType.LevelUpUpgrade => (UpgradeTitle, UpgradeDescription), 
			CharGenPhaseType.LevelUpFeature => (FeatureTitle, FeatureDescription), 
			_ => (string.Empty, string.Empty), 
		};
	}
}
