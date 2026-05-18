using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Owlcat.AI;
using Pathfinding;

namespace Kingmaker.UnitLogic.Squads.Goals;

public sealed class MeleeAttackGoal : IMovementGoal
{
	private readonly MechanicEntity m_Target;

	private readonly PreferredPositionNearEntity m_PreferredPositionNearEntity;

	public MeleeAttackGoal(MechanicEntity target, PreferredPositionNearEntity preferredPositionNearEntity)
	{
		m_Target = target;
		m_PreferredPositionNearEntity = preferredPositionNearEntity;
	}

	public IReadOnlyList<SquadCandidateCell> GetCandidates(UnitSquad squad)
	{
		if (m_Target == null || squad == null)
		{
			return Array.Empty<SquadCandidateCell>();
		}
		BaseUnitEntity baseUnitEntity = null;
		foreach (UnitReference unit in squad.Units)
		{
			BaseUnitEntity baseUnitEntity2 = unit.ToBaseUnitEntity();
			if (baseUnitEntity2 != null && !baseUnitEntity2.IsDeadOrUnconscious)
			{
				if (baseUnitEntity == null)
				{
					baseUnitEntity = baseUnitEntity2;
				}
				else if (!baseUnitEntity2.SizeRect.Equals(baseUnitEntity.SizeRect))
				{
					PFLog.AI.Error("Squad " + squad.Id + " has heterogeneous SizeRect units; MeleeAttackGoal assumes homogeneous squads. " + $"Falling back to reference unit ({baseUnitEntity}) size ({baseUnitEntity.SizeRect}); " + $"mismatched unit {baseUnitEntity2} has {baseUnitEntity2.SizeRect}.");
					break;
				}
			}
		}
		if (baseUnitEntity == null)
		{
			return Array.Empty<SquadCandidateCell>();
		}
		HashSet<GridNodeBase> nodesNearTargetEntity = MeleeCandidateHelper.GetNodesNearTargetEntity(baseUnitEntity, m_Target, m_PreferredPositionNearEntity);
		if (nodesNearTargetEntity.Count == 0)
		{
			return Array.Empty<SquadCandidateCell>();
		}
		List<SquadCandidateCell> list = new List<SquadCandidateCell>(nodesNearTargetEntity.Count);
		foreach (GridNodeBase item in nodesNearTargetEntity)
		{
			list.Add(new SquadCandidateCell(item, 1f));
		}
		return list;
	}
}
