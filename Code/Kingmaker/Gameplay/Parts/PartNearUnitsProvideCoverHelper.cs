using Kingmaker.EntitySystem.Entities;
using Pathfinding;

namespace Kingmaker.Gameplay.Parts;

public static class PartNearUnitsProvideCoverHelper
{
	public static bool IsNodeCoveredByEntity(this GridNodeBase node, int direction, out MechanicEntity coverEntity)
	{
		GridNodeBase neighbourAlongDirection = node.GetNeighbourAlongDirection(direction, checkConnectivity: false);
		BaseUnitEntity firstUnit = node.GetFirstUnit();
		if (firstUnit != null)
		{
			BaseUnitEntity baseUnitEntity = neighbourAlongDirection?.GetFirstUnit();
			if (baseUnitEntity != null)
			{
				PartNearUnitsProvideCover optional = firstUnit.GetOptional<PartNearUnitsProvideCover>();
				if (optional != null && optional.IsSuitableCover(baseUnitEntity))
				{
					coverEntity = baseUnitEntity;
					return true;
				}
			}
		}
		coverEntity = null;
		return false;
	}
}
