using System;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UITooltips
{
	public LocalizedString BaseDamage;

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

	public LocalizedString Before;

	public LocalizedString After;

	[Obsolete]
	public LocalizedString TwoHanded;

	public LocalizedString OneHanded;

	public LocalizedString NoItemsAvailableToSelect;

	public LocalizedString NonStackHeaderLabel;

	public LocalizedString ShowInfo;

	public LocalizedString Source;

	public LocalizedString Sources;

	[Obsolete]
	public LocalizedString RateOfFire;

	public LocalizedString RateOfFireMelee;

	[Obsolete]
	public LocalizedString Recoil;

	public LocalizedString MaximumRange;

	public LocalizedString CostAP;

	public LocalizedString AP;

	public LocalizedString MP;

	public LocalizedString PsychicPowerCostAP;

	public LocalizedString PsykerPower;

	public LocalizedString EndsTurn;

	public LocalizedString SpendAllMovementPoints;

	public LocalizedString AttackAbilityGroupCooldown;

	public LocalizedString MajorVeilDegradation;

	public LocalizedString ShotsCount;

	public LocalizedString EndTurn;

	public LocalizedString SpeedUpEnemies;

	public LocalizedString SpeedingUp;

	public LocalizedString HeroicActAbility;

	public LocalizedString DesperateMeasureAbility;

	public LocalizedString HitChances;

	public LocalizedString HitChancesEffectiveDistance;

	public LocalizedString HitChancesMaxDistance;

	public LocalizedString ItemFooterLabel;

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

	public LocalizedString BonusesSum;

	public LocalizedString BaseChance;

	public LocalizedString DifficultyReduceDescription;

	public LocalizedString HPLeft;

	public LocalizedString HPMax;

	public LocalizedString HPTemporary;

	public LocalizedString HPTotalLeft;

	public LocalizedString HPTotalMax;

	public LocalizedString ArmorLeft;

	public LocalizedString ArmorMax;

	public LocalizedString HasHPDamageBonus;

	public LocalizedString HasArmorDamageBonus;

	public LocalizedString PossibleToKill;

	public LocalizedString TotalDamage;

	public LocalizedString Damage;

	public LocalizedString InitialDamage;

	public LocalizedString BurstCount;

	public LocalizedString PossibleHits;

	public LocalizedString PossibleToPush;

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

	[Obsolete]
	public LocalizedString DodgeAvoidance;

	[Obsolete]
	public LocalizedString ParryAvoidance;

	[Obsolete]
	public LocalizedString CoverAvoidance;

	public LocalizedString Cover;

	public LocalizedString Defence;

	public LocalizedString DefencePenalty;

	public LocalizedString Overpenetration;

	public LocalizedString DamageReduction;

	public LocalizedString YouWillGainTitle;

	public LocalizedString YouWillLoseTitle;

	public LocalizedString ReputationPointsAbbreviation;

	public LocalizedString CommonFeatureDesc;

	[Header("Hints")]
	public LocalizedString ShowTooltipHint;

	public LocalizedString HideTooltipHint;

	public LocalizedString ShowComparativeHint;

	public LocalizedString HideComparativeHint;

	[Header("SoulMarks")]
	public LocalizedString SoulMarkRankHeader;

	public LocalizedString SoulMarkRankDescription;

	public LocalizedString SoulMarkIsLocked;

	public LocalizedString SoulMarkMayBeLocked;

	[Header("Prerequisites")]
	public LocalizedString Prerequisites;

	public LocalizedString PrerequisiteAbilities;

	public LocalizedString PrerequisiteCareers;

	public LocalizedString PrerequisiteFeatures;

	public LocalizedString PrerequisiteRank;

	public LocalizedString PrerequisiteLevel;

	public LocalizedString PrerequisitesFooter;

	public LocalizedString ToCurrentPrerequisiteFeature;

	[Header("CharGen")]
	public LocalizedString DoctrinesHeader;

	public LocalizedString DoctrinesShortDesc;

	public LocalizedString DoctrinesDescription;

	public LocalizedString AbilitiesWillBeLostHeader;

	public LocalizedString AbilitiesWillBeLostDescription;

	public LocalizedString NotEquipped;

	[Header("Detective")]
	public LocalizedString RelatedDetectiveItemsTitle;

	public LocalizedString RelatedDetectiveItemsDescription;

	public LocalizedString CriticalEffectHint;

	[Header("Tags")]
	public LocalizedString WeaponTagBrutal;

	public LocalizedString WeaponTagDestructive;

	public LocalizedString GetWeaponTagLabel(WeaponTagProperty tag)
	{
		return tag switch
		{
			WeaponTagProperty.Brutal => WeaponTagBrutal, 
			WeaponTagProperty.Destructive => WeaponTagDestructive, 
			_ => null, 
		};
	}
}
