using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Gameplay.Features.Cohesion.Ability;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CutsceneAttack;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Blueprints;

[Serializable]
[TypeId("da11db195c86e0d4dae17a2c03a4ba9a")]
[OwlPackable(OwlPackableMode.NoGenerate)]
[ComponentName("Ability/BlueprintAbility")]
public class BlueprintAbility : BlueprintUnitFact, IBlueprintScanner, IResourceIdsHolder
{
	public enum UsingInThreateningAreaType
	{
		CanUse,
		CannotUse
	}

	public enum CombatStateRestrictionType
	{
		NoRestriction,
		InCombatOnly,
		NotInCombatOnly
	}

	public AbilityType Type;

	public AbilityRange Range;

	public BpRef<BlueprintAbilityTag>[] Tags = new BpRef<BlueprintAbilityTag>[0];

	[ShowIf("IsRangeCustom")]
	public int CustomRange;

	[HideIf("IsRangePersonal")]
	public int MinRange;

	public int ActionPointCost = 1;

	public WarhammerAbilityParamsSource AbilityParamsSource = WarhammerAbilityParamsSource.None;

	[ShowIf("IsPsykerAbility")]
	[Tooltip("Используется для оверрайда значения выдаваемого от Psychic Power")]
	public int VeilDamage = 1;

	public int CooldownRounds;

	public bool CanTargetPoint;

	public bool CanTargetEnemies;

	[InfoBox("Allows to cast on allies. But does not prevent from casting on enemies if only selected")]
	public bool CanTargetFriends;

	public bool CanTargetSelf = true;

	public bool CanTargetDestructibleObjects = true;

	public bool Hidden;

	public bool DisableBestShootingPosition;

	public bool NeedEquipWeapons;

	public bool NotOffensive;

	public bool ShowInDialogue;

	[SerializeField]
	private BlueprintAbilityReference m_Parent;

	public AbilityAnimationStyle Animation;

	public bool CastInOffHand;

	public bool UseOnMechadendrite;

	[ShowIf("UseOnMechadendrite")]
	public MechadendritesType UsedMechadendrite = MechadendritesType.Utility;

	public bool IsFreeAction;

	public bool ShouldTurnToTarget = true;

	[SerializeField]
	private bool m_IsStratagem;

	public UsingInThreateningAreaType UsingInThreateningArea;

	public CombatStateRestrictionType CombatStateRestriction = CombatStateRestrictionType.InCombatOnly;

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintAbilityGroupReference[] m_AbilityGroups;

	public bool DisableLog;

	public string[] ResourceAssetIds;

	[SerializeField]
	private BlueprintAbilityFXSettings.Reference m_FXSettings;

	[SerializeField]
	private AttackAnimationType m_AttackAnimationType;

	[SerializeField]
	private AbilityTag m_AbilityTag;

	[SerializeField]
	private CombatHudCommandSetAsset m_CombatHudCommandsOverride;

	[ValidateNoNullEntries]
	[InspectorReadOnly]
	public BpRef<BlueprintAbility>[] Upgrades = new BpRef<BlueprintAbility>[0];

	[ValidateNoNullEntries]
	public BpRef<BlueprintAbilityModifier>[] Modifiers = new BpRef<BlueprintAbilityModifier>[0];

	[CanBeNull]
	private IAbilityRestriction[] m_CachedRestrictions;

	[CanBeNull]
	private IAbilityTargetRestriction[] m_CachedTargetRestrictions;

	[CanBeNull]
	private IAbilityCasterRestriction[] m_CachedCasterRestrictions;

	[CanBeNull]
	private IAbilityCanTargetPointRestriction[] m_CachedCanTargetPointRestrictions;

	public override string Description => UtilityAbilities.GetLongOrShortText(base.Description, state: true);

	public AbilityTag AbilityTag => m_AbilityTag;

	public BlueprintAbility Parent
	{
		get
		{
			return m_Parent?.Get();
		}
		set
		{
			m_Parent = value.ToReference<BlueprintAbilityReference>();
		}
	}

