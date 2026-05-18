using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Squads;

public sealed class SquadMovementPlan
{
	internal static readonly object PoolClaimer = new object();

	public readonly Dictionary<BaseUnitEntity, SquadMovementAssignment> Assignments = new Dictionary<BaseUnitEntity, SquadMovementAssignment>();

	public void ReleaseAll()
	{
		foreach (SquadMovementAssignment value in Assignments.Values)
		{
			value.Path?.Release(PoolClaimer);
		}
		Assignments.Clear();
	}
}
