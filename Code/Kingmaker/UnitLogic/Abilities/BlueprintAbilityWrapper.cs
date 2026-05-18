using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation;
using Owlcat.Fmw.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities;

public class BlueprintAbilityWrapper : IHashable
{
	private BlueprintAbility m_Blueprint;

	private BlueprintAbilityModifier[] m_AllModifiers;

	private AbilityRange? m_Range;

	private int? m_CustomRange;

	private int? m_MinRange;

	private int? m_ActionPointCost;

	private int? m_VeilDamage;

	private int? m_CooldownRounds;

	private bool? m_CanTargetSelf;

	private bool? m_CanTargetDestructibleObjects;

	private bool? m_CanTargetEnemies;

	private bool? m_CanTargetFriends;

	private bool? m_CanTargetPoint;

	private BlueprintAbility.UsingInThreateningAreaType? m_UsingInThreateningArea;

	public ReadonlyList<BlueprintAbilityModifier> AllModifiers => m_AllModifiers;

	public AbilityRange Range
	{
		get
		{
			AbilityRange valueOrDefault = m_Range.GetValueOrDefault();
			if (!m_Range.HasValue)
			{
				valueOrDefault = m_AllModifiers.LastItem((BlueprintAbilityModifier i) => i.Range.Enabled)?.Range.Value ?? m_Blueprint.Range;
				m_Range = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public int CustomRange
	{
		get
		{
			int valueOrDefault = m_CustomRange.GetValueOrDefault();
			if (!m_CustomRange.HasValue)
			{
				valueOrDefault = m_AllModifiers.LastItem((BlueprintAbilityModifier i) => i.CustomRange.Enabled)?.CustomRange.Value ?? m_Blueprint.CustomRange;
				m_CustomRange = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public int MinRange
	{
		get
		{
			int valueOrDefault = m_MinRange.GetValueOrDefault();
			if (!m_MinRange.HasValue)
			{
				valueOrDefault = m_AllModifiers.LastItem((BlueprintAbilityModifier i) => i.MinRange.Enabled)?.MinRange.Value ?? m_Blueprint.MinRange;
				m_MinRange = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public int ActionPointCost
	{
		get
		{
			int valueOrDefault = m_ActionPointCost.GetValueOrDefault();
			if (!m_ActionPointCost.HasValue)
			{
				valueOrDefault = m_AllModifiers.LastItem((BlueprintAbilityModifier i) => i.ActionPointCost.Enabled)?.ActionPointCost.Value ?? m_Blueprint.ActionPointCost;
				m_ActionPointCost = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public int VeilDamage
	{
		get
		{
			int valueOrDefault = m_VeilDamage.GetValueOrDefault();
			if (!m_VeilDamage.HasValue)
			{
				valueOrDefault = m_AllModifiers.LastItem((BlueprintAbilityModifier i) => i.VeilDamage.Enabled)?.VeilDamage.Value ?? m_Blueprint.VeilDamage;
				m_VeilDamage = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public int CooldownRounds
	{
		get
		{
			int valueOrDefault = m_CooldownRounds.GetValueOrDefault();
			if (!m_CooldownRounds.HasValue)
			{
				valueOrDefault = m_AllModifiers.LastItem((BlueprintAbilityModifier i) => i.CooldownRounds.Enabled)?.CooldownRounds.Value ?? m_Blueprint.CooldownRounds;
				m_CooldownRounds = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool CanTargetSelf
	{
		get
		{
			bool valueOrDefault = m_CanTargetSelf.GetValueOrDefault();
			if (!m_CanTargetSelf.HasValue)
			{
				valueOrDefault = m_AllModifiers.LastItem((BlueprintAbilityModifier i) => i.CanTargetSelf.Enabled)?.CanTargetSelf.Value ?? m_Blueprint.CanTargetSelf;
				m_CanTargetSelf = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool CanTargetDestructibleObjects
	{
		get
		{
			bool valueOrDefault = m_CanTargetDestructibleObjects.GetValueOrDefault();
			if (!m_CanTargetDestructibleObjects.HasValue)
			{
				valueOrDefault = m_AllModifiers.LastItem((BlueprintAbilityModifier i) => i.CanTargetDestructibleObjects.Enabled)?.CanTargetDestructibleObjects.Value ?? m_Blueprint.CanTargetDestructibleObjects;
				m_CanTargetDestructibleObjects = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool CanTargetEnemies
	{
		get
		{
			bool valueOrDefault = m_CanTargetEnemies.GetValueOrDefault();
			if (!m_CanTargetEnemies.HasValue)
			{
				valueOrDefault = m_AllModifiers.LastItem((BlueprintAbilityModifier i) => i.CanTargetEnemies.Enabled)?.CanTargetEnemies.Value ?? m_Blueprint.CanTargetEnemies;
				m_CanTargetEnemies = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool CanTargetFriends
	{
		get
		{
			bool valueOrDefault = m_CanTargetFriends.GetValueOrDefault();
			if (!m_CanTargetFriends.HasValue)
			{
				valueOrDefault = m_AllModifiers.LastItem((BlueprintAbilityModifier i) => i.CanTargetFriends.Enabled)?.CanTargetFriends.Value ?? m_Blueprint.CanTargetFriends;
				m_CanTargetFriends = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool CanTargetPoint
	{
		get
		{
			bool valueOrDefault = m_CanTargetPoint.GetValueOrDefault();
			if (!m_CanTargetPoint.HasValue)
			{
				valueOrDefault = m_AllModifiers.LastItem((BlueprintAbilityModifier i) => i.CanTargetPoint.Enabled)?.CanTargetPoint.Value ?? m_Blueprint.CanTargetPoint;
				m_CanTargetPoint = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool CanCastToDeadTarget => GetComponent<ICanTargetDeadUnits>() != null;

	public BlueprintAbility.UsingInThreateningAreaType UsingInThreateningArea
	{
		get
		{
			BlueprintAbility.UsingInThreateningAreaType valueOrDefault = m_UsingInThreateningArea.GetValueOrDefault();
			if (!m_UsingInThreateningArea.HasValue)
			{
				valueOrDefault = m_AllModifiers.LastItem((BlueprintAbilityModifier i) => i.UsingInThreateningArea.Enabled)?.UsingInThreateningArea.Value ?? m_Blueprint.UsingInThreateningArea;
				m_UsingInThreateningArea = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool CanUseInThreateningArea => UsingInThreateningArea == BlueprintAbility.UsingInThreateningAreaType.CanUse;

	public bool ProvokesAttackOfOpportunity => false;

	public bool HasVariants => m_Blueprint.HasVariants;

	public IAbilityResourceLogic ResourceLogic => m_Blueprint.GetComponent<IAbilityResourceLogic>();

	public bool NeedLoS
	{
		get
		{
			if (m_Blueprint.GetComponent<AbilityTargetIsDeadCompanion>() == null)
			{
				return m_Blueprint.GetComponent<AbilityIgnoreLineOfSight>() == null;
			}
			return false;
		}
	}

	public BlueprintAbility OriginalBlueprint => m_Blueprint;

	public bool IsCharge => m_Blueprint.IsCharge;

	public string Name => m_Blueprint.Name;

	public string Description => m_Blueprint.Description;

	public Sprite Icon => m_Blueprint.Icon;

	public string NameForAcronym => m_Blueprint.NameForAcronym;

	public BlueprintAbility.CombatStateRestrictionType CombatStateRestriction => m_Blueprint.CombatStateRestriction;

	public IEnumerable<IAbilityCasterRestriction> CasterRestrictions => m_Blueprint.CasterRestrictions;

	public bool IsWeaponAbility => m_Blueprint.IsWeaponAbility;

	public bool IsPsykerAbility => m_Blueprint.IsPsykerAbility;

	public IAbilityRestriction[] Restrictions => m_Blueprint.Restrictions;

	public bool IsFreeAction
	{
		get
		{
			if (!ContextData<AbilityData.ForceFreeAction>.Current)
			{
				return m_Blueprint.IsFreeAction;
			}
			return true;
		}
	}

	public BlueprintAbilityFXSettings FXSettings => m_Blueprint.FXSettings;

	public AbilityAttackType? AttackType => ComponentsArray.OfType<AbilityAttackDelivery>().FirstOrDefault()?.AttackType;

	public AttackAnimationType AttackAnimationType => m_Blueprint.AttackAnimationType;

	public bool UseBestShootingPosition => m_Blueprint.UseBestShootingPosition;

	public string name => m_Blueprint.name;

	public ReferenceArrayProxy<BlueprintAbilityGroup> AbilityGroups => m_Blueprint.AbilityGroups;

	public bool IsSummoningUnit => m_Blueprint.IsSummoningUnit;

	public IAbilityTargetRestriction[] TargetRestrictions => m_Blueprint.TargetRestrictions;

	public bool ShouldTurnToTarget => m_Blueprint.ShouldTurnToTarget;

	public bool IsGrenade => m_Blueprint.IsGrenade;

	public bool DisableLog => m_Blueprint.DisableLog;

	public AbilityTag AbilityTag => m_Blueprint.AbilityTag;

	public IAbilityAoEPatternProvider PatternSettings
	{
		get
		{
			for (int num = m_AllModifiers.Length - 1; num >= 0; num--)
			{
				IAbilityAoEPatternProvider component = m_AllModifiers[num].GetComponent<IAbilityAoEPatternProvider>();
				if (component != null)
				{
					return component;
				}
			}
			return m_Blueprint.PatternSettings;
		}
	}

	public string AssetGuid => m_Blueprint.AssetGuid;

	public WarhammerAbilityParamsSource AbilityParamsSource => m_Blueprint.AbilityParamsSource;

	public bool NotOffensive => m_Blueprint.NotOffensive;

	public bool IsMoveUnit => m_Blueprint.IsMoveUnit;

	public List<Element> ElementsArray => m_Blueprint.ElementsArray;

	public AbilityType Type => m_Blueprint.Type;

	public bool IsAoE
	{
		get
		{
			if (!m_Blueprint.IsAoE)
			{
				return GetComponent<AbilityTargetsInPattern>() != null;
			}
			return true;
		}
	}

	public bool IsRangedAoE => m_Blueprint.IsRangedAoe;

	public bool IsCoverIgnore => m_Blueprint.IsCoverIgnore;

	public bool IsBurst => m_Blueprint.IsBurst;

	public bool IsControlledBurst => m_Blueprint.IsControlledBurst;

	public bool IsSpell => m_Blueprint.IsSpell;

	public MechadendritesType UsedMechadendrite => m_Blueprint.UsedMechadendrite;

	public bool UseOnMechadendrite => m_Blueprint.UseOnMechadendrite;

	public bool HasAnimation => m_Blueprint.HasAnimation;

	public AbilityAnimationStyle SpellAnimation => m_Blueprint.SpellAnimation;

	public bool NeedEquipWeapons => m_Blueprint.NeedEquipWeapons;

	public bool CastInOffHand => m_Blueprint.CastInOffHand;

	public BlueprintAbility Parent => m_Blueprint.Parent;

	public TargetType AoETargets => PatternSettings?.Targets ?? TargetType.Any;

	public IEnumerable<BlueprintComponent> ComponentsArray => Components();

	public CombatHudCommandSetAsset CombatHudCommandsOverride => m_Blueprint.CombatHudCommandsOverride;

	public bool IsStratagem => m_Blueprint.IsStratagem;

	public IEnumerable<BlueprintAbility> Upgrades => m_Blueprint.Upgrades.Dereference();

	[CanBeNull]
	public BlueprintAbility BaseAbility => m_Blueprint.BaseAbility;

	public BpRef<BlueprintAbilityTag>[] Tags => m_Blueprint.Tags;

	public bool IsHeroic => ComponentsArray.Any((BlueprintComponent x) => x is AbilitySpecialMoraleAction abilitySpecialMoraleAction && abilitySpecialMoraleAction.MoralePhaseType == MoraleAbilityType.Heroic);

	public bool IsBroken => ComponentsArray.Any((BlueprintComponent x) => x is AbilitySpecialMoraleAction abilitySpecialMoraleAction && abilitySpecialMoraleAction.MoralePhaseType == MoraleAbilityType.Broken);

	public BlueprintAbilityWrapper(BlueprintAbility ability, IEnumerable<BlueprintAbilityModifier> modifiers)
	{
		m_Blueprint = ability;
		m_AllModifiers = modifiers.EmptyIfNull().ToArray();
	}

	public int GetBlueprintRange()
	{
		return Range switch
		{
			AbilityRange.Personal => 0, 
			AbilityRange.Touch => 1, 
			AbilityRange.Unlimited => 100000, 
			AbilityRange.Weapon => -1, 
			AbilityRange.Custom => CustomRange, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public string GetShortenedDescription()
	{
		return m_Blueprint.GetShortenedDescription();
	}

	public bool CanTargetPointAfterRestrictions(AbilityData abilityData)
	{
		return m_Blueprint.CanTargetPointAfterRestrictions(abilityData);
	}

	public bool TryGetComponent<T>(out T component)
	{
		component = GetComponent<T>();
		return component != null;
	}

	private IEnumerable<BlueprintComponent> Components()
	{
		for (int i = 0; i < m_Blueprint.ComponentsArray.Length; i++)
		{
			yield return m_Blueprint.ComponentsArray[i];
		}
		BlueprintAbilityModifier[] allModifiers = m_AllModifiers;
		foreach (BlueprintAbilityModifier modifier in allModifiers)
		{
			int i = 0;
			while (i < modifier.ComponentsArray.Length)
			{
				yield return modifier.ComponentsArray[i];
				int num = i + 1;
				i = num;
			}
		}
	}

	public IEnumerable<T> GetComponents<T>()
	{
		int i = 0;
		while (i < m_Blueprint.ComponentsArray.Length)
		{
			BlueprintComponent blueprintComponent = m_Blueprint.ComponentsArray[i];
			if (blueprintComponent is T)
			{
				yield return (T)(object)((blueprintComponent is T) ? blueprintComponent : null);
			}
			int num = i + 1;
			i = num;
		}
		BlueprintAbilityModifier[] allModifiers = m_AllModifiers;
		foreach (BlueprintAbilityModifier modifier in allModifiers)
		{
			i = 0;
			while (i < modifier.ComponentsArray.Length)
			{
				BlueprintComponent blueprintComponent = modifier.ComponentsArray[i];
				if (blueprintComponent is T)
				{
					yield return (T)(object)((blueprintComponent is T) ? blueprintComponent : null);
				}
				int num = i + 1;
				i = num;
			}
		}
	}

	[CanBeNull]
	public T GetComponent<T>()
	{
		for (int i = 0; i < m_Blueprint.ComponentsArray.Length; i++)
		{
			BlueprintComponent blueprintComponent = m_Blueprint.ComponentsArray[i];
			if (blueprintComponent is T)
			{
				return (T)(object)((blueprintComponent is T) ? blueprintComponent : null);
			}
		}
		BlueprintAbilityModifier[] allModifiers = m_AllModifiers;
		foreach (BlueprintAbilityModifier blueprintAbilityModifier in allModifiers)
		{
			for (int k = 0; k < blueprintAbilityModifier.ComponentsArray.Length; k++)
			{
				BlueprintComponent blueprintComponent = blueprintAbilityModifier.ComponentsArray[k];
				if (blueprintComponent is T)
				{
					return (T)(object)((blueprintComponent is T) ? blueprintComponent : null);
				}
			}
		}
		return default(T);
	}

	public bool SameAbility(BlueprintAbility blueprint)
	{
		if (blueprint != null)
		{
			if (OriginalBlueprint != blueprint)
			{
				return BaseAbility == blueprint;
			}
			return true;
		}
		return false;
	}

	public int GetVeilDamage()
	{
		return m_Blueprint.GetVeilDamage();
	}

	public string NameSafe()
	{
		return m_Blueprint.NameSafe();
	}

	public string GetTarget(int weaponAttackRange = -1, MechanicEntity caster = null)
	{
		return m_Blueprint.GetTarget(weaponAttackRange, caster);
	}

	public override string ToString()
	{
		return m_Blueprint?.ToString() ?? "null";
	}

	public bool IsRangeUnrestrictedForTarget(AbilityData ability, TargetWrapper target)
	{
		return GetComponents<AbilityUnrestrictedRangeForTarget>().Any((AbilityUnrestrictedRangeForTarget i) => i.IsRangeUnrestrictedForTarget(ability, target));
	}

	public bool IsIgnoredLoSForTarget(AbilityData ability, TargetWrapper target)
	{
		return GetComponents<AbilityIgnoreLosForTarget>().Any((AbilityIgnoreLosForTarget i) => i.IsIgnoredLoSForTarget(ability, target));
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
