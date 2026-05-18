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

	public LocalizedString Beard;

	public LocalizedString BeardColor;

	public LocalizedString Appearance;

	public LocalizedString Eyebrows;

	public LocalizedString EyebrowsColor;

	public LocalizedString Scars;

	public LocalizedString Implant;

	public LocalizedString SoulMark;

	public LocalizedString Homeworld;

	public LocalizedString NobleHomeworldChild;

	public LocalizedString NobleHomeworldChildSelection;

	public LocalizedString NobleHomeworldDescription;

	public LocalizedString DeathWorld;

	public LocalizedString ChooseDeathWorld;

	public LocalizedString DeathWorldDescription;

	public LocalizedString Mystic;

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

	public LocalizedString LevelUpBattleSkillHint;

	public LocalizedString InstantGainLabel;

	public LocalizedString LevelupNeededLabel;

	public LocalizedString InstantGainHint;

	public LocalizedString LevelupNeededHint;

	public LocalizedString HomeworldSelection;

	public LocalizedString Origin;

	public LocalizedString ChooseOrigin;

	public LocalizedString OriginDescription;

	public LocalizedString Career;

	public LocalizedString ChooseCareer;

	public LocalizedString CareerDescription;

	[Header("Portrait")]
	public LocalizedString Portrait;

	public LocalizedString UploadPortraitManual;

	public LocalizedString PortraitCategoryDefault;

	[Obsolete]
	public LocalizedString PortraitCategoryWarhammer;

	[Obsolete]
	public LocalizedString PortraitCategoryNavigator;

	public LocalizedString PortraitCategoryDarkHeresy;

	public LocalizedString PortraitCategoryRogueTrader;

	public LocalizedString PortraitCategoryCustom;

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

	[Header("Summary")]
	public LocalizedString SelectNameTitle;

	public LocalizedString SelectNameDisclaimer;

	[Header("Default LevelUp Tooltips")]
	public LocalizedString AbilityTitle;

	public LocalizedString AbilityDescription;

	public LocalizedString ModificationTitle;

	public LocalizedString ModificationDescription;

	public LocalizedString SpecializationTitle;

	public LocalizedString SpecializationDescription;

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

	public LocalizedString ShowAll;

	public LocalizedString SkillMaxValue;

	public LocalizedString Psykana;

	public LocalizedString Buff;

	public LocalizedString Movement;

	public LocalizedString Melee;

	public LocalizedString Range;

	public LocalizedString Randomize;

	public LocalizedString Undo;

	public string GetPageLabelByType(CharGenAppearancePageType pageType)
	{
		return pageType switch
		{
			CharGenAppearancePageType.General => Appearance, 
			CharGenAppearancePageType.Hair => Hair, 
			CharGenAppearancePageType.Tattoo => Tattoo, 
			CharGenAppearancePageType.Implants => Implants, 
			CharGenAppearancePageType.Servoskull => Servoskull, 
			_ => string.Empty, 
		};
	}

	public string GetPhaseName(CharGenPhaseType type)
	{
		return type switch
		{
			CharGenPhaseType.Pregen => Pregen, 
			CharGenPhaseType.Portrait => Portrait, 
			CharGenPhaseType.Appearance => Appearance, 
			CharGenPhaseType.SoulMark => SoulMark, 
			CharGenPhaseType.Homeworld => Homeworld, 
			CharGenPhaseType.Occupation => Occupation, 
			CharGenPhaseType.MomentOfTriumph => MomentOfTriumph, 
			CharGenPhaseType.DarkestHour => DarkestHour, 
			CharGenPhaseType.Career => Careers, 
			CharGenPhaseType.Attributes => Attributes, 
			CharGenPhaseType.Summary => Summary, 
			CharGenPhaseType.NobleHomeworldChild => NobleHomeworldChild, 
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
			CharGenPhaseType.DeathHomeworldChild => DeathWorld, 
			CharGenPhaseType.Voice => Voice, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
