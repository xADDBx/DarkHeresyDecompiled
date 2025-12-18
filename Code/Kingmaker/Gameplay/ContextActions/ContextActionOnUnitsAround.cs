using System;
using System.Collections.Generic;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Mechanics;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using Pathfinding;
using UnityEngine.Pool;

namespace Kingmaker.Gameplay.ContextActions;

[Serializable]
[TypeId("6b5ecaee6ddc4e55829134a8da35dc61")]
public sealed class ContextActionOnUnitsAround : ContextAction
{
	[InfoBox("CurrentEntity - юнит, для которого хотим запустить Actions")]
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public TargetType Filter;

	public bool IncludeCaster;

	public ActionList Actions = new ActionList();

	public override string GetCaption()
	{
		return "Run actions on units around";
	}

	protected override void RunAction()
	{
		HashSet<MechanicEntity> value;
		using (CollectionPool<HashSet<MechanicEntity>, MechanicEntity>.Get(out value))
		{
			foreach (GridNodeBase item in GetNodesAround(base.TargetEntity))
			{
				foreach (BaseUnitEntity unit in item.GetUnits())
				{
					if (value.Add(unit) && IsSuitableTarget(unit))
					{
						Actions.RunWithTarget(unit);
					}
				}
			}
		}
	}

	private IEnumerable<GridNodeBase> GetNodesAround(MechanicEntity entity)
	{
		if (!(entity is DestructibleEntity { IsWall: not false }))
		{
			return GridAreaHelper.GetNodesSpiralAround(entity.CurrentUnwalkableNode, entity.SizeRect, 1);
		}
		return entity.GetOccupiedNodes();
	}

	private bool IsSuitableTarget(MechanicEntity target)
	{
		if (Filter switch
		{
			TargetType.Enemy => base.Caster.IsEnemy(target), 
			TargetType.Ally => base.Caster.IsAlly(target), 
			TargetType.Any => true, 
			_ => throw new ArgumentOutOfRangeException(), 
		} && (IncludeCaster || base.Caster != target))
		{
			return Restrictions.IsPassed(base.Context, target);
		}
		return false;
	}
}
