using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.PatternAttack;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Abilities.Visual.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.FX;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[ComponentName("Ability/AbilityDeliverBeamChain")]
[TypeId("bf32645714054eb4f84983eb687a2120")]
public class AbilityDeliverBeamChain : AbilityDeliverEffect, IAbilityAoEPatternProviderHolder
{
	public enum SourceSelectionMode
	{
		PreviousBeamFirstHit,
		PreviousBeamNearestToCaster,
		AnyInSearchRadius,
		ClosestToCaster
	}

	private const float SqrMinBeamDirectionLength = 0.0001f;

	public AbilityAoEPatternSettings PatternSettings;

	public ContextValue BeamsCount;

	public SourceSelectionMode SourceSelection;

	public bool SourceAllowDead;

	public TargetType SourceTargetType;

	public RestrictionCalculator SourceRestriction;

	public TargetType NextBeamTargetType;

	public RestrictionCalculator TargetRestriction;

	public bool TargetAllowDead;

	public ContextValue SearchRadius;

	public float DelayBetweenBeamsSeconds;

	IAbilityAoEPatternProvider IAbilityAoEPatternProviderHolder.PatternProvider => PatternSettings;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
			yield break;
		}
		int totalBeams = BeamsCount.Calculate(context);
		if (totalBeams <= 0)
		{
			yield break;
		}
		if (!(target.Entity is BaseUnitEntity baseUnitEntity))
		{
			PFLog.Default.Error(this, "AbilityDeliverBeamChain requires an entity click target (beam 1 endpoint)");
			yield break;
		}
		int searchRadius = SearchRadius.Calculate(context);
		HashSet<MechanicEntity> visited;
		using (CollectionPool<HashSet<MechanicEntity>, MechanicEntity>.Get(out visited))
		{
			List<BaseUnitEntity> previousBeamHits;
			using (CollectionPool<List<BaseUnitEntity>, BaseUnitEntity>.Get(out previousBeamHits))
			{
				visited.Add(maybeCaster);
				MechanicEntity currentSource = maybeCaster;
				BaseUnitEntity baseUnitEntity2 = baseUnitEntity;
				for (int beamIndex = 0; beamIndex < totalBeams; beamIndex++)
				{
					GridNodeBase gridNodeBase = (GridNodeBase)currentSource.CurrentNode.node;
					GridNodeBase gridNodeBase2 = (GridNodeBase)baseUnitEntity2.CurrentNode.node;
					if (gridNodeBase == null || gridNodeBase2 == null)
					{
						break;
					}
					if (beamIndex > 0)
					{
						SpawnChainBeamStartVfx(context, currentSource, baseUnitEntity2);
					}
					OrientedPatternData pattern = PatternSettings.GetOrientedPattern(context.Ability, gridNodeBase, gridNodeBase2, currentSource.Size);
					previousBeamHits.Clear();
					foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
					{
						if (allBaseUnit != currentSource && AoEPatternHelper.WouldTargetEntity(pattern, allBaseUnit) && context.Ability.IsValidTargetForAttack(allBaseUnit) && PassesBeamFilters(context, allBaseUnit))
						{
							previousBeamHits.Add(allBaseUnit);
							visited.Add(allBaseUnit);
							yield return new AbilityDeliveryTarget(allBaseUnit);
						}
					}
					bool moreBeamsRemain = beamIndex < totalBeams - 1;
					if (moreBeamsRemain && DelayBetweenBeamsSeconds > 0f)
					{
						TimeSpan startTime = Game.Instance.Controllers.TimeController.GameTime;
						while (Game.Instance.Controllers.TimeController.GameTime - startTime < DelayBetweenBeamsSeconds.Seconds())
						{
							yield return null;
						}
					}
					if (!moreBeamsRemain)
					{
						break;
					}
					BaseUnitEntity baseUnitEntity3 = SelectNextSource(context, previousBeamHits, currentSource, searchRadius, visited);
					if (baseUnitEntity3 == null)
					{
						break;
					}
					BaseUnitEntity baseUnitEntity4 = SelectNextBeamEndpoint(context, baseUnitEntity3, searchRadius, visited);
					if (baseUnitEntity4 == null)
					{
						break;
					}
					currentSource = baseUnitEntity3;
					baseUnitEntity2 = baseUnitEntity4;
				}
			}
		}
	}

	private static void SpawnChainBeamStartVfx(AbilityExecutionContext context, MechanicEntity source, BaseUnitEntity endpoint)
	{
		BlueprintAbilityVisualFXSettings blueprintAbilityVisualFXSettings = context.Ability?.FXSettings?.VisualFXSettings;
		if (blueprintAbilityVisualFXSettings == null)
		{
			return;
		}
		IMechanicEntityView view = source.View;
		if (view == null)
		{
			return;
		}
		Vector3 forward = endpoint.Position - source.Position;
		if (forward.sqrMagnitude < 0.0001f)
		{
			return;
		}
		Quaternion value = Quaternion.LookRotation(forward, Vector3.up);
		GameObject gameObject = view.gameObject;
		IEnumerable<IFXSettings> fXs = blueprintAbilityVisualFXSettings.GetFXs(AbilityEventType.Start);
		if (fXs == null)
		{
			return;
		}
		foreach (IFXSettings item in fXs)
		{
			if (item?.Settings?.FXs == null)
			{
				continue;
			}
			VisualFXSettings[] fXs2 = item.Settings.FXs;
			for (int i = 0; i < fXs2.Length; i++)
			{
				GameObject gameObject2 = fXs2[i]?.Prefab?.Load();
				if (!(gameObject2 == null))
				{
					FxHelper.SpawnFxOnGameObject(gameObject2, gameObject, 1f, enableFxObject: true, value);
				}
			}
		}
	}

	private BaseUnitEntity SelectNextBeamEndpoint(AbilityExecutionContext context, MechanicEntity anchor, int searchRadius, HashSet<MechanicEntity> visited)
	{
		BaseUnitEntity result = null;
		float num = float.MaxValue;
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (allBaseUnit != anchor && !visited.Contains(allBaseUnit) && PassesBeamFilters(context, allBaseUnit))
			{
				float num2 = allBaseUnit.DistanceToInCells(anchor.Position);
				if (!(num2 > (float)searchRadius) && !(num2 >= num))
				{
					result = allBaseUnit;
					num = num2;
				}
			}
		}
		return result;
	}

	private BaseUnitEntity SelectNextSource(AbilityExecutionContext context, List<BaseUnitEntity> hitsThisBeam, MechanicEntity previousSource, int searchRadius, HashSet<MechanicEntity> visited)
	{
		switch (SourceSelection)
		{
		case SourceSelectionMode.PreviousBeamFirstHit:
			foreach (BaseUnitEntity item in hitsThisBeam)
			{
				if (PassesSourceFilters(context, item))
				{
					return item;
				}
			}
			return null;
		case SourceSelectionMode.PreviousBeamNearestToCaster:
		{
			MechanicEntity maybeCaster2 = context.MaybeCaster;
			if (maybeCaster2 == null)
			{
				return null;
			}
			BaseUnitEntity result2 = null;
			float num2 = float.MaxValue;
			{
				foreach (BaseUnitEntity item2 in hitsThisBeam)
				{
					if (PassesSourceFilters(context, item2))
					{
						float sqrMagnitude2 = (item2.Position - maybeCaster2.Position).sqrMagnitude;
						if (sqrMagnitude2 < num2)
						{
							result2 = item2;
							num2 = sqrMagnitude2;
						}
					}
				}
				return result2;
			}
		}
		case SourceSelectionMode.AnyInSearchRadius:
		{
			BaseUnitEntity result3 = null;
			float num3 = float.MaxValue;
			{
				foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
				{
					if (!visited.Contains(allBaseUnit) && PassesSourceFilters(context, allBaseUnit))
					{
						float num4 = allBaseUnit.DistanceToInCells(previousSource.Position);
						if (!(num4 > (float)searchRadius) && !(num4 >= num3))
						{
							result3 = allBaseUnit;
							num3 = num4;
						}
					}
				}
				return result3;
			}
		}
		case SourceSelectionMode.ClosestToCaster:
		{
			MechanicEntity maybeCaster = context.MaybeCaster;
			if (maybeCaster == null)
			{
				return null;
			}
			BaseUnitEntity result = null;
			float num = float.MaxValue;
			{
				foreach (BaseUnitEntity allBaseUnit2 in Game.Instance.EntityPools.AllBaseUnits)
				{
					if (!visited.Contains(allBaseUnit2) && PassesSourceFilters(context, allBaseUnit2) && allBaseUnit2.DistanceToInCells(previousSource.Position) <= searchRadius)
					{
						float sqrMagnitude = (allBaseUnit2.Position - maybeCaster.Position).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							result = allBaseUnit2;
							num = sqrMagnitude;
						}
					}
				}
				return result;
			}
		}
		default:
			return null;
		}
	}

	private bool PassesBeamFilters(AbilityExecutionContext context, BaseUnitEntity unit)
	{
		return PassesFilters(context, unit, NextBeamTargetType, TargetRestriction, TargetAllowDead);
	}

	private bool PassesSourceFilters(AbilityExecutionContext context, BaseUnitEntity unit)
	{
		return PassesFilters(context, unit, SourceTargetType, SourceRestriction, SourceAllowDead);
	}

	private static bool PassesFilters(AbilityExecutionContext context, BaseUnitEntity unit, TargetType targetType, RestrictionCalculator restriction, bool allowDead)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			return false;
		}
		if (!allowDead && unit.LifeState.IsDead)
		{
			return false;
		}
		if ((targetType == TargetType.Enemy && !maybeCaster.IsEnemy(unit)) || (targetType == TargetType.Ally && maybeCaster.IsEnemy(unit)))
		{
			return false;
		}
		if (restriction != null && !restriction.Empty)
		{
			using (EvalContext.PushContext(context, (TargetWrapper)unit.ToITargetWrapper()))
			{
				if (!restriction.IsPassed(context, unit))
				{
					return false;
				}
			}
		}
		return true;
	}
}
