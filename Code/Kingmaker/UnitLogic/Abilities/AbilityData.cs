using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.Visual.Animation;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities;

[OwlPackable(OwlPackableMode.Generate)]
public class AbilityData : IUIDataProvider, IAbilityDataProviderForPattern, IHashable, IOwlPackable, IOwlPackable<AbilityData>
{
	public enum UnavailabilityReasonType
	{
		None = -1,
		Unknown,
		AbilityDisabled,
		CasterRestrictionNotPassed,
		AbilityRestrictionNotPassed,
		IsOnCooldown,
		IsOnCooldownUntilEndOfCombat,
		CannotUseInThreatenedArea,
		CannotUseInConcussionArea,
		CannotUseInCantAttackArea,
		CannotUseInInertWarpArea,
		HasNoLosToTarget,
		TargetTooFar,
		TargetTooClose,
		AreaEffectsCannotOverlap,
		IsUltimateAbilityUsedThisRound,
		CannotTargetSelf,
		CannotTargetAlly,
		CannotTargetEnemy,
		CannotTargetDestructibleObject,
		CannotTargetDead,
		CannotTargetAlive,
		CannotTargetNotEmptyCell,
		AbilityForbidden,
		UntargetableForAbilityGroup,
		TargetRestrictionNotPassed,
		CannotMove,
		FriendlyFire,
		NullTarget,
		RestrictedByInterruption,
		TargetEntityDisposed,
		TargetCannotBeAttackedByPreciseAttack
	}

	public class IgnoreCooldown : ContextFlag<IgnoreCooldown>
	{
	}

	public class ForceFreeAction : ContextFlag<ForceFreeAction>
	{
	}

	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<MechanicEntity> m_CasterRef;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	private AbilityData m_ConvertedFrom;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private bool m_IsCharge;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private EntityFactRef<Ability> m_FactRef;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	private BlueprintAbilityModifier[] m_Modifiers;

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintAbility OriginalBlueprint;

	private BlueprintAbilityWrapper m_BlueprintWrapper;

	private List<BlueprintAbilityGroup> m_AbilityGroups;

	private string m_CachedName;

	[CanBeNull]
	private IAbilityVisibilityProvider[] m_CachedVisibilityProviders;

	private GridNodeBase m_TargetGridNodeCached;

	private GridNodeBase m_CasterGridNodeCached;

	private bool m_CanTargetResultCached;

