using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.QA;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Blueprints;

[TypeId("4e19ee98b71c98b40ba235cfa715b760")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintAreaEffect : BlueprintMechanicEntityFact, IAbilityAoEPatternProvider, IResourcesHolder
{
	[SerializeField]
	private bool m_AllowNonContextActions;

	public TargetType TargetType;

	public bool AffectEnemies;

	public bool AffectDead;

	public bool AffectDestructibleObjects;

	public bool AggroEnemies = true;

	[SerializeField]
	private AreaEffectRestrictions m_AreaEffectRestrictions = AreaEffectRestrictions.None;

	public bool IsAllArea;

	public bool OnlyInCombat;

	[Tooltip("Debug visualization in AR grid")]
	public bool SavePersistentArea;

	public Texture2D PersistentAreaTexture2D;

	public CombatHudMaterialRemapAsset PersistentAreaMaterialRemap;

	public bool IsStrategistAbility;

	[ShowIf("CanChooseStrategistTacticsAbilityType")]
	public StrategistTacticsAreaEffectType TacticsAreaEffectType;

	[SerializeField]
	[HideIf("IsAllArea")]
	private AoEPattern m_Pattern;

	[HideIf("IsAllArea")]
	public bool IgnoreLosWhenSpread;

	[HideIf("IsAllArea")]
	public bool IgnoreLevelDifferenceWhenSpread;

	public bool DontNeedView;

	[HideIf("DontNeedView")]
	public bool ScrollCameraToAreaEffectWhenEnded;

	[HideIf("DontNeedView")]
	[SerializeField]
	private BlueprintAbilityFXSettings.Reference m_FXSettings;

	private AreaEffectClusterComponent m_ClusterComponent;

	public AoEPattern Pattern
	{
		get
		{
			if (!IsAllArea)
			{
				return m_Pattern;
			}
			return null;
		}
	}

	public int? HaloSize => null;

	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettings => m_FXSettings;

	public bool CanTargetEnemies => TargetType != TargetType.Ally;

	public bool CanTargetAllies => TargetType != TargetType.Enemy;

	private bool CanChooseStrategistTacticsAbilityType => IsStrategistAbility;

	public override bool AllowContextActionsOnly => !m_AllowNonContextActions;

	public AreaEffectRestrictions AreaEffectRestrictions => m_AreaEffectRestrictions;

	public bool HasConcussionEffect => HasFlag(AreaEffectRestrictions.CanOnlyUseWeaponAbilities);

	public bool HasCantAttackEffect => HasFlag(AreaEffectRestrictions.CannotUseWeaponAbilities);

	public bool HasInertWarpEffect => HasFlag(AreaEffectRestrictions.CannotUsePsychicPowers);

	private bool SearchedForCusterComponent { get; set; }

	public AreaEffectClusterComponent ClusterComponent
	{
		get
		{
			if (m_ClusterComponent == null)
			{
				if (SearchedForCusterComponent)
				{
					return null;
				}
				IEnumerable<AreaEffectClusterComponent> source = base.ComponentsArray.OfType<AreaEffectClusterComponent>();
				m_ClusterComponent = source.FirstOrDefault();
				SearchedForCusterComponent = true;
				return m_ClusterComponent;
			}
			return m_ClusterComponent;
		}
	}

	bool IAbilityAoEPatternProvider.IsIgnoreLos => IgnoreLosWhenSpread;

	public bool UseMeleeLos => false;

	bool IAbilityAoEPatternProvider.IsIgnoreLevelDifference => IgnoreLevelDifferenceWhenSpread;

	int IAbilityAoEPatternProvider.PatternAngle => 0;

	bool IAbilityAoEPatternProvider.CalculateAttackFromPatternCentre => true;

	TargetType IAbilityAoEPatternProvider.Targets => TargetType;

	private BlueprintComponent[] TryOverrideComponentsArray()
	{
		if (ClusterComponent != null)
		{
			return ClusterComponent.ClusterLogicBlueprint.ComponentsArray;
		}
		return base.ComponentsArray;
	}

	public void HandleEntityEnter(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AreaEffectLogic)?.HandleEntityEnter(context, areaEffect, entity);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	public void HandleEntityExit(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AreaEffectLogic)?.HandleEntityExit(context, areaEffect, entity);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	public void HandleEntityMove(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AreaEffectLogic)?.HandleEntityMove(context, areaEffect, entity);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	public void HandleUnitStartTurn(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AreaEffectLogic)?.HandleEntityTurnStart(context, areaEffect, entity);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	public void HandleUnitEndTurn(IEvalContext context, AreaEffectEntity areaEffect, MechanicEntity entity)
	{
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AreaEffectLogic)?.HandleEntityTurnEnd(context, areaEffect, entity);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	public void HandleRound(IEvalContext context, AreaEffectEntity areaEffect)
	{
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AreaEffectLogic)?.HandleRound(context, areaEffect);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	public void HandleTick(IEvalContext context, AreaEffectEntity areaEffect)
	{
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AreaEffectLogic)?.HandleTick(context, areaEffect);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	[Obsolete]
	public void HandleSpawn(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AreaEffectSpawnLogic)?.HandleAreaEffectSpawn(context, areaEffect);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	public void HandleEnd(IEvalContext context, AreaEffectEntity areaEffect)
	{
		if (ScrollCameraToAreaEffectWhenEnded)
		{
			((ICameraFocusTarget)areaEffect).RetainCamera();
		}
		BlueprintComponent[] array = TryOverrideComponentsArray();
		foreach (BlueprintComponent blueprintComponent in array)
		{
			try
			{
				(blueprintComponent as AreaEffectLogic)?.HandleEnd(context, areaEffect);
			}
			catch (Exception exception)
			{
				PFLog.Default.ExceptionWithReport(exception, null);
			}
		}
	}

	private bool HasFlag(AreaEffectRestrictions flag)
	{
		return (AreaEffectRestrictions & flag) != 0;
	}

	public void Editor_SetFXSettings(BlueprintAbilityFXSettings fxSettings)
	{
	}

	public void OverrideHaloSize(int? haloSize)
	{
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize, bool coveredTargetsOnly)
	{
		GridNodeBase actualCastNode;
		if (Pattern != null)
		{
			return AoEPatternHelper.GetOrientedPattern(ability, ability.Caster, Pattern, this, casterNode, targetNode, castOnSameLevel: false, directional: false, coveredTargetsOnly, targetSize, out actualCastNode);
		}
		return OrientedPatternData.Empty;
	}

	public OrientedPatternData GetOrientedHaloPattern(IAbilityDataProviderForPattern ability, int haloSize, GridNodeBase casterNode, GridNodeBase targetNode, Size targetSize = Size.Medium, bool coveredTargetsOnly = false)
	{
		return default(OrientedPatternData);
	}
}