	public bool IsWeaponAbility => AbilityParamsSource == WarhammerAbilityParamsSource.Weapon;

	public bool IsPsykerAbility => AbilityParamsSource == WarhammerAbilityParamsSource.PsychicPower;

	public bool IsGrenade => AbilityTag == AbilityTag.ThrowingGrenade;

	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettings => m_FXSettings;

	public AttackAnimationType AttackAnimationType => m_AttackAnimationType;

	public ReferenceArrayProxy<BlueprintAbilityGroup> AbilityGroups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] abilityGroups = m_AbilityGroups;
			return abilityGroups;
		}
	}

	[UsedImplicitly]
	public bool IsRangeCustom => Range == AbilityRange.Custom;

	public bool IsRangeWeapon => Range == AbilityRange.Weapon;

	[UsedImplicitly]
	private bool IsRangePersonal => Range == AbilityRange.Personal;

	public AbilityAttackType? AttackType => base.ComponentsArray.FirstItemOfType<AbilityAttackDelivery>()?.AttackType;

	public bool IsRangedAoe => base.ComponentsArray.HasItem((BlueprintComponent i) => i is AbilityAttackDelivery abilityAttackDelivery && abilityAttackDelivery.IsRangedAoe);

	public bool IsCoverIgnore => base.ComponentsArray.HasItem((BlueprintComponent i) => i is AbilityAttackDelivery abilityAttackDelivery && !abilityAttackDelivery.IsRespectCover);

	public bool CanCastToDeadTarget => this.GetComponent<ICanTargetDeadUnits>() != null;

	public IAbilityAoEPatternProvider PatternSettings => GetPatternSettings();

	public IAbilityRestriction[] Restrictions
	{
		get
		{
			if (m_CachedRestrictions == null)
			{
				m_CachedRestrictions = this.GetComponents<IAbilityRestriction>().ToArray();
			}
			return m_CachedRestrictions;
		}
	}

	public IAbilityTargetRestriction[] TargetRestrictions
	{
		get
		{
			if (m_CachedTargetRestrictions == null)
			{
				m_CachedTargetRestrictions = this.GetComponents<IAbilityTargetRestriction>().ToArray();
			}
			return m_CachedTargetRestrictions;
		}
	}

	public IAbilityCasterRestriction[] CasterRestrictions
	{
		get
		{
			if (m_CachedCasterRestrictions == null)
			{
				m_CachedCasterRestrictions = this.GetComponents<IAbilityCasterRestriction>().ToArray();
			}
			return m_CachedCasterRestrictions;
		}
	}

	public IAbilityCanTargetPointRestriction[] CanTargetPointRestrictions
	{
		get
		{
			if (m_CachedCanTargetPointRestrictions == null)
			{
				m_CachedCanTargetPointRestrictions = this.GetComponents<IAbilityCanTargetPointRestriction>().ToArray();
			}
			return m_CachedCanTargetPointRestrictions;
		}
	}

	public TargetType AoETargets => PatternSettings?.Targets ?? TargetType.Any;

	public bool HasVariants => this.GetComponent<AbilityVariants>();

	public bool IsSpell => Type == AbilityType.Spell;

	public bool IsAoE
	{
		get
		{
			if (this.GetComponent<AbilityTargetsInPattern>() == null && this.GetComponent<AbilityTargetsInCohesionRange>() == null)
			{
				IAbilityAttackTypeProvider component = this.GetComponent<IAbilityAttackTypeProvider>();
				if (component == null || !component.IsAoe)
				{
					AbilityEffectRunAction component2 = this.GetComponent<AbilityEffectRunAction>();
					if (component2 != null)
					{
						ActionList actions = component2.Actions;
						if (actions != null)
						{
							GameAction[] actions2 = actions.Actions;
							if (actions2 != null && actions2.TryFind((GameAction x) => x is ContextActionSpawnAreaEffect, out var _))
							{
								return true;
							}
						}
					}
					return false;
				}
			}
			return true;
		}
	}

	public bool IsBurst => base.ComponentsArray.HasItem(delegate(BlueprintComponent i)
	{
		if (i is AbilityAttackDelivery abilityAttackDelivery)
		{
			if (abilityAttackDelivery.IsBurst)
			{
				goto IL_0026;
			}
		}
		else if (i is AbilityCutsceneAttack { IsBurst: not false })
		{
			goto IL_0026;
		}
		return false;
		IL_0026:
		return true;
	});

	public bool IsControlledBurst => base.ComponentsArray.HasItem((BlueprintComponent i) => i is AbilityAttackDelivery abilityAttackDelivery && abilityAttackDelivery.IsControlledBurst);

	public bool UseBestShootingPosition
	{
		get
		{
			if (!DisableBestShootingPosition && this.GetComponent<AbilityCustomDirectMovement>() == null)
			{
				AbilityAttackDelivery component = this.GetComponent<AbilityAttackDelivery>();
				return component == null || component.UseBestShootingPosition;
			}
			return false;
		}
	}

	public bool IsMoveUnit => this.GetComponent<AbilityCustomLogic>()?.IsMoveUnit ?? false;

	public bool IsCharge
	{
		get
		{
			AbilityCustomDirectMovement component = this.GetComponent<AbilityCustomDirectMovement>();
			if (component == null || !component.IsCharge)
			{
				return this.GetComponent<AbilityCustomMoveToTarget>();
			}
			return true;
		}
	}

	public bool IsDirectMovement => this.GetComponent<AbilityCustomDirectMovement>();

	public bool IsStratagem => m_IsStratagem;

	public CombatHudCommandSetAsset CombatHudCommandsOverride => m_CombatHudCommandsOverride;

	public bool IsSummoningUnit => false;

	public bool IsCustomProjectileDistribution => this.GetComponent<CustomProjectileDistribution>();

	[CanBeNull]
	public BlueprintAbility BaseAbility => this.GetComponent<UpgradeAbility>()?.BaseAbility;

	public bool CanTargetPointAfterRestrictions(AbilityData abilityData)
	{
		if (abilityData.CanTargetPoint)
		{
			return CanTargetPointRestrictions.ToList().All((IAbilityCanTargetPointRestriction checker) => checker.IsAbilityCanTargetPointRestrictionPassed(abilityData));
		}
		return false;
	}

	public int GetRange()
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

	public int GetVeilDamage()
	{
		if (!IsPsykerAbility)
		{
			return 0;
		}
		return VeilDamage;
	}

	protected override Type GetFactType()
	{
		return typeof(Ability);
	}

	public override MechanicEntityFact CreateFact(MechanicsContext parentContext, BuffDuration duration, int rank = 1)
	{
		return new Ability(this);
	}

	public bool HasVariant(BlueprintAbility other)
	{
		return this.GetComponent<AbilityVariants>()?.Variants.HasReference(other) ?? false;
	}

	public string[] GetResourceIds()
	{
		return ResourceAssetIds;
	}

	[BlueprintButton]
	[UsedImplicitly]
	public void FixParentForVariants()
	{
	}

	private IAbilityAoEPatternProvider GetPatternSettings()
	{
		IAbilityAoEPatternProvider abilityAoEPatternProvider = this.GetComponent<IAbilityAoEPatternProviderHolder>()?.PatternProvider ?? this.GetComponent<IAbilityAoEPatternProvider>();
		if (abilityAoEPatternProvider != null)
		{
			return abilityAoEPatternProvider;
		}
		foreach (Element item in base.ElementsArray)
		{
			if (item is ContextActionSpawnAreaEffect contextActionSpawnAreaEffect)
			{
				return contextActionSpawnAreaEffect.AreaEffect;
			}
		}
		return null;
	}

	public void Scan()
	{
	}

	public void Editor_SetFXSettings(BlueprintAbilityFXSettings fxSettings)
	{
	}
}
