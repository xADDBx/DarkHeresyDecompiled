using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Enums;
using Kingmaker.Gameplay.Blueprints.Root;
using Kingmaker.Gameplay.Features.Items.Utility;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Enum;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Items.Weapons;

[ComponentName("Items/BlueprintItemWeapon")]
[TypeId("c00f723cccf2d314198c42a572c631fd")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintItemWeapon : BlueprintItemEquipmentHand
{
	public WeaponProgressionType ProgressionType;

	public ItemPowerLevel PowerLevel;

	[FormerlySerializedAs("m_Modifiers")]
	[ValidateNoNullEntries]
	public BpRef<BlueprintAbilityModifier>[] AbilityModifiers = new BpRef<BlueprintAbilityModifier>[0];

	[SerializeField]
	[JsonProperty(PropertyName = "AbilityContainer")]
	public WeaponAbilityContainer WeaponAbilities;

	[SerializeField]
	[ShowIf("IsMelee")]
	private BlueprintAbilityReference m_AttackOfOpportunityAbility;

	[SerializeField]
	[ShowIf("IsMelee")]
	private BlueprintAbilityFXSettings.Reference m_AttackOfOpportunityAbilityFXSettings;

	public WeaponCategory Category;

	public WeaponFamily Family;

	public WeaponClassification Classification;

	public ItemFaction Faction;

	[Space]
	[SerializeField]
	private WeaponHoldingType m_HoldingType;

	[SerializeField]
	private WeaponRange m_Range;

	[SerializeField]
	private WeaponHeaviness m_Heaviness;

	[Space]
	public bool CanBeUsedInGame;

	[Space]
	[SerializeField]
	private LocalizedString m_TypeNameText;

	[SerializeField]
	private LocalizedString m_DefaultNameText;

	[SerializeField]
	private LocalizedString m_DescriptionText;

	[HideIf("IsDoubleHanded")]
	[Tooltip("Двуручное оружие, если true, то нельзя сделать его Double (двойным)")]
	public bool IsTwoHanded;

	[HideIf("IsTwoHanded")]
	[Tooltip("Двойное оружие, во вторую руку экипируется SecondWeapon, не может быть двуручным (IsTwoHanded)")]
	public bool IsDoubleHanded;

	[ShowIf("IsDoubleHanded")]
	[SerializeField]
	private BpRef<BlueprintItemWeapon> m_SecondWeapon;

	[SerializeField]
	private bool m_OverrideDamage;

	[SerializeField]
	[ShowIf("OverrideDamage")]
	private int m_MinDamage;

	[SerializeField]
	[ShowIf("OverrideDamage")]
	private int m_MaxDamage;

	[SerializeField]
	private bool m_OverrideVitalDamage;

	[SerializeField]
	[ShowIf("OverrideVitalDamage")]
	private int m_VitalDamage;

	[SerializeField]
	private bool m_OverrideBrutalDamage;

	[SerializeField]
	[ShowIf("OverrideBrutalDamage")]
	private int m_BrutalDamage;

	[SerializeField]
	private bool m_OverrideDestructiveDamage;

	[SerializeField]
	[ShowIf("OverrideDestructiveDamage")]
	private int m_DestructiveDamage;

	[SerializeField]
	private bool m_OverrideAdditionalHitChance;

	[SerializeField]
	[ShowIf("OverrideAdditionalHitChance")]
	private int m_AdditionalHitChance;

	[SerializeField]
	private bool m_OverrideOverpenetrationChance;

	[SerializeField]
	[ShowIf("OverrideOverpenetrationChance")]
	[Range(0f, 100f)]
	private int m_OverpenetrationChance;

	[SerializeField]
	private bool m_OverrideRateOfFire;

	[SerializeField]
	[ShowIf("OverrideRateOfFire")]
	private int m_RateOfFire;

	[SerializeField]
	private bool m_OverrideRecoil;

	[Obsolete("WH2")]
	public int WarhammerPenetration;

	[Range(0f, 1f)]
	public int AdditionalCrits;

	[Range(0f, 3f)]
	public int CritsThroughArmor;

	public DamageStrategy DamageApplyStrategy;

	public int WarhammerMaxDistance;

	[ShowIf("OverrideRecoil")]
	public int Recoil;

	[SerializeField]
	private DamageTypeSettings m_DamageType;

	[SerializeField]
	private bool m_IsNatural;

	[SerializeField]
	private DamageStatBonusFactor m_DamageStatBonusFactor;

	private static BlueprintItemProgressionRoot Progression => ConfigRoot.Instance.ItemProgressionRoot;

	public BlueprintItemWeapon SecondWeapon => m_SecondWeapon?.Blueprint;

	private bool OverrideDamage
	{
		get
		{
			if (!m_OverrideDamage)
			{
				return !Progression.EnableForWeapons;
			}
			return true;
		}
	}

	private bool OverrideVitalDamage
	{
		get
		{
			if (!m_OverrideVitalDamage)
			{
				return !Progression.EnableForWeapons;
			}
			return true;
		}
	}

	private bool OverrideBrutalDamage
	{
		get
		{
			if (!m_OverrideBrutalDamage)
			{
				return !Progression.EnableForWeapons;
			}
			return true;
		}
	}

	private bool OverrideDestructiveDamage
	{
		get
		{
			if (!m_OverrideDestructiveDamage)
			{
				return !Progression.EnableForWeapons;
			}
			return true;
		}
	}

	private bool OverrideAdditionalHitChance
	{
		get
		{
			if (!m_OverrideAdditionalHitChance)
			{
				return !Progression.EnableForWeapons;
			}
			return true;
		}
	}

	private bool OverrideOverpenetrationChance
	{
		get
		{
			if (!m_OverrideOverpenetrationChance)
			{
				return !Progression.EnableForWeapons;
			}
			return true;
		}
	}

	private bool OverrideRateOfFire
	{
		get
		{
			if (!m_OverrideRateOfFire)
			{
				return !Progression.EnableForWeapons;
			}
			return true;
		}
	}

	private bool OverrideRecoil
	{
		get
		{
			if (!m_OverrideRecoil)
			{
				return !Progression.EnableForWeapons;
			}
			return true;
		}
	}

	public bool IsAnyProgressionOverride
	{
		get
		{
			if (!m_OverrideDamage && !m_OverrideVitalDamage && !m_OverrideBrutalDamage && !m_OverrideDestructiveDamage && !m_OverrideAdditionalHitChance)
			{
				return m_OverrideOverpenetrationChance;
			}
			return true;
		}
	}

	public override IEnumerable<BlueprintAbility> Abilities => base.Abilities.Concat(WeaponAbilities.Select((WeaponAbility i) => i.Ability).NotNull());

	public override bool GainAbility
	{
		get
		{
			if (!base.GainAbility)
			{
				return WeaponAbilities.Any();
			}
			return true;
		}
	}

	public override string SubtypeName => m_TypeNameText;

	public override string SubtypeDescription => m_DescriptionText;

	public BlueprintAbility AttackOfOpportunityAbility
	{
		get
		{
			if (!IsMelee || m_AttackOfOpportunityAbility == null)
			{
				return null;
			}
			return m_AttackOfOpportunityAbility.Get();
		}
	}

	public BlueprintAbilityFXSettings AttackOfOpportunityAbilityFXSettings
	{
		get
		{
			if (!IsMelee || m_AttackOfOpportunityAbilityFXSettings == null)
			{
				return null;
			}
			return m_AttackOfOpportunityAbilityFXSettings.Get();
		}
	}

	public IEnumerable<WeaponTagUISettings> WeaponTags => from f in base.ComponentsArray.OfType<AddFactToEquipmentWielder>()
		select f.Fact.GetComponent<WeaponTagUISettings>() into tag
		where tag != null
		select tag;

	public override string Name
	{
		get
		{
			if (!string.IsNullOrEmpty(base.Name))
			{
				return base.Name;
			}
			string text = m_DefaultNameText;
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			if (!string.IsNullOrEmpty(m_TypeNameText))
			{
				return m_TypeNameText;
			}
			string weaponCategoryLabel = UIStrings.Instance.WeaponCategories.GetWeaponCategoryLabel(Category);
			if (string.IsNullOrEmpty(weaponCategoryLabel))
			{
				return UIStrings.Instance.CharacterSheet.Attack;
			}
			return weaponCategoryLabel;
		}
	}

	public override string Description
	{
		get
		{
			string description = base.Description;
			if (!string.IsNullOrWhiteSpace(description))
			{
				return description;
			}
			return m_DescriptionText;
		}
	}

	public override ItemsItemType ItemType => ItemsItemType.Weapon;

	public AttackType AttackType
	{
		get
		{
			if (!IsMelee)
			{
				return AttackType.Ranged;
			}
			return AttackType.Melee;
		}
	}

	public int AttackRange => WarhammerMaxDistance;

	public int AttackOptimalRange
	{
		get
		{
			if (!IsMelee)
			{
				return WarhammerMaxDistance / 2;
			}
			return WarhammerMaxDistance;
		}
	}

	public DamageTypeSettings DamageType => m_DamageType;

	public bool IsNatural => m_IsNatural;

	public bool IsRanged => !IsMelee;

	public bool IsMelee => Category == WeaponCategory.Melee;

	public override float Weight => m_Weight;

	public WeaponHoldingType HoldingType => m_HoldingType;

	public WeaponRange Range => m_Range;

	public WeaponHeaviness Heaviness => m_Heaviness;

	public DamageStatBonusFactor DamageStatBonusFactor => m_DamageStatBonusFactor;

	public int GetDamageMin(ItemPowerLevel powerLevel = ItemPowerLevel.Undefined)
	{
		if (!OverrideDamage)
		{
			return Progression.Get(ProgressionType, ResolvePowerLevel(powerLevel)).DamageMin;
		}
		return m_MinDamage;
	}

	public int GetDamageMax(ItemPowerLevel powerLevel = ItemPowerLevel.Undefined)
	{
		if (!OverrideDamage)
		{
			return Progression.Get(ProgressionType, ResolvePowerLevel(powerLevel)).DamageMax;
		}
		return m_MaxDamage;
	}

	public int GetDamageVital(ItemPowerLevel powerLevel = ItemPowerLevel.Undefined)
	{
		if (!OverrideVitalDamage)
		{
			return Progression.Get(ProgressionType, ResolvePowerLevel(powerLevel)).DamageVital;
		}
		return m_VitalDamage;
	}

	public int GetBrutalDamage(ItemPowerLevel powerLevel = ItemPowerLevel.Undefined)
	{
		if (!OverrideBrutalDamage)
		{
			return Progression.Get(ProgressionType, ResolvePowerLevel(powerLevel)).BrutalDamage;
		}
		return m_BrutalDamage;
	}

	public int GetDestructiveDamage(ItemPowerLevel powerLevel = ItemPowerLevel.Undefined)
	{
		if (!OverrideDestructiveDamage)
		{
			return Progression.Get(ProgressionType, ResolvePowerLevel(powerLevel)).DestructiveDamage;
		}
		return m_DestructiveDamage;
	}

	public int GetAdditionalHitChance(ItemPowerLevel powerLevel = ItemPowerLevel.Undefined)
	{
		if (!OverrideAdditionalHitChance)
		{
			return Progression.Get(ProgressionType, ResolvePowerLevel(powerLevel)).HitChanceBonus;
		}
		return m_AdditionalHitChance;
	}

	public int GetOverpenetrationChance(ItemPowerLevel powerLevel = ItemPowerLevel.Undefined)
	{
		if (!OverrideOverpenetrationChance)
		{
			return Progression.Get(ProgressionType, ResolvePowerLevel(powerLevel)).OverpenetrationChance;
		}
		return m_OverpenetrationChance;
	}

	public int GetRateOfFire(ItemPowerLevel powerLevel = ItemPowerLevel.Undefined)
	{
		if (!OverrideRateOfFire)
		{
			return Progression.Get(ProgressionType, ResolvePowerLevel(powerLevel)).RateOfFire;
		}
		return m_RateOfFire;
	}

	public int GetRecoil(ItemPowerLevel powerLevel = ItemPowerLevel.Undefined)
	{
		if (!OverrideRecoil)
		{
			return Progression.Get(ProgressionType, ResolvePowerLevel(powerLevel)).Recoil;
		}
		return Recoil;
	}

	internal ItemPowerLevel ResolvePowerLevel(ItemPowerLevel powerLevel)
	{
		if (powerLevel == ItemPowerLevel.Undefined)
		{
			return PowerLevel;
		}
		return powerLevel;
	}
}
