using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Mechanics;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Gameplay.Features.Cohesion.Actions;

[Serializable]
[TypeId("4a25bd8df2ad48018bdb056095ca729a")]
public sealed class ContextActionOnUnitsInCohesionRange : ContextAction
{
	public enum TargetSelection
	{
		All,
		Random,
		Closest,
		Farthest
	}

	[Tooltip("CurrentEntity - юнит в Cohesion Range, которого проверяем")]
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	[InfoBox("Фильтр применяется относительно владельца Cohesion, не Caster'а")]
	public TargetType Filter;

	public TargetSelection TargetStrategy;

	public ActionList Actions = new ActionList();

	public override string GetCaption()
	{
		string text = TargetStrategy switch
		{
			TargetSelection.All => "", 
			TargetSelection.Random => " random", 
			TargetSelection.Closest => " closest", 
			TargetSelection.Farthest => " farthest", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		string text2 = Filter switch
		{
			TargetType.Enemy => " enemy", 
			TargetType.Ally => " ally", 
			TargetType.Any => "", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		string text3 = ((TargetStrategy == TargetSelection.All) ? "units" : "unit");
		return "Actions on" + text + text2 + " " + text3 + " in cohesion range";
	}

	protected override void RunAction()
	{
		PartCohesion partCohesion = base.Target.Entity?.GetOptional<PartCohesion>();
		if (partCohesion == null)
		{
			return;
		}
		List<UnitEntity> value;
		using (CollectionPool<List<UnitEntity>, UnitEntity>.Get(out value))
		{
			SelectTargets(partCohesion.UnitsInRange.Where(IsSuitable), value);
			foreach (UnitEntity item in value)
			{
				Actions.RunWithTarget(item);
			}
		}
	}

	private void SelectTargets(IEnumerable<UnitEntity> possibleTargets, List<UnitEntity> targets)
	{
		switch (TargetStrategy)
		{
		case TargetSelection.All:
			targets.AddRange(possibleTargets);
			break;
		case TargetSelection.Random:
		{
			UnitEntity unitEntity2 = possibleTargets.Random(PFStatefulRandom.Action);
			if (unitEntity2 != null)
			{
				targets.Add(unitEntity2);
			}
			break;
		}
		case TargetSelection.Closest:
		{
			UnitEntity unitEntity3 = possibleTargets.OrderBy(DistanceToCaster).FirstOrDefault();
			if (unitEntity3 != null)
			{
				targets.Add(unitEntity3);
			}
			break;
		}
		case TargetSelection.Farthest:
		{
			UnitEntity unitEntity = possibleTargets.OrderByDescending(DistanceToCaster).FirstOrDefault();
			if (unitEntity != null)
			{
				targets.Add(unitEntity);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private int DistanceToCaster(UnitEntity unit)
	{
		return unit.DistanceToInCells(base.Caster);
	}

	private bool IsSuitable(MechanicEntity unit)
	{
		bool flag = base.Target.Entity != null;
		if (flag)
		{
			flag = Filter switch
			{
				TargetType.Enemy => base.Target.Entity.IsEnemy(unit), 
				TargetType.Ally => base.Target.Entity.IsAlly(unit), 
				TargetType.Any => true, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		if (flag)
		{
			return Restriction.IsPassed(base.Context, unit);
		}
		return false;
	}
}
