using System;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Levelup.Selections;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UITextCharSheet
{
	[Header("AbilityScores")]
	[NotNull]
	public LocalizedString WeaponSkillShort;

	[NotNull]
	public LocalizedString BallisticSkillShort;

	[NotNull]
	public LocalizedString StrengthShort;

	[NotNull]
	public LocalizedString ToughnessShort;

	[NotNull]
	public LocalizedString AgilityShort;

	[NotNull]
	public LocalizedString InteligenceShort;

	[NotNull]
	public LocalizedString PerceptionShort;

	[NotNull]
	public LocalizedString WillpowerShort;

	[NotNull]
	public LocalizedString FellowshipShort;

	[NotNull]
	public LocalizedString LvlShort;

	[NotNull]
	public LocalizedString Attack;

	[NotNull]
	public LocalizedString Abilities;

	[Header("Menu")]
	[NotNull]
	public LocalizedString Summary;

	[NotNull]
	public LocalizedString Features;

	[NotNull]
	public LocalizedString PsykerPowers;

	[NotNull]
	public LocalizedString LevelProgression;

	[NotNull]
	public LocalizedString Biography;

	[NotNull]
	public LocalizedString FactionsReputation;

	[Header("Container's label")]
	[NotNull]
	public LocalizedString Skills;

	[NotNull]
	public LocalizedString Convictions;

	[NotNull]
	public LocalizedString Stats;

	[NotNull]
	public LocalizedString OffenceLabel;

	[NotNull]
	public LocalizedString DefenceLabel;

	[NotNull]
	public LocalizedString MeleeLabel;

	[NotNull]
	public LocalizedString DamageReductionLabel;

	[NotNull]
	public LocalizedString RangedLabel;

	[NotNull]
	public LocalizedString StatsLabel;

	[NotNull]
	public LocalizedString Career;

	[NotNull]
	public LocalizedString StatusEffects;

	[NotNull]
	public LocalizedString Weapons;

	[NotNull]
	public LocalizedString PsyRatingShort;

	[NotNull]
	public LocalizedString Vendors;

	[NotNull]
	public LocalizedString BackgroundAbilities;

	[NotNull]
	public LocalizedString CareerAbilities;

	public LocalizedString Decisions;

	[Header("Container's label")]
	[NotNull]
	public LocalizedString ItemsAbilities;

	[NotNull]
	public LocalizedString SoulMarkAbilities;

	[Header("Buffs")]
	[NotNull]
	public LocalizedString NoBuffText;

	[NotNull]
	public LocalizedString Permanent;

	public LocalizedString DeactivatedFeature;

	[Header("Stats")]
	public LocalizedString ArmorAbsorption;

	public LocalizedString ArmorDeflection;

	[Obsolete("Defence")]
	public LocalizedString Dodge;

	[Obsolete("Defence")]
	public LocalizedString DodgeReduction;

	public LocalizedString Resolve;

	public LocalizedString WeaponSet;

	[Header("Factions Reputation")]
	public LocalizedString Drusians;

	public LocalizedString DrusiansDescription;

	public LocalizedString Explorators;

	public LocalizedString ExploratorsDescription;

	public LocalizedString Kasballica;

	public LocalizedString KasballicaDescription;

	public LocalizedString MaxReputationLevel;

	public LocalizedString FactionAstraMilitarum;

	public LocalizedString FactionAdeptusMechanicus;

	public LocalizedString FactionRulingCouncilOfNobles;

	public LocalizedString FactionMinistorumAndRedemptionists;

	[Header("Biography")]
	public LocalizedString EmptyBiographyDesc;

	public LocalizedString Alignment;

	[Header("Progression")]
	public LocalizedString CareerPathHeader;

	public LocalizedString CareerPathHasNewRanksHeader;

	public LocalizedString CareerPathDescription;

	public LocalizedString CareerUpgradeHeader;

	public LocalizedString CareerUpgradeDescription;

	public LocalizedString CareerUpgradedDescription;

	public LocalizedString DialogCloseProgression;

	public LocalizedString LevelupDialogCloseProgression;

	public LocalizedString CurrentLevelLabel;

	public LocalizedString RanksCounterLabel;

	public LocalizedString HeaderFeatureDescriptionTab;

	public LocalizedString HeaderImprovement;

	public LocalizedString HeaderSummaryTab;

	public LocalizedString ToSummaryTab;

	public LocalizedString KeystoneAbilitiesHeader;

	public LocalizedString KeystoneFeaturesHeader;

	public LocalizedString UltimateAbilitiesHeader;

	public LocalizedString LevelUpOnOtherUnitButtonHint;

	public LocalizedString NoRanksForUpgradeButtonHint;

	public LocalizedString UnitIsInCombatButtonHint;

	public LocalizedString SelectFeatureButtonHint;

	public LocalizedString SelectedCareerFinished;

	public LocalizedString RecommendedCareerPath;

	public LocalizedString RecommendedByCareerPath;

	public LocalizedString BackToCareersList;

	public LocalizedString RankLabel;

	public LocalizedString ClickToDoctrine;

	public LocalizedString ClickToDoctrineConsole;

	public LocalizedString ToggleFavorites;

	public LocalizedString NoFeaturesInFilter;

	public LocalizedString AlreadyInLevelUp;

	public LocalizedString ShowUnitPanel;

	public LocalizedString ShowTooltip;

	public LocalizedString ShowUnavailableFeatures;

	public LocalizedString HideUnavailableFeatures;

	public LocalizedString PlanLevelUp;

	public LocalizedString NoTalentsYet;

	public LocalizedString ToLevelUp;

	[Header("Feature Groups")]
	public LocalizedString AttributeFeatureGroupLabel;

	public LocalizedString SkillFeatureGroupLabel;

	public LocalizedString TalentFeatureGroupLabel;

	public LocalizedString UltimateAbilityFeatureGroupLabel;

	public LocalizedString ActiveAbilityFeatureGroupLabel;

	public LocalizedString AttributeFeatureGroupHint;

	public LocalizedString SkillFeatureGroupHint;

	public LocalizedString TalentFeatureGroupHint;

	public LocalizedString AscensionFirstCareerAbilityFeatureGroupLabel;

	public LocalizedString AscensionSecondCareerAbilityFeatureGroupLabel;

	public LocalizedString AscensionFirstOrSecondCareerAbilityFeatureGroupLabel;

	public LocalizedString BackgroundAbilityFeatureGroupLabel;

	public LocalizedString AscensionTalentFeatureGroupLabel;

	public LocalizedString AscensionFirstCareerTalentFeatureGroupLabel;

	public LocalizedString AscensionSecondCareerTalentFeatureGroupLabel;

	public LocalizedString AscensionFirstOrSecondCareerTalentFeatureGroupLabel;

	public LocalizedString CommonTalentFeatureGroupLabel;

	public LocalizedString AscensionFirstCareerAbilityFeatureGroupHint;

	public LocalizedString AscensionSecondCareerAbilityFeatureGroupHint;

	public LocalizedString AscensionFirstOrSecondCareerAbilityFeatureGroupHint;

	public LocalizedString BackgroundAbilityFeatureGroupHint;

	public LocalizedString AscensionTalentFeatureGroupHint;

	public LocalizedString AscensionFirstCareerTalentFeatureGroupHint;

	public LocalizedString AscensionSecondCareerTalentFeatureGroupHint;

	public LocalizedString AscensionFirstOrSecondCareerTalentFeatureGroupHint;

	public LocalizedString CommonTalentFeatureGroupHint;

	public LocalizedString UltimateAbilityFeatureGroupHint;

	public LocalizedString ActiveAbilityFeatureGroupHint;

	public LocalizedString ChooseAttributeFeatureGroupHint;

	public LocalizedString ChooseSkillFeatureGroupHint;

	public LocalizedString ChooseAdvancedAbilityGroupHint;

	public LocalizedString ActiveAdvancedAbilityGroupHint;

	public LocalizedString ChooseTalentFeatureGroupHint;

	public LocalizedString ChooseUltimateAbilityFeatureGroupHint;

	public LocalizedString ChooseActiveAbilityFeatureGroupHint;

	public LocalizedString ChooseAscensionFirstCareerAbilityFeatureGroupHint;

	public LocalizedString ChooseAscensionSecondCareerAbilityFeatureGroupHint;

	public LocalizedString ChooseAscensionFirstOrSecondCareerAbilityFeatureGroupHint;

	public LocalizedString ChooseBackgroundAbilityFeatureGroupHint;

	public LocalizedString ChooseAscensionTalentFeatureGroupHint;

	public LocalizedString ChooseAscensionFirstCareerTalentFeatureGroupHint;

	public LocalizedString ChooseAscensionSecondCareerTalentFeatureGroupHint;

	public LocalizedString ChooseAscensionFirstOrSecondCareerTalentFeatureGroupHint;

	public LocalizedString ChooseCommonTalentFeatureGroupHint;

	public LocalizedString KeystoneFeaturesChargenDescription;

	public LocalizedString UltimateAbilitiesChargenDescription;

	public LocalizedString PredefinedAbilitiesChargenDescription;

	public LocalizedString ActiveAbilitiesLabel;

	public LocalizedString PassiveAbilitiesLabel;

	public LocalizedString NoAbilitiesLabel;

	public LocalizedString NoModificationsHint;

	public LocalizedString ActionPanelLabel;

	public LocalizedString Chaos;

	public LocalizedString Human;

	public LocalizedString Xenos;

	[Header("Feature Groups Description")]
	public LocalizedString AscensionFirstCareerAbilityFeatureGroupDescription;

	public LocalizedString AscensionSecondCareerAbilityFeatureGroupDescription;

	public LocalizedString AscensionFirstOrSecondCareerAbilityFeatureGroupDescription;

	public LocalizedString AscensionFirstCareerTalentFeatureGroupDescription;

	public LocalizedString AscensionSecondCareerTalentFeatureGroupDescription;

	public LocalizedString AscensionFirstOrSecondCareerTalentFeatureGroupDescription;

	public LocalizedString AscensionMissingOnlySecondCareerAbilityFeatureGroupDescription;

	public LocalizedString AscensionMissingOnlySecondCareerTalentFeatureGroupDescription;

	[Header("Visual Settings")]
	public LocalizedString VisualSettingsTitle;

	public LocalizedString VisualSettingsShowHelmet;

	public LocalizedString VisualSettingsShowBackpack;

	public LocalizedString VisualSettingsShowCloak;

	public LocalizedString VisualSettingsShowGloves;

	public LocalizedString VisualSettingsShowBoots;

	public LocalizedString VisualSettingsShowHood;

	public LocalizedString VisualSettingsShowArmor;

	public LocalizedString VisualSettingsShowBaseHeadwear;

	public LocalizedString VisualSettingsEnableClothes;

	public LocalizedString VisualSettingsDisabledForCharacter;

	[Header("Hints")]
	public LocalizedString AvailableRanksHint;

	public LocalizedString NoneHint;

	public LocalizedString RecommendedFilterHint;

	public LocalizedString FavoritesFilterHint;

	public LocalizedString OffenseFilterHint;

	public LocalizedString DefenseFilterHint;

	public LocalizedString SupportFilterHint;

	public LocalizedString UniversalFilterHint;

	public LocalizedString ArchetypeFilterHint;

	public LocalizedString OriginFilterHint;

	public LocalizedString WarpFilterHint;

	public LocalizedString ShowUnavailableFeaturesHint;

	public LocalizedString HideUnavailableFeaturesHint;

	public LocalizedString GetMenuLabel(CharInfoPageType pageType)
	{
		return pageType switch
		{
			CharInfoPageType.Summary => Summary, 
			CharInfoPageType.Features => Features, 
			CharInfoPageType.PsykerPowers => PsykerPowers, 
			CharInfoPageType.LevelProgression => LevelProgression, 
			CharInfoPageType.Biography => Biography, 
			CharInfoPageType.FactionsReputation => FactionsReputation, 
			CharInfoPageType.Modifiers => UIStrings.Instance.AbilityModifications.BindModificationsButtonLabel, 
			CharInfoPageType.Abilities => Abilities, 
			CharInfoPageType.Archetype => Career, 
			CharInfoPageType.Characteristics => Stats, 
			CharInfoPageType.Convictions => Convictions, 
			_ => null, 
		};
	}

	public string GetFactionLabel(FactionType factionType)
	{
		return factionType switch
		{
			FactionType.Drusians => Drusians, 
			FactionType.Explorators => Explorators, 
			FactionType.Kasballica => Kasballica, 
			FactionType.AstraMilitarum => FactionAstraMilitarum, 
			FactionType.AdeptusMechanicus => FactionAdeptusMechanicus, 
			FactionType.RulingCouncilOfNobles => FactionRulingCouncilOfNobles, 
			FactionType.MinistorumAndRedemptionists => FactionMinistorumAndRedemptionists, 
			_ => string.Empty, 
		};
	}

	public string GetFactionDescription(FactionType factionType)
	{
		return factionType switch
		{
			FactionType.Drusians => DrusiansDescription, 
			FactionType.Explorators => ExploratorsDescription, 
			FactionType.Kasballica => KasballicaDescription, 
			_ => string.Empty, 
		};
	}

	public string GetFeatureGroupLabel(FeatureGroup featureGroup)
	{
		return featureGroup switch
		{
			FeatureGroup.Attribute => AttributeFeatureGroupLabel, 
			FeatureGroup.Skill => SkillFeatureGroupLabel, 
			FeatureGroup.Talent => TalentFeatureGroupLabel, 
			FeatureGroup.ActiveAbility => ActiveAbilityFeatureGroupLabel, 
			FeatureGroup.AscensionTalent => AscensionTalentFeatureGroupLabel, 
			FeatureGroup.FirstCareerAbility => AscensionFirstCareerAbilityFeatureGroupLabel, 
			FeatureGroup.FirstCareerTalent => AscensionFirstCareerTalentFeatureGroupLabel, 
			FeatureGroup.SecondCareerAbility => AscensionSecondCareerAbilityFeatureGroupLabel, 
			FeatureGroup.SecondCareerTalent => AscensionSecondCareerTalentFeatureGroupLabel, 
			FeatureGroup.FirstOrSecondCareerAbility => AscensionFirstOrSecondCareerAbilityFeatureGroupLabel, 
			FeatureGroup.FirstOrSecondCareerTalent => AscensionFirstOrSecondCareerTalentFeatureGroupLabel, 
			FeatureGroup.CommonTalent => CommonTalentFeatureGroupLabel, 
			FeatureGroup.BackgroundFeature => BackgroundAbilityFeatureGroupLabel, 
			FeatureGroup.UltimateAbility => UltimateAbilityFeatureGroupLabel, 
			_ => throw new ArgumentOutOfRangeException("featureGroup", featureGroup, "GetFeatureGroupLabel"), 
		};
	}

	public string GetFeatureGroupDescription(FeatureGroup featureGroup)
	{
		return featureGroup switch
		{
			FeatureGroup.FirstCareerAbility => AscensionFirstCareerAbilityFeatureGroupDescription, 
			FeatureGroup.FirstCareerTalent => AscensionFirstCareerTalentFeatureGroupDescription, 
			FeatureGroup.SecondCareerAbility => AscensionSecondCareerAbilityFeatureGroupDescription, 
			FeatureGroup.SecondCareerTalent => AscensionSecondCareerTalentFeatureGroupDescription, 
			FeatureGroup.FirstOrSecondCareerAbility => AscensionFirstOrSecondCareerAbilityFeatureGroupDescription, 
			FeatureGroup.FirstOrSecondCareerTalent => AscensionFirstOrSecondCareerTalentFeatureGroupDescription, 
			_ => "", 
		};
	}

	public string GetFeatureGroupHint(FeatureGroup featureGroup, bool canChoose)
	{
		return featureGroup switch
		{
			FeatureGroup.Attribute => canChoose ? ChooseAttributeFeatureGroupHint : AttributeFeatureGroupHint, 
			FeatureGroup.Skill => canChoose ? ChooseSkillFeatureGroupHint : SkillFeatureGroupHint, 
			FeatureGroup.Talent => canChoose ? ChooseTalentFeatureGroupHint : TalentFeatureGroupHint, 
			FeatureGroup.AscensionTalent => canChoose ? ChooseAscensionTalentFeatureGroupHint : AscensionTalentFeatureGroupHint, 
			FeatureGroup.FirstCareerAbility => canChoose ? ChooseAscensionFirstCareerAbilityFeatureGroupHint : AscensionFirstCareerAbilityFeatureGroupHint, 
			FeatureGroup.FirstCareerTalent => canChoose ? ChooseAscensionFirstCareerTalentFeatureGroupHint : AscensionFirstCareerTalentFeatureGroupHint, 
			FeatureGroup.SecondCareerAbility => canChoose ? ChooseAscensionSecondCareerAbilityFeatureGroupHint : AscensionSecondCareerAbilityFeatureGroupHint, 
			FeatureGroup.SecondCareerTalent => canChoose ? ChooseAscensionSecondCareerTalentFeatureGroupHint : AscensionSecondCareerTalentFeatureGroupHint, 
			FeatureGroup.FirstOrSecondCareerAbility => canChoose ? ChooseAscensionFirstOrSecondCareerAbilityFeatureGroupHint : AscensionFirstOrSecondCareerAbilityFeatureGroupHint, 
			FeatureGroup.FirstOrSecondCareerTalent => canChoose ? ChooseAscensionFirstOrSecondCareerTalentFeatureGroupHint : AscensionFirstOrSecondCareerTalentFeatureGroupHint, 
			FeatureGroup.CommonTalent => canChoose ? ChooseCommonTalentFeatureGroupHint : CommonTalentFeatureGroupHint, 
			FeatureGroup.BackgroundFeature => canChoose ? ChooseBackgroundAbilityFeatureGroupHint : BackgroundAbilityFeatureGroupHint, 
			FeatureGroup.ActiveAbility => canChoose ? ChooseActiveAbilityFeatureGroupHint : ActiveAbilityFeatureGroupHint, 
			FeatureGroup.UltimateAbility => canChoose ? ChooseUltimateAbilityFeatureGroupHint : UltimateAbilityFeatureGroupHint, 
			FeatureGroup.AdvancedAbility => canChoose ? ChooseAdvancedAbilityGroupHint : ActiveAdvancedAbilityGroupHint, 
			_ => string.Empty, 
		};
	}
}
