using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Rules.Modifiers;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UITooltips
{
	public LocalizedString CannotbeEquip;

	public LocalizedString CanBeEquip;

	public LocalizedString CanBeUsed;

	public LocalizedString CannotbeUsed;

	public LocalizedString IsNotRemovable;

	public LocalizedString Loot;

	public LocalizedString Door;

	public LocalizedString DoorOpen;

	public LocalizedString DoorClose;

	public LocalizedString Trap;

	public LocalizedString TrapNeutralize;

	public LocalizedString Ladder;

	public LocalizedString TitlePreviewSkillcheckSkillDC;

	public LocalizedString TipPreviewSkillcheckBestCharacter;

	public LocalizedString NoFeature;

	public LocalizedString and;

	public LocalizedString or;

	public LocalizedString[] EncumbranceStatus = new LocalizedString[4];

	public LocalizedString CurrentValue;

	public LocalizedString BaseValue;

	public LocalizedString TotalValue;

	public LocalizedString TotalSkillValue;

	public LocalizedString TotalAttributeValue;

	public LocalizedString BonusValue;

	public LocalizedString UnitIsNotInspected;

	public LocalizedString CurrentLevelExperience;

	public LocalizedString NextLevelExperience;

	public LocalizedString TillNextLevelExperience;

	public LocalizedString RelatedSkills;

	public LocalizedString UpgradingLimit;

	public LocalizedString Require;

	public LocalizedString RequireInverted;

	public LocalizedString FittingAbilities;

	public LocalizedString NoSuitableAbilities;

	public LocalizedString AbilityAPCost;

	public LocalizedString ToggleAbilityType;

	public LocalizedString Before;

	public LocalizedString After;

	public LocalizedString OneHanded;

	public LocalizedString TwoHanded;

	public LocalizedString NoItemsAvailableToSelect;

	public LocalizedString NonStackHeaderLabel;

	public LocalizedString ShowInfo;

	public LocalizedString Source;

	public LocalizedString PermanentSources;

	public LocalizedString TemporarySources;

	public LocalizedString CostAP;

	public LocalizedString AP;

	public LocalizedString MP;

	public LocalizedString PsychicPowerCostAP;

	public LocalizedString PsykerPower;

	public LocalizedString EndsTurn;

	public LocalizedString SpendAllMovementPoints;

	public LocalizedString AttackAbilityGroupCooldown;

	public LocalizedString VeilDegradation;

	public LocalizedString ShotsCount;

	public LocalizedString EndTurn;

	public LocalizedString SpeedUpEnemies;

	public LocalizedString SpeedingUp;

	public LocalizedString HeroicActAbility;

	public LocalizedString DesperateMeasureAbility;

	public LocalizedString HitChances;

	public LocalizedString HitChancesEffectiveDistance;

	public LocalizedString HitChancesMaxDistance;

	public LocalizedString AbilityDistance;

	public LocalizedString AttachedModifiers;

	public LocalizedString SpendAllMovementPointsShort;

	public LocalizedString AttackAbilityGroupCooldownShort;

	public LocalizedString IncreaseVeilDegradationShort;

	public LocalizedString ArmourDamageReduceDescription;

	[Obsolete("Defence")]
	public LocalizedString ArmourDodgeChanceDescription;

	public LocalizedString ReplenishingItem;

	public LocalizedString ScatterMainLineClose;

	public LocalizedString ScatterClose;

	public LocalizedString ScatterMainLine;

	public LocalizedString ScatterNear;

	public LocalizedString ScatterFar;

	public LocalizedString DifficultyReduceDescription;

	public LocalizedString HasHPDamageBonus;

	public LocalizedString HasArmorDamageBonus;

	public LocalizedString TotalDamage;

	public LocalizedString Damage;

	public LocalizedString InitialDamage;

	public LocalizedString PossibleHits;

	public LocalizedString EveryRound;

	public LocalizedString TotalHeal;

	public LocalizedString HPHeal;

	public LocalizedString ArmorHeal;

	public LocalizedString HasNoArmorToRepair;

	public LocalizedString NoLineOfSight;

	public LocalizedString NoHit;

	public LocalizedString TotalHitChance;

	public LocalizedString InitialHitChance;

	public LocalizedString CriticalEffectsAvoidanceChance;

	public LocalizedString Cover;

	public LocalizedString Defence;

	public LocalizedString DefencePenalty;

	public LocalizedString MaxDefence;

	public LocalizedString Overpenetration;

	public LocalizedString DamageReduction;

	public LocalizedString YouWillGainTitle;

	public LocalizedString YouWillLoseTitle;

	public LocalizedString CommonFeatureDesc;

	[Header("Hints")]
	public LocalizedString ShowTooltipHint;

	[Header("Prerequisites")]
	public LocalizedString Prerequisites;

	public LocalizedString PrerequisiteAbilities;

	public LocalizedString PrerequisiteCareers;

	public LocalizedString PrerequisiteFeatures;

	public LocalizedString PrerequisiteRank;

	public LocalizedString PrerequisiteLevel;

	public LocalizedString PrerequisitesFooter;

	[Header("CharGen")]
	public LocalizedString DoctrinesHeader;

	public LocalizedString DoctrinesShortDesc;

	public LocalizedString DoctrinesDescription;

	public LocalizedString AbilitiesWillBeLostHeader;

	public LocalizedString AbilitiesWillBeLostDescription;

	public LocalizedString NotEquipped;

	[Header("Detective")]
	public LocalizedString RelatedDetectiveItemsTitle;

	public LocalizedString DetectiveCloseCaseDataTitle;

	public LocalizedString RelatedDetectiveItemsDescription;

	public LocalizedString CriticalEffectHint;

	[Header("Tags")]
	public LocalizedString WeaponTagBrutal;

	public LocalizedString WeaponTagDestructive;

	public LocalizedString WeaponTagVital;

	[Header("Formats")]
	public LocalizedString ModifierPctAdd;

	public LocalizedString ModifierPctMul;

	[Header("Abilities")]
	public LocalizedString AbilityScalingCharacteristics;

	public LocalizedString AbilityRestrictionsTitle;

	public LocalizedString LogicalOr;

	public LocalizedString GetWeaponTagLabel(SpecialWeaponDamageType tag)
	{
		return tag switch
		{
			SpecialWeaponDamageType.Brutal => WeaponTagBrutal, 
			SpecialWeaponDamageType.Destructive => WeaponTagDestructive, 
			SpecialWeaponDamageType.Vital => WeaponTagVital, 
			_ => null, 
		};
	}

	public string GetModifierFormat(ModifierType type)
	{
		return type switch
		{
			ModifierType.PctAdd => ModifierPctAdd, 
			ModifierType.PctMul => ModifierPctMul, 
			_ => "{0}", 
		};
	}
}