	private UnavailabilityReasonType? m_UnavailabilityReasonTypeCached;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AbilityData",
		OldNames = null,
		Fields = new FieldInfo[15]
		{
			new FieldInfo("m_CasterRef", typeof(EntityRef<MechanicEntity>)),
			new FieldInfo("m_ConvertedFrom", typeof(AbilityData)),
			new FieldInfo("m_IsCharge", typeof(bool)),
			new FieldInfo("m_FactRef", typeof(EntityFactRef<Ability>)),
			new FieldInfo("m_Modifiers", typeof(BlueprintAbilityModifier[])),
			new FieldInfo("OriginalBlueprint", typeof(BlueprintAbility)),
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("OverrideWeapon", typeof(ItemEntityWeapon)),
			new FieldInfo("OverrideRateOfFire", typeof(int)),
			new FieldInfo("IndexInItemSettings", typeof(int)),
			new FieldInfo("FXSettingsOverride", typeof(BlueprintAbilityFXSettings)),
			new FieldInfo("IsRhymed", typeof(bool)),
			new FieldInfo("UnrestrictedRanged", typeof(bool)),
			new FieldInfo("IgnoreRestrictions", typeof(bool)),
			new FieldInfo("PreciseBodyPart", typeof(BlueprintBodyPart))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public string UniqueId { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public ItemEntityWeapon OverrideWeapon { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public int OverrideRateOfFire { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public int IndexInItemSettings { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	public BlueprintAbilityFXSettings FXSettingsOverride { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool IsRhymed { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool UnrestrictedRanged { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool IgnoreRestrictions { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	public BlueprintBodyPart PreciseBodyPart { get; set; }

	public IAbilityCasterRestriction AbilityCasterRestriction { get; private set; }

	public bool IsAttackOfOpportunity { get; set; }

	public bool IgnoreUsingInThreateningArea { get; set; }

	[JsonProperty]
	public BlueprintAbilityWrapper Blueprint => m_BlueprintWrapper ?? (m_BlueprintWrapper = new BlueprintAbilityWrapper(OriginalBlueprint, m_Modifiers));

	public IEnumerable<BlueprintAbilityModifier> Modifiers => m_Modifiers.EmptyIfNull();

	AbilityData IAbilityDataProviderForPattern.Data => this;

	public List<BlueprintAbilityGroup> AbilityGroups
	{
		get
		{
			if (m_AbilityGroups == null)
			{
				InitAbilityGroups();
			}
			return m_AbilityGroups;
		}
	}

	[CanBeNull]
	public Ability Fact => m_FactRef;

	[CanBeNull]
	public WeaponAbility SettingsFromItem => Weapon?.Blueprint?.WeaponAbilities[IndexInItemSettings];

	[CanBeNull]
	public ItemEntityWeapon SourceWeapon => SourceItem as ItemEntityWeapon;

	[CanBeNull]
	public ItemEntityWeapon Weapon => OverrideWeapon ?? SourceWeapon;

	public bool SourceItemIsWeapon => SourceItem is ItemEntityWeapon;

	public int BurstAttacksCount
	{
		get
		{
			if (!Blueprint.IsBurst)
			{
				return 1;
			}
			if (OverrideRateOfFire <= 0)
			{
				return Rulebook.Trigger(new RuleCalculateBurstCount(Caster, this)).Result;
			}
			return OverrideRateOfFire;
		}
	}

	public int ActionsCount => BurstAttacksCount;

	public int RangeCells => Rulebook.Trigger(new RuleCalculateAbilityRange(Caster, this)).Result;

	public int MinRangeCells
	{
		get
		{
			if (Blueprint.Range == AbilityRange.Personal)
			{
				return 0;
			}
			return Blueprint.MinRange;
		}
	}

	public int RateOfFire => this.GetWeaponStats().ResultRateOfFire;

	public bool ClearMPAfterUse => Blueprint.GetComponent<EndTurn>()?.clearMPInsteadOfEndingTurn ?? false;

	[CanBeNull]
	public AbilityData ConvertedFrom
	{
		get
		{
			return m_ConvertedFrom;
		}
		private set
		{
			m_ConvertedFrom = value;
			m_FactRef = ConvertedFrom?.Fact;
			if (ConvertedFrom != null && !ConvertedFrom.CanBeConvertedTo(Blueprint.OriginalBlueprint))
			{
				PFLog.Default.Error("Invalid ability conversion: {0} -> {1}", ConvertedFrom, this);
			}
		}
	}

	public bool CanTargetPoint
	{
		get
		{
			if (!Blueprint.CanTargetPoint)
			{
				if (IsBurst)
				{
					return Caster.Features.BurstFirePointTarget;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsAoe => Blueprint.IsAoE;

	public bool IsMoraleChange
	{
		get
		{
			AbilityEffectRunAction component = Blueprint.GetComponent<AbilityEffectRunAction>();
			if (component != null)
			{
				if (component.Actions.Actions.TryFind((GameAction x) => x is ContextActionMoraleChange, out var result))
				{
					return true;
				}
				if (component.ActionsOnAlly.Actions.TryFind((GameAction x) => x is ContextActionMoraleChange, out result))
				{
					return true;
				}
				if (component.ActionsOnEnemy.Actions.TryFind((GameAction x) => x is ContextActionMoraleChange, out result))
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsHeal
	{
		get
		{
			GameAction result;
			return Blueprint.GetComponent<AbilityEffectRunAction>()?.Actions.Actions.TryFind((GameAction x) => x is ContextActionHealTarget, out result) ?? false;
		}
	}

	public bool IsBurst => Blueprint.GetComponent<IAbilityAttackTypeProvider>()?.IsBurst ?? false;

	public bool IsSingleTarget => Blueprint.GetComponent<IAbilityAttackTypeProvider>()?.IsSingle ?? false;

	public bool IsMelee => Blueprint.GetComponent<IAbilityAttackTypeProvider>()?.IsMelee ?? false;

	public bool IsRanged => Blueprint.GetComponent<IAbilityAttackTypeProvider>()?.IsRanged ?? false;

	public bool IsThrow => Blueprint.GetComponent<IAbilityAttackTypeProvider>()?.IsThrow ?? false;

	public bool IsPrecise => Blueprint.GetComponent<IAbilityAttackTypeProvider>()?.IsPrecise ?? false;

	public bool IsCharge
	{
		get
		{
			if (!Blueprint.IsCharge)
			{
				return m_IsCharge;
			}
			return true;
		}
		set
		{
			m_IsCharge = value;
		}
	}

	public MechanicEntity Caster
	{
		get
		{
			object obj = ConvertedFrom?.Caster;
			if (obj == null)
			{
				obj = m_CasterRef.Entity;
				if (obj == null)
				{
					Ability fact = Fact;
					if (fact == null)
					{
						return null;
					}
					obj = fact.Owner;
				}
			}
			return (MechanicEntity)obj;
		}
	}

	[CanBeNull]
	public ItemEntity SourceItem => (ItemEntity)(Fact?.SourceItem);

	[CanBeNull]
	public BlueprintItemEquipmentUsable SourceItemUsableBlueprint => SourceItem?.Blueprint as BlueprintItemEquipmentUsable;

	public bool IsVariable => Blueprint.HasVariants;

	[CanBeNull]
	public IAbilityResourceLogic ResourceLogic => Blueprint.ResourceLogic;

	public bool NeedLoS => Blueprint.NeedLoS;

	public string Name
	{
		get
		{
			if (ConvertedFrom == null)
			{
				return Blueprint.Name;
			}
			m_CachedName = (string.IsNullOrEmpty(m_CachedName) ? (ConvertedFrom.Name + "-" + Blueprint.Name) : m_CachedName);
			return m_CachedName;
		}
	}

	public string Description
	{
		get
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionAbility = this;
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)Caster;
				return Blueprint.Description;
			}
		}
	}

	public string ShortenedDescription
	{
		get
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionAbility = this;
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)Caster;
				return Blueprint.GetShortenedDescription();
			}
		}
	}

	public Sprite Icon => Blueprint.Icon;

	public string NameForAcronym => Blueprint.NameForAcronym;

	public bool IsAvailable
	{
		get
		{
			if (GetAvailableForCastCount() != 0 && HasEnoughActionPoint && !IsRestricted)
			{
				if (IsOnCooldown)
				{
					return IsBonusUsage;
				}
				return true;
			}
			return false;
		}
	}

	public bool HasEnoughActionPoint
	{
		get
		{
			PartUnitCombatState combatStateOptional = Caster.GetCombatStateOptional();
			if (combatStateOptional != null && combatStateOptional.IsInCombat)
			{
				return combatStateOptional.ActionPoints >= CalculateActionPointCost();
			}
			return true;
		}
	}

	public bool IsOnCooldown
	{
		get
		{
			if (!ContextData<IgnoreCooldown>.Current)
			{
				return Caster.GetAbilityCooldownsOptional()?.IsOnCooldown(this) ?? false;
			}
			return false;
		}
	}

	public bool IsOnCooldownUntilEndOfCombat
	{
		get
		{
			if (!ContextData<IgnoreCooldown>.Current)
			{
				return Caster.GetAbilityCooldownsOptional()?.IsOnCooldownUntilEndOfCombat(this) ?? false;
			}
			return false;
		}
	}

	public int Cooldown => (Caster.GetAbilityCooldownsOptional()?.GetAutonomousCooldown(Blueprint.OriginalBlueprint)).GetValueOrDefault();

	public bool IsBonusUsage => Caster.GetBonusAbilityUseOptional()?.HasBonusAbilityUsage(this) ?? false;

	public bool IsRestricted
	{
		get
		{
			if ((bool)Game.Instance.LoadedAreaState?.Settings.Peaceful)
			{
				return true;
			}
			if ((Blueprint.CombatStateRestriction == BlueprintAbility.CombatStateRestrictionType.InCombatOnly && !Caster.IsInCombat) || (Blueprint.CombatStateRestriction == BlueprintAbility.CombatStateRestrictionType.NotInCombatOnly && Caster.IsInCombat))
			{
				return true;
			}
			AbilityCasterRestriction = null;
			foreach (IAbilityCasterRestriction casterRestriction in Blueprint.CasterRestrictions)
			{
				if (!casterRestriction.IsCasterRestrictionPassed(Caster))
				{
					AbilityCasterRestriction = casterRestriction;
					return true;
				}
			}
			PartUnitCombatState combatStateOptional = Caster.GetCombatStateOptional();
			if (combatStateOptional != null && combatStateOptional.IsEngagedInRealOrVirtualPosition && !IgnoreUsingInThreateningArea && !CanUseInThreateningArea && !Caster.GetMechanicFeature(MechanicsFeatureType.CanShootInMelee).Value)
			{
				EventBus.RaiseEvent(delegate(IAbilityCannotUseInThreateningArea h)
				{
					h.HandleCannotUseAbilityInThreateningArea(this);
				});
				return true;
			}
			GridNodeBase gridNodeBase = (GridNodeBase)(GraphNode)Caster.CurrentNode;
			if (!Blueprint.IsWeaponAbility && gridNodeBase != null && AreaEffectsController.CheckConcussionEffect(gridNodeBase))
			{
				return true;
			}
			if (Blueprint.IsWeaponAbility && gridNodeBase != null && AreaEffectsController.CheckCantAttackEffect(gridNodeBase))
			{
				return true;
			}
			if (Blueprint.IsPsykerAbility && gridNodeBase != null && AreaEffectsController.CheckInertWarpEffect(gridNodeBase))
			{
				return true;
			}
			IAbilityRestriction[] restrictions = Blueprint.Restrictions;
			for (int i = 0; i < restrictions.Length; i++)
			{
				if (!restrictions[i].IsAbilityRestrictionPassed(this))
				{
					return true;
				}
			}
			if (ShouldDelegateToMount && SameMountAbility == null)
			{
				return true;
			}
			PartAbilityRestrictions optional = Caster.GetOptional<PartAbilityRestrictions>();
			if (optional != null && !optional.IsRestrictionPassed(this))
			{
				return true;
			}
			if (Caster.Facts.GetComponents<AbilityRestriction>().Any((AbilityRestriction restriction) => restriction.AbilityIsRestricted(this)))
			{
				return true;
			}
			return false;
		}
	}

	public bool ShouldDelegateToMount => false;

	[CanBeNull]
	public AbilityData SameMountAbility => null;

	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettings => FXSettingsOverride ?? SettingsFromItem?.FXSettings ?? Blueprint.FXSettings;

	public AttackAnimationType AttackAnimationType => SettingsFromItem?.AttackAnimationType ?? Blueprint.AttackAnimationType;

	public int CustomRpm
	{
		get
		{
			WeaponAbility settingsFromItem = SettingsFromItem;
			if (settingsFromItem == null || !settingsFromItem.IsBurst)
			{
				return 0;
			}
			return (int)settingsFromItem.Rpm;
		}
	}

	public IEnumerable<BlueprintProjectile> ProjectileVariants
	{
		get
		{
			if (FXSettings == null || FXSettings.VisualFXSettings == null || FXSettings.VisualFXSettings.Projectiles.Empty())
			{
				yield break;
			}
			foreach (BlueprintProjectile projectile in FXSettings.VisualFXSettings.Projectiles)
			{
				yield return projectile;
			}
		}
	}

	public bool UseBestShootingPosition
	{
		get
		{
			if (Blueprint.UseBestShootingPosition)
			{
				return Caster.Size.Is1x1();
			}
			return false;
		}
	}

	public bool IsInstantDeliver => Blueprint.GetComponent<AbilityEffectOverwatch>() != null;

	public bool CanUseInThreateningArea => Blueprint.CanUseInThreateningArea;

	public bool ProvokesAttackOfOpportunity => Blueprint.ProvokesAttackOfOpportunity;

	public bool IsFreeAction => Blueprint.IsFreeAction;

	public bool IsBurstAttack => Blueprint.IsBurst;

	public AbilityTargetAnchor TargetAnchor
	{
		get
		{
			if (Blueprint.Range == AbilityRange.Personal)
			{
				return AbilityTargetAnchor.Owner;
			}
			if (!Blueprint.CanTargetFriends && !Blueprint.CanTargetEnemies && !CanTargetPoint)
			{
				return AbilityTargetAnchor.Owner;
			}
			if (!Blueprint.CanTargetPointAfterRestrictions(this))
			{
				return AbilityTargetAnchor.Unit;
			}
			return AbilityTargetAnchor.Point;
		}
	}

	private AbilityData([NotNull] BlueprintAbility blueprint, [NotNull] MechanicEntity caster, [CanBeNull] Ability fact, [CanBeNull] string guid, int indexInItemSettings = 0, IEnumerable<BlueprintAbilityModifier> modifiers = null)
	{
		m_Modifiers = modifiers?.ToArray();
		m_CasterRef = caster ?? throw new ArgumentNullException("caster");
		m_FactRef = fact;
		UniqueId = guid ?? Uuid.Instance.CreateString();
		IndexInItemSettings = indexInItemSettings;
		OriginalBlueprint = blueprint;
		m_BlueprintWrapper = new BlueprintAbilityWrapper(blueprint, m_Modifiers);
		BlueprintItemWeapon blueprintItemWeapon = Blueprint.GetComponent<OverrideAbilityWeapon>()?.Weapon;
		if (blueprintItemWeapon != null)
		{
			OverrideWeapon = (ItemEntityWeapon)blueprintItemWeapon.CreateEntity();
		}
		AbilityCustomBladeDance component = Blueprint.GetComponent<AbilityCustomBladeDance>();
		if (component != null)
		{
			OverrideRateOfFire = component.RateOfAttack.Calculate(ClaimExecutionContext(caster));
		}
		Blueprint.GetComponent<AbilityUseCurrentWeaponSetting>()?.Set(this);
		InitAbilityGroups();
	}

	public AbilityData([NotNull] BlueprintAbility blueprint, [NotNull] MechanicEntity caster, int indexInItemSettings = 0, IEnumerable<BlueprintAbilityModifier> modifiers = null)
		: this(blueprint, caster, null, null, indexInItemSettings, modifiers)
	{
	}

	public AbilityData([NotNull] Ability fact, MechanicEntity caster = null, int indexInItemSettings = 0, IEnumerable<BlueprintAbilityModifier> modifiers = null)
		: this(fact.Blueprint, caster ?? fact.Owner ?? throw new ArgumentException("Caster is missing!"), fact, fact.UniqueId, indexInItemSettings, modifiers)
	{
	}

	public AbilityData([NotNull] AbilityData other, [NotNull] BlueprintAbility replaceBlueprint)
		: this(replaceBlueprint, other.Caster, other.Fact, other.UniqueId + "_" + replaceBlueprint.name)
	{
		ConvertedFrom = other;
	}

	public AbilityData([NotNull] AbilityData other)
		: this(other.Blueprint.OriginalBlueprint, other.Caster, 0, other.m_Modifiers)
	{
		OverrideWeapon = other.Weapon;
		PreciseBodyPart = other.PreciseBodyPart;
	}

	[JsonConstructor]
	private AbilityData()
	{
	}

	public AbilityData Clone()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			return new AbilityData(this);
		}
	}

	private void InitAbilityGroups()
	{
		m_AbilityGroups = new List<BlueprintAbilityGroup>();
		foreach (BlueprintAbilityGroup abilityGroup in Blueprint.AbilityGroups)
		{
			if (abilityGroup != null)
			{
				m_AbilityGroups.AddRange(abilityGroup.GetAllAbilityGroups());
			}
		}
	}

	public int GetPredictedVeilDeltaBeforeCast()
	{
		return Rulebook.Trigger(new RuleCalculateVeilDamage(Caster, UpdateVeilEventType.BeforeAbilityCast, this)).ResultDamageDelta;
	}

	public int GetPredictedVeilDeltaAfterCast()
	{
		return Rulebook.Trigger(new RuleCalculateVeilDamage(Caster, UpdateVeilEventType.AfterAbilityCast, this)).ResultDamageDelta;
	}

	public int GetPredictedVeilDelta()
	{
		return GetPredictedVeilDeltaBeforeCast() + GetPredictedVeilDeltaAfterCast();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((AbilityData)obj);
	}

	public override int GetHashCode()
	{
		return (((((((Blueprint != null) ? Blueprint.GetHashCode() : 0) * 397) ^ m_CasterRef.GetHashCode()) * 397) ^ ((m_ConvertedFrom != null) ? m_ConvertedFrom.GetHashCode() : 0)) * 397) ^ ((Fact != null) ? Fact.GetHashCode() : 0);
	}

	protected bool Equals(AbilityData other)
	{
		if ((object)other != null && object.Equals(Blueprint, other.Blueprint) && m_CasterRef == other.m_CasterRef && object.Equals(m_ConvertedFrom, other.m_ConvertedFrom) && object.Equals(Fact, other.Fact))
		{
			return PreciseBodyPart == other.PreciseBodyPart;
		}
		return false;
	}

	public static bool operator ==(AbilityData a1, AbilityData a2)
	{
		return a1?.Equals(a2) ?? ((object)a2 == null);
	}

	public static bool operator !=(AbilityData a1, AbilityData a2)
	{
		return !(a1 == a2);
	}

	public AbilityExecutionContext ClaimExecutionContext([NotNull] TargetWrapper target, MechanicsContext parentContext = null)
	{
		return ClaimExecutionContext(target, Caster.Position, parentContext);
	}

	public AbilityExecutionContext ClaimExecutionContext([NotNull] TargetWrapper target, Vector3 casterPosition, MechanicsContext parentContext = null)
	{
		return AbilityExecutionContext.Claim(this, target ?? throw new ArgumentNullException("target"), casterPosition, parentContext);
	}

	public AbilityExecutionProcess Cast(AbilityExecutionContext context)
	{
		if (!context.IsForced && !IsAvailable)
		{
			PFLog.Default.ErrorWithReport("Can't cast spell: !context.IsForced && !IsAvailable");
			return null;
		}
		if (Blueprint.HasVariants)
		{
			PFLog.Default.ErrorWithReport("Can't cast spell with variants");
			return null;
		}
		return Game.Instance.Controllers.AbilityExecutor.Execute(context);
	}

	public void Spend()
	{
		Rulebook.Trigger(new RuleSpendAbilityCharge(Caster, this));
	}

	public bool IsValid(TargetWrapper target)
	{
		return IsValid(target, Caster.Position);
	}

	public bool IsValid(TargetWrapper target, out UnavailabilityReasonType unavailabilityReason)
	{
		return IsValid(target, Caster.Position, out unavailabilityReason);
	}

	public bool IsValid(TargetWrapper target, Vector3 casterPosition)
	{
		UnavailabilityReasonType unavailabilityReason;
		return IsValid(target, casterPosition, out unavailabilityReason);
	}

	public bool IsValid(TargetWrapper target, Vector3 casterPosition, out UnavailabilityReasonType unavailabilityReason)
	{
		AbilityTargetingCache instance = AbilityTargetingCache.Instance;
		if (!instance.IsActive)
		{
			return CalculateIsValid(target, casterPosition, out unavailabilityReason);
		}
		if (instance.TryGetReason(Blueprint.OriginalBlueprint, target, casterPosition, out unavailabilityReason))
		{
			return unavailabilityReason == UnavailabilityReasonType.None;
		}
		bool flag = CalculateIsValid(target, casterPosition, out unavailabilityReason);
		if (!flag && unavailabilityReason == UnavailabilityReasonType.None)
		{
			PFLog.AI.Error($"invalid ability targeting didn't get any reasoning\n ability {Blueprint} by {Caster} from {casterPosition} to {target}");
		}
		instance.AddEntry(Blueprint.OriginalBlueprint, target, casterPosition, unavailabilityReason);
		return flag;
	}

	public bool CalculateIsValid(TargetWrapper target, Vector3 casterPosition, out UnavailabilityReasonType unavailabilityReason)
	{
		IAbilityTargetRestriction[] targetRestrictions = Blueprint.TargetRestrictions;
		for (int i = 0; i < targetRestrictions.Length; i++)
		{
			if (!targetRestrictions[i].IsTargetRestrictionPassed(this, target, casterPosition))
			{
				unavailabilityReason = UnavailabilityReasonType.TargetRestrictionNotPassed;
				return false;
			}
		}
		foreach (BlueprintComponent item in Blueprint.ComponentsArray)
		{
			if (item is AbilityEffectRunAction abilityEffectRunAction && !abilityEffectRunAction.IsValidToCast(target, Caster, casterPosition))
			{
				unavailabilityReason = UnavailabilityReasonType.TargetRestrictionNotPassed;
				return false;
			}
		}
		if (!IsCasterValid(casterPosition, out var unavailabilityReason2))
		{
			unavailabilityReason = unavailabilityReason2;
			return false;
		}
		if (target.HasEntity && target.Entity == null)
		{
			unavailabilityReason = UnavailabilityReasonType.TargetEntityDisposed;
			return false;
		}
		if (target.Entity != null && TargetAnchor != AbilityTargetAnchor.Point)
		{
			if (!Blueprint.CanTargetSelf && target.Entity == Caster)
			{
				unavailabilityReason = UnavailabilityReasonType.CannotTargetSelf;
				return false;
			}
			if (!Blueprint.CanTargetFriends && target.Entity != Caster && Caster.IsAlly(target.Entity))
			{
				unavailabilityReason = UnavailabilityReasonType.CannotTargetAlly;
				return false;
			}
			if (!Blueprint.CanTargetEnemies && Caster.IsEnemy(target.Entity))
			{
				unavailabilityReason = UnavailabilityReasonType.CannotTargetEnemy;
				return false;
			}
			bool flag = !Blueprint.CanTargetDestructibleObjects;
			if (flag)
			{
				MechanicEntity entity = target.Entity;
				bool flag2 = ((entity is DestructibleEntity || (entity != null && entity.IsMechanism)) ? true : false);
				flag = flag2;
			}
			if (flag)
			{
				unavailabilityReason = UnavailabilityReasonType.CannotTargetDestructibleObject;
				return false;
			}
			if (target.Entity.IsDeadOrUnconscious && !Blueprint.CanCastToDeadTarget)
			{
				unavailabilityReason = UnavailabilityReasonType.CannotTargetDead;
				return false;
			}
		}
		if (!Blueprint.CanTargetSelf && TargetAnchor == AbilityTargetAnchor.Point && Caster.GetOccupiedNodes(casterPosition).Contains(target.NearestNode))
		{
			unavailabilityReason = UnavailabilityReasonType.CannotTargetSelf;
			return false;
		}
		PartAbilityRestrictions optional = Caster.GetOptional<PartAbilityRestrictions>();
		if (optional != null && !optional.IsRestrictionPassed(this, target))
		{
			unavailabilityReason = UnavailabilityReasonType.AbilityForbidden;
			return false;
		}
		if (Blueprint.IsSummoningUnit && WarhammerBlockManager.Instance.NodeContainsAny(target.NearestNode))
		{
			unavailabilityReason = UnavailabilityReasonType.CannotTargetNotEmptyCell;
			return false;
		}
		unavailabilityReason = UnavailabilityReasonType.None;
		switch (TargetAnchor)
		{
		case AbilityTargetAnchor.Owner:
			if (target.HasEntity && target.Entity == Caster)
			{
				return true;
			}
			unavailabilityReason = UnavailabilityReasonType.NullTarget;
			return false;
		case AbilityTargetAnchor.Unit:
			if (target.Entity == null)
			{
				unavailabilityReason = UnavailabilityReasonType.NullTarget;
				return false;
			}
			if (Blueprint.CanTargetFriends || target.Entity.IsPlayerFaction || Caster.CanAttack(target.Entity))
			{
				return true;
			}
			unavailabilityReason = UnavailabilityReasonType.CannotTargetAlly;
			return false;
		case AbilityTargetAnchor.Point:
			return true;
		default:
			unavailabilityReason = UnavailabilityReasonType.Unknown;
			throw new ArgumentOutOfRangeException();
		}
	}

	public bool IsCasterValid(Vector3 casterPosition, out UnavailabilityReasonType unavailabilityReason)
	{
		if (!Caster.CanMove)
		{
			foreach (BlueprintComponent item in Blueprint.ComponentsArray)
			{
				if (item is AbilityCustomLogic { IsMoveUnit: not false })
				{
					unavailabilityReason = UnavailabilityReasonType.CannotMove;
					return false;
				}
			}
		}
		GridNodeBase nearestNodeXZUnwalkable = casterPosition.GetNearestNodeXZUnwalkable();
		if (!IgnoreUsingInThreateningArea && !CanUseInThreateningArea && !Caster.GetMechanicFeature(MechanicsFeatureType.CanShootInMelee).Value)
		{
			PartCombatGroup combatGroupOptional = Caster.GetCombatGroupOptional();
			if (combatGroupOptional != null)
			{
				foreach (UnitGroupMemory.UnitInfo enemy in combatGroupOptional.Memory.Enemies)
				{
					if (enemy.Unit.IsThreat(casterPosition.GetNearestNodeXZUnwalkable(), enemy.Unit.Position, Caster.SizeRect))
					{
						unavailabilityReason = UnavailabilityReasonType.CannotUseInThreatenedArea;
						return false;
					}
				}
			}
		}
		if (!Blueprint.IsWeaponAbility && AreaEffectsController.CheckConcussionEffect(nearestNodeXZUnwalkable))
		{
			unavailabilityReason = UnavailabilityReasonType.CannotUseInConcussionArea;
			return false;
		}
		if (Blueprint.IsWeaponAbility && AreaEffectsController.CheckCantAttackEffect(nearestNodeXZUnwalkable))
		{
			unavailabilityReason = UnavailabilityReasonType.CannotUseInCantAttackArea;
			return false;
		}
		if (Blueprint.IsPsykerAbility && AreaEffectsController.CheckInertWarpEffect(nearestNodeXZUnwalkable))
		{
			unavailabilityReason = UnavailabilityReasonType.CannotUseInInertWarpArea;
			return false;
		}
		unavailabilityReason = UnavailabilityReasonType.None;
		return true;
	}

	public bool CanTarget(TargetWrapper target)
	{
		UnavailabilityReasonType? unavailableReason;
		return CanTarget(target, out unavailableReason);
	}

	public bool CanTarget(TargetWrapper target, out UnavailabilityReasonType? unavailableReason)
	{
		return CanTarget(target, Caster.Position, out unavailableReason, null);
	}

	public bool CanTargetFromDesiredPosition(TargetWrapper target)
	{
		UnavailabilityReasonType? unavailabilityReason;
		return CanTargetFromDesiredPosition(target, out unavailabilityReason);
	}

	public bool CanTargetFromDesiredPosition(TargetWrapper target, out UnavailabilityReasonType? unavailabilityReason)
	{
		return CanTarget(target, Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(Caster), out unavailabilityReason, Game.Instance.Controllers.VirtualPositionController.GetDesiredRotation(Caster));
	}

	private bool CanTarget(TargetWrapper target, Vector3 casterPosition, out UnavailabilityReasonType? unavailabilityReason, Vector3? casterDirection)
	{
		GridNodeBase nearestNodeXZUnwalkable = casterPosition.GetNearestNodeXZUnwalkable();
		if (target != null && target.NearestNode == m_TargetGridNodeCached && nearestNodeXZUnwalkable == m_CasterGridNodeCached)
		{
			unavailabilityReason = m_UnavailabilityReasonTypeCached;
			return m_CanTargetResultCached;
		}
		m_CasterGridNodeCached = nearestNodeXZUnwalkable;
		m_TargetGridNodeCached = target?.NearestNode;
		int? casterDirection2 = ((casterDirection.HasValue && casterDirection.GetValueOrDefault().sqrMagnitude > 1E-06f) ? new int?(GraphHelper.GuessDirection(casterDirection.Value)) : null);
		m_CanTargetResultCached = CanTargetFromNode(nearestNodeXZUnwalkable, null, target, out var _, out var _, out unavailabilityReason, casterDirection2);
		m_UnavailabilityReasonTypeCached = unavailabilityReason;
		return m_CanTargetResultCached;
	}

	public bool CanTargetFromNode(GridNodeBase casterNode, GridNodeBase targetNodeHint, TargetWrapper target, out int distance, out LosCalculations.CoverType los, int? casterDirection = null)
	{
		UnavailabilityReasonType? unavailabilityReason;
		return CanTargetFromNode(casterNode, targetNodeHint, target, out distance, out los, out unavailabilityReason, casterDirection);
	}

	public bool CanTargetFromNode(GridNodeBase casterNode, GridNodeBase targetNodeHint, TargetWrapper target, out int distance, out LosCalculations.CoverType los, out UnavailabilityReasonType? unavailabilityReason, int? casterDirection = null)
	{
		distance = WarhammerGeometryUtils.DistanceToInCells(casterNode.Vector3Position(), Caster.SizeRect, target.Point, target.SizeRect);
		los = LosCalculations.CoverType.Obstacle;
		GridNodeBase bestShootingPosition = GetBestShootingPosition(casterNode, target);
		if (!IsValid(target, casterNode.Vector3Position(), out var unavailabilityReason2))
		{
			unavailabilityReason = unavailabilityReason2;
			return false;
		}
		if (!this.IsPatternRestrictionPassed(target))
		{
			unavailabilityReason = UnavailabilityReasonType.AreaEffectsCannotOverlap;
			return false;
		}
		GridNodeBase end = targetNodeHint ?? target.NearestNode;
		if (IsMelee && !LosCalculations.HasMeleeLos(bestShootingPosition, Caster.SizeRect, end, target.SizeRect))
		{
			unavailabilityReason = UnavailabilityReasonType.HasNoLosToTarget;
			return false;
		}
		if (target.HasEntity && target.Entity.Buffs.SelectComponents<UnitBuffUntargetableByAbilityGroups>().Any((UnitBuffUntargetableByAbilityGroups buff) => buff.BlockedGroups.Any((BlueprintAbilityGroupReference group) => AbilityGroups.Contains(group))))
		{
			unavailabilityReason = UnavailabilityReasonType.UntargetableForAbilityGroup;
			return false;
		}
		if (IsRangeUnrestrictedForTarget(target))
		{
			unavailabilityReason = UnavailabilityReasonType.None;
			return true;
		}
		if (NeedLoS && !IsIgnoredLoSForTarget(target) && !LosCalculations.HasLos(UseBestShootingPosition ? bestShootingPosition : casterNode, Caster.SizeRect, end, target.SizeRect, out var obstacle) && (obstacle.Entity == null || obstacle.Entity != target.Entity))
		{
			unavailabilityReason = UnavailabilityReasonType.HasNoLosToTarget;
			return false;
		}
		if (distance < MinRangeCells)
		{
			unavailabilityReason = UnavailabilityReasonType.TargetTooClose;
			return false;
		}
		if (distance > RangeCells)
		{
			unavailabilityReason = UnavailabilityReasonType.TargetTooFar;
			return false;
		}
		unavailabilityReason = UnavailabilityReasonType.None;
		return true;
	}

	public int GetAvailableForCastCount()
	{
		int num = -1;
		if (m_ConvertedFrom != null)
		{
			num = m_ConvertedFrom.GetAvailableForCastCount();
		}
		else
		{
			ItemEntity sourceItem = SourceItem;
			if (sourceItem != null && sourceItem.IsSpendCharges)
			{
				num = SourceItem.Charges;
			}
		}
		int num2 = 0;
		int num3 = 0;
		if (ResourceLogic != null)
		{
			num3 = GetResourceAmount();
			num2 = ResourceLogic?.CalculateCost(this) ?? 1;
		}
		if (num2 > 0)
		{
			int num4 = num3 / num2;
			int num5 = ((num > -1) ? Math.Min(num4, num) : num4);
			num = ((num > -1) ? Math.Min(num, num5) : num5);
		}
		return num;
	}

	public int GetResourceCost()
	{
		int result = -1;
		IAbilityResourceLogic resourceLogic = ResourceLogic;
		if (resourceLogic != null && resourceLogic.IsSpendResource())
		{
			result = ResourceLogic?.CalculateCost(this) ?? 1;
		}
		return result;
	}

	public int GetResourceAmount()
	{
		return ResourceLogic?.CalculateResourceAmount(this) ?? (-1);
	}

	public List<UnavailabilityReasonType> GetUnavailabilityReasons()
	{
		return GetUnavailabilityReasons(Caster.Position);
	}

	private List<UnavailabilityReasonType> GetUnavailabilityReasons(Vector3 castPosition)
	{
		if (m_ConvertedFrom != null)
		{
			return m_ConvertedFrom.GetUnavailabilityReasons(castPosition);
		}
		List<UnavailabilityReasonType> list = new List<UnavailabilityReasonType>();
		foreach (IAbilityCasterRestriction casterRestriction in Blueprint.CasterRestrictions)
		{
			if (!casterRestriction.IsCasterRestrictionPassed(Caster))
			{
				list.Add(UnavailabilityReasonType.CasterRestrictionNotPassed);
			}
		}
		if (Caster.IsEngagedInMelee() && !CanUseInThreateningArea && !Caster.GetMechanicFeature(MechanicsFeatureType.CanShootInMelee).Value)
		{
			list.Add(UnavailabilityReasonType.CannotUseInThreatenedArea);
		}
		GridNodeBase node = (GridNodeBase)ObstacleAnalyzer.GetNearestNode(castPosition).node;
		if (!Blueprint.IsWeaponAbility && AreaEffectsController.CheckConcussionEffect(node))
		{
			list.Add(UnavailabilityReasonType.CannotUseInConcussionArea);
		}
		if (Blueprint.IsWeaponAbility && AreaEffectsController.CheckCantAttackEffect(node))
		{
			list.Add(UnavailabilityReasonType.CannotUseInCantAttackArea);
		}
		if (Blueprint.IsPsykerAbility && AreaEffectsController.CheckInertWarpEffect(node))
		{
			list.Add(UnavailabilityReasonType.CannotUseInInertWarpArea);
		}
		if (IsOnCooldownUntilEndOfCombat)
		{
			list.Add(UnavailabilityReasonType.IsOnCooldownUntilEndOfCombat);
		}
		if (IsOnCooldown)
		{
			list.Add(UnavailabilityReasonType.IsOnCooldown);
		}
		if (Fact != null && !Fact.Active)
		{
			list.Add(UnavailabilityReasonType.AbilityDisabled);
		}
		IAbilityRestriction[] restrictions = Blueprint.Restrictions;
		for (int i = 0; i < restrictions.Length; i++)
		{
			if (!restrictions[i].IsAbilityRestrictionPassed(this))
			{
				list.Add(UnavailabilityReasonType.AbilityRestrictionNotPassed);
			}
		}
		if (Game.Instance.Controllers.TurnController.IsUltimateAbilityUsedThisRound)
		{
			list.Add(UnavailabilityReasonType.IsUltimateAbilityUsedThisRound);
		}
		return list;
	}

	public string GetUnavailableReason(Vector3 casterPosition)
	{
		List<UnavailabilityReasonType> unavailabilityReasons = GetUnavailabilityReasons(casterPosition);
		if (unavailabilityReasons.Count != 0)
		{
			return GetUnavailabilityReasonString(unavailabilityReasons[0]);
		}
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}

	public string GetUnavailabilityReasonString(UnavailabilityReasonType type)
	{
		return GetUnavailabilityReasonString(type, null, null);
	}

	public string GetUnavailabilityReasonString(UnavailabilityReasonType type, Vector3 casterPosition, [NotNull] TargetWrapper target)
	{
		return GetUnavailabilityReasonString(type, (Vector3?)casterPosition, target);
	}

	private string GetUnavailabilityReasonString(UnavailabilityReasonType type, Vector3? casterPosition, [CanBeNull] TargetWrapper target)
	{
		string unavailabilityReasonStringInternal = GetUnavailabilityReasonStringInternal(type, casterPosition, target);
		if (!unavailabilityReasonStringInternal.IsNullOrEmpty())
		{
			return unavailabilityReasonStringInternal;
		}
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}

	private string GetUnavailabilityReasonStringInternal(UnavailabilityReasonType type, Vector3? casterPosition, [CanBeNull] TargetWrapper target)
	{
		switch (type)
		{
		case UnavailabilityReasonType.AbilityDisabled:
			return LocalizedTexts.Instance.Reasons.AbilityDisabled;
		case UnavailabilityReasonType.CasterRestrictionNotPassed:
			foreach (IAbilityCasterRestriction casterRestriction in Blueprint.CasterRestrictions)
			{
				if (!casterRestriction.IsCasterRestrictionPassed(Caster))
				{
					return casterRestriction.GetAbilityCasterRestrictionUIText(Caster);
				}
			}
			break;
		case UnavailabilityReasonType.AbilityRestrictionNotPassed:
		{
			IAbilityRestriction[] restrictions = Blueprint.Restrictions;
			foreach (IAbilityRestriction abilityRestriction in restrictions)
			{
				if (!abilityRestriction.IsAbilityRestrictionPassed(this))
				{
					return abilityRestriction.GetAbilityRestrictionUIText();
				}
			}
			break;
		}
		case UnavailabilityReasonType.TargetRestrictionNotPassed:
		{
			if (!casterPosition.HasValue || !(target != null))
			{
				break;
			}
			IAbilityTargetRestriction[] targetRestrictions = Blueprint.TargetRestrictions;
			foreach (IAbilityTargetRestriction abilityTargetRestriction in targetRestrictions)
			{
				if (!abilityTargetRestriction.IsTargetRestrictionPassed(this, target, casterPosition.Value))
				{
					return abilityTargetRestriction.GetAbilityTargetRestrictionUIText(this, target, casterPosition.Value);
				}
			}
			break;
		}
		case UnavailabilityReasonType.IsOnCooldown:
			return LocalizedTexts.Instance.Reasons.IsOnCooldown;
		case UnavailabilityReasonType.IsOnCooldownUntilEndOfCombat:
			return LocalizedTexts.Instance.Reasons.IsOnCooldownUntilEndOfCombat;
		case UnavailabilityReasonType.CannotUseInThreatenedArea:
			return LocalizedTexts.Instance.Reasons.CannotUseInThreatenedArea;
		case UnavailabilityReasonType.TargetTooFar:
			return LocalizedTexts.Instance.Reasons.TargetTooFar;
		case UnavailabilityReasonType.TargetTooClose:
			return LocalizedTexts.Instance.Reasons.TargetTooClose;
		case UnavailabilityReasonType.HasNoLosToTarget:
			return LocalizedTexts.Instance.Reasons.HasNoLosToTarget;
		case UnavailabilityReasonType.AreaEffectsCannotOverlap:
			return LocalizedTexts.Instance.Reasons.AreaEffectsCannotOverlap;
		case UnavailabilityReasonType.IsUltimateAbilityUsedThisRound:
			return LocalizedTexts.Instance.Reasons.AlreadyDesperateMeasuredThisTurn;
		case UnavailabilityReasonType.TargetCannotBeAttackedByPreciseAttack:
			return LocalizedTexts.Instance.Reasons.TargetCannotBeAttackedByPreciseAttack;
		}
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}

	public bool IsVisible()
	{
		if (m_CachedVisibilityProviders == null)
		{
			m_CachedVisibilityProviders = Blueprint.GetComponents<IAbilityVisibilityProvider>().ToArray();
		}
		IAbilityVisibilityProvider[] cachedVisibilityProviders = m_CachedVisibilityProviders;
		for (int i = 0; i < cachedVisibilityProviders.Length; i++)
		{
			if (!cachedVisibilityProviders[i].IsAbilityVisible(this))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanBeConvertedTo(BlueprintAbility otherSpell)
	{
		if (Blueprint.OriginalBlueprint == otherSpell)
		{
			return true;
		}
		if (Blueprint.OriginalBlueprint.HasVariant(otherSpell))
		{
			return true;
		}
		return false;
	}

	public IEnumerable<AbilityData> GetConversions()
	{
		List<AbilityData> result = null;
		ReferenceArrayProxy<BlueprintAbility>? referenceArrayProxy = Blueprint.GetComponent<AbilityVariants>()?.Variants;
		if (referenceArrayProxy.HasValue)
		{
			foreach (BlueprintAbility item in referenceArrayProxy.Value)
			{
				AddAbilityUnique(ref result, new AbilityData(this, item));
			}
		}
		IEnumerable<AbilityData> enumerable = result;
		return enumerable ?? Enumerable.Empty<AbilityData>();
	}

	private static void AddAbilityUnique([CanBeNull] ref List<AbilityData> result, AbilityData ability)
	{
		result = result ?? TempList.Get<AbilityData>();
		foreach (AbilityData item in result)
		{
			if (ability.Equals(item))
			{
				return;
			}
		}
		result.Add(ability);
	}

	public GridNodeBase GetBestShootingPosition(TargetWrapper target)
	{
		return GetBestShootingPosition(Caster.CurrentUnwalkableNode, target);
	}

	public GridNodeBase GetBestShootingPositionForDesiredPosition(TargetWrapper target)
	{
		return GetBestShootingPosition(Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(Caster).GetNearestNodeXZUnwalkable(), target);
	}

	public GridNodeBase GetBestShootingPosition(GridNodeBase castNode, TargetWrapper target)
	{
		if (!UseBestShootingPosition)
		{
			return castNode;
		}
		return LosCalculations.GetBestShootingNode(castNode, Caster.SizeRect, target.NearestNode, target.SizeRect, Caster);
	}

	public OrientedPatternData GetPattern(TargetWrapper target, Vector3 casterPosition)
	{
		IAbilityAoEPatternProvider patternSettings = GetPatternSettings();
		if (patternSettings?.Pattern == null)
		{
			return OrientedPatternData.Empty;
		}
		GridNodeBase nearestNodeXZUnwalkable = target.Point.GetNearestNodeXZUnwalkable();
		GridNodeBase nearestNodeXZUnwalkable2 = casterPosition.GetNearestNodeXZUnwalkable();
		GridNodeBase bestShootingPosition = GetBestShootingPosition(nearestNodeXZUnwalkable2, target);
		Size targetSizeForPattern = GetTargetSizeForPattern(target);
		return patternSettings.GetOrientedPattern(this, bestShootingPosition, nearestNodeXZUnwalkable, targetSizeForPattern);
	}

	public OrientedPatternData GetHaloPattern(TargetWrapper target, Vector3 casterPosition, int haloSize)
	{
		IAbilityAoEPatternProvider patternSettings = GetPatternSettings();
		if (patternSettings == null)
		{
			return OrientedPatternData.Empty;
		}
		GridNodeBase nearestNodeXZUnwalkable = target.Point.GetNearestNodeXZUnwalkable();
		GridNodeBase nearestNodeXZUnwalkable2 = casterPosition.GetNearestNodeXZUnwalkable();
		GridNodeBase bestShootingPosition = GetBestShootingPosition(nearestNodeXZUnwalkable2, target);
		Size targetSizeForPattern = GetTargetSizeForPattern(target);
		return patternSettings.GetOrientedHaloPattern(this, haloSize, bestShootingPosition, nearestNodeXZUnwalkable, targetSizeForPattern);
	}

	public bool GetHaloSize(out int haloSize)
	{
		IEnumerable<AbilityHaloEffect> components = Blueprint.GetComponents<AbilityHaloEffect>();
		if (components == null || !components.Any())
		{
			haloSize = 0;
			return false;
		}
		haloSize = components.Max((AbilityHaloEffect c) => c.HaloSize);
		return haloSize > 0;
	}

	public IAbilityAoEPatternProvider GetPatternSettings()
	{
		return PartAbilityPatternSettings.GetAbilityPatternSettings(this);
	}

	public int CalculateActionPointCost()
	{
		if (!Blueprint.IsFreeAction)
		{
			return Rulebook.Trigger(new RuleCalculateAbilityActionPointCost(Caster, this)).Result;
		}
		return 0;
	}

	public bool IsRangeUnrestrictedForTarget(TargetWrapper target)
	{
		if (!UnrestrictedRanged)
		{
			return Blueprint.IsRangeUnrestrictedForTarget(this, target);
		}
		return true;
	}

	public bool IsIgnoredLoSForTarget(TargetWrapper target)
	{
		return Blueprint.IsIgnoredLoSForTarget(this, target);
	}

	public float CalculateDefenceChanceCached(UnitEntity unit, LosCalculations.CoverType coverType)
	{
		return (float)Rulebook.Trigger(new RuleCalculateDefence(Caster, unit)).ResultDefence / 100f;
	}

	public bool HasLosCached(GridNodeBase fromNode, GridNodeBase toNode)
	{
		return LosCalculations.HasLos(fromNode, default(IntRect), toNode, default(IntRect));
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(128);
		stringBuilder.Append(Blueprint.ToString());
		stringBuilder.Append("[caster=");
		string value = Caster.GetDescriptionOptional()?.Name ?? Caster.Blueprint.name;
		stringBuilder.Append(value);
		if (SourceItem != null)
		{
			stringBuilder.Append(", item=");
			stringBuilder.Append(SourceItem.Blueprint.ToString());
			if (SourceItem.IsSpendCharges)
			{
				stringBuilder.Append("(charges=");
				stringBuilder.Append($"{((SourceItem.Count > 1) ? SourceItem.Count : SourceItem.Charges)}");
				stringBuilder.Append(")");
			}
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public bool CanTargetPointAfterRestrictions()
	{
		return Blueprint.CanTargetPointAfterRestrictions(this);
	}

	public Size GetTargetSizeForPattern(TargetWrapper target)
	{
		if (Blueprint.Range != 0)
		{
			if (!(target?.Entity is AbstractUnitEntity abstractUnitEntity))
			{
				return Size.Medium;
			}
			return abstractUnitEntity.Size;
		}
		return Caster?.Size ?? Size.Medium;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<MechanicEntity> obj = m_CasterRef;
		Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<AbilityData>.GetHash128(m_ConvertedFrom);
		result.Append(ref val2);
		result.Append(ref m_IsCharge);
		EntityFactRef<Ability> obj2 = m_FactRef;
		Hash128 val3 = StructHasher<EntityFactRef<Ability>>.GetHash128(ref obj2);
		result.Append(ref val3);
		BlueprintAbilityModifier[] modifiers = m_Modifiers;
		if (modifiers != null)
		{
			for (int i = 0; i < modifiers.Length; i++)
			{
				Hash128 val4 = SimpleBlueprintHasher.GetHash128(modifiers[i]);
				result.Append(ref val4);
			}
		}
		Hash128 val5 = SimpleBlueprintHasher.GetHash128(OriginalBlueprint);
		result.Append(ref val5);
		result.Append(UniqueId);
		Hash128 val6 = ClassHasher<ItemEntityWeapon>.GetHash128(OverrideWeapon);
		result.Append(ref val6);
		int val7 = OverrideRateOfFire;
		result.Append(ref val7);
		int val8 = IndexInItemSettings;
		result.Append(ref val8);
		Hash128 val9 = SimpleBlueprintHasher.GetHash128(FXSettingsOverride);
		result.Append(ref val9);
		bool val10 = IsRhymed;
		result.Append(ref val10);
		bool val11 = UnrestrictedRanged;
		result.Append(ref val11);
		bool val12 = IgnoreRestrictions;
		result.Append(ref val12);
		Hash128 val13 = SimpleBlueprintHasher.GetHash128(PreciseBodyPart);
		result.Append(ref val13);
		Hash128 val14 = ClassHasher<BlueprintAbilityWrapper>.GetHash128(Blueprint);
		result.Append(ref val14);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AbilityData source = new AbilityData();
		result = Unsafe.As<AbilityData, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<AbilityData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_CasterRef", ref m_CasterRef, state);
		formatter.Field(1, "m_ConvertedFrom", ref m_ConvertedFrom, state);
		formatter.UnmanagedField(2, "m_IsCharge", ref m_IsCharge, state);
		formatter.Field(3, "m_FactRef", ref m_FactRef, state);
		formatter.Field(4, "m_Modifiers", ref m_Modifiers, state);
		formatter.Field(5, "OriginalBlueprint", ref OriginalBlueprint, state);
		string value = UniqueId;
		formatter.StringField(6, "UniqueId", ref value, state);
		ItemEntityWeapon value2 = OverrideWeapon;
		formatter.Field(7, "OverrideWeapon", ref value2, state);
		int value3 = OverrideRateOfFire;
		formatter.UnmanagedField(8, "OverrideRateOfFire", ref value3, state);
		int value4 = IndexInItemSettings;
		formatter.UnmanagedField(9, "IndexInItemSettings", ref value4, state);
		BlueprintAbilityFXSettings value5 = FXSettingsOverride;
		formatter.Field(10, "FXSettingsOverride", ref value5, state);
		bool value6 = IsRhymed;
		formatter.UnmanagedField(11, "IsRhymed", ref value6, state);
		bool value7 = UnrestrictedRanged;
		formatter.UnmanagedField(12, "UnrestrictedRanged", ref value7, state);
		bool value8 = IgnoreRestrictions;
		formatter.UnmanagedField(13, "IgnoreRestrictions", ref value8, state);
		BlueprintBodyPart value9 = PreciseBodyPart;
		formatter.Field(14, "PreciseBodyPart", ref value9, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AbilityData>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_CasterRef = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 1:
				m_ConvertedFrom = formatter.ReadPackable<AbilityData>(state);
				break;
			case 2:
				m_IsCharge = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_FactRef = formatter.ReadPackable<EntityFactRef<Ability>>(state);
				break;
			case 4:
				m_Modifiers = formatter.ReadPackable<BlueprintAbilityModifier[]>(state);
				break;
			case 5:
				OriginalBlueprint = formatter.ReadPackable<BlueprintAbility>(state);
				break;
			case 6:
				UniqueId = formatter.ReadString(state);
				break;
			case 7:
				OverrideWeapon = formatter.ReadPackable<ItemEntityWeapon>(state);
				break;
			case 8:
				OverrideRateOfFire = formatter.ReadUnmanaged<int>(state);
				break;
			case 9:
				IndexInItemSettings = formatter.ReadUnmanaged<int>(state);
				break;
			case 10:
				FXSettingsOverride = formatter.ReadPackable<BlueprintAbilityFXSettings>(state);
				break;
			case 11:
				IsRhymed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 12:
				UnrestrictedRanged = formatter.ReadUnmanaged<bool>(state);
				break;
			case 13:
				IgnoreRestrictions = formatter.ReadUnmanaged<bool>(state);
				break;
			case 14:
				PreciseBodyPart = formatter.ReadPackable<BlueprintBodyPart>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
