using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Cohesion;

namespace Kingmaker.Tutorial.Triggers;

internal static class TutorialEnemyCohesionQuery
{
	public static BaseUnitEntity FindEnemyOwner(Func<PartCohesion, bool> predicate)
	{
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (allBaseUnit.IsInCombat && allBaseUnit.Faction.IsPlayerEnemy)
			{
				PartCohesion optional = allBaseUnit.GetOptional<PartCohesion>();
				if (optional != null && predicate(optional))
				{
					return allBaseUnit;
				}
			}
		}
		return null;
	}
}
