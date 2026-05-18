using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Utility;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using UnityEngine.Pool;

namespace Kingmaker.Framework.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintAbilityModifier))]
[ComponentName("Ability/AbilityAdditionalTargets")]
[TypeId("0e83feedb83e433c963e2db12dcc6f08")]
public sealed class AbilityAdditionalTargets : BlueprintComponent
{
	public AdditionalTargetsSelectionStrategy SelectionStrategy;

	public ContextValue Range = new ContextValue();

	public bool LimitCount;

	[ShowIf("LimitCount")]
	public ContextValue Count = new ContextValue();

	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public RestrictionCalculator TargetRestriction = new RestrictionCalculator();

	public IEnumerable<AbilityDeliveryTarget> GetTargets(IEvalContext context)
	{
		if (!Restriction.IsPassed(context))
		{
			return Enumerable.Empty<AbilityDeliveryTarget>();
		}
		TargetWrapper clickedTarget = context.ClickedTarget;
		Cells range = Range.Calculate(context).Cells();
		int targetsCountLimit = (LimitCount ? Count.Calculate(context) : int.MaxValue);
		return SelectionStrategy switch
		{
			AdditionalTargetsSelectionStrategy.Around => SelectTargetsAround(context, clickedTarget, range, targetsCountLimit), 
			AdditionalTargetsSelectionStrategy.Chain => SelectTargetsChain(context, clickedTarget, range, targetsCountLimit), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public IEnumerable<AbilityDeliveryTarget> SelectTargetsAround(IEvalContext context, TargetWrapper anchor, Cells range, int targetsCountLimit)
	{
		List<BaseUnitEntity> list = EntityBoundsHelper.FindUnitsInRange(anchor.Point, Range.Calculate(context));
		list.Shuffle(PFStatefulRandom.Mechanics);
		int selectedTargetsCount = 0;
		foreach (BaseUnitEntity item in list)
		{
			if (item != context.ClickedTarget?.Entity && IsValidTarget(context, item))
			{
				yield return new AbilityDeliveryTarget(item);
				int num = selectedTargetsCount + 1;
				selectedTargetsCount = num;
				if (num >= targetsCountLimit)
				{
					break;
				}
			}
		}
	}

	public IEnumerable<AbilityDeliveryTarget> SelectTargetsChain(IEvalContext context, TargetWrapper anchor, Cells range, int targetsCountLimit)
	{
		HashSet<MechanicEntity> selectedTargets;
		using (CollectionPool<HashSet<MechanicEntity>, MechanicEntity>.Get(out selectedTargets))
		{
			int selectedTargetsCount = 0;
			while (selectedTargetsCount < targetsCountLimit)
			{
				MechanicEntity nextChainTarget = GetNextChainTarget(context, anchor, range, selectedTargets);
				if (nextChainTarget != null)
				{
					selectedTargets.Add(nextChainTarget);
					anchor = nextChainTarget;
					selectedTargetsCount++;
					yield return new AbilityDeliveryTarget(nextChainTarget);
					continue;
				}
				break;
			}
		}
	}

	[CanBeNull]
	public MechanicEntity GetNextChainTarget(IEvalContext context, TargetWrapper anchor, Cells range, HashSet<MechanicEntity> selectedTargets)
	{
		List<BaseUnitEntity> list = EntityBoundsHelper.FindUnitsInRange(anchor.Point, range.Meters);
		list.Shuffle(PFStatefulRandom.Mechanics);
		foreach (BaseUnitEntity item in list)
		{
			if (item != context.ClickedTarget?.Entity && !selectedTargets.Contains(item) && IsValidTarget(context, item))
			{
				return item;
			}
		}
		return null;
	}

	private bool IsValidTarget(IEvalContext context, MechanicEntity target)
	{
		AbilityData ability = context.Ability;
		if (ability == null || !ability.IsValidTargetForAttack(target))
		{
			return false;
		}
		if (!ability.Blueprint.CanTargetEnemies && context.Caster.IsEnemy(target))
		{
			return false;
		}
		if (!ability.Blueprint.CanTargetFriends && context.Caster.IsAlly(target))
		{
			return false;
		}
		if (!TargetRestriction.IsPassed(context, target))
		{
			return false;
		}
		return true;
	}
}
