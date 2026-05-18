using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Pathfinding;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/Position Towards Position")]
[AllowMultipleComponents]
[TypeId("243d10d8d36f4abfb4acfedcfb820a44")]
public class PositionTowardsPosition : PositionEvaluator
{
	[SerializeField]
	[SerializeReference]
	public PositionEvaluator PointFrom;

	[SerializeField]
	[SerializeReference]
	public PositionEvaluator PointToward;

	public int Distance;

	protected override Vector3 GetValueInternal()
	{
		Vector3 value = PointFrom.GetValue();
		Vector3 value2 = PointToward.GetValue();
		GridNodeBase nearestNodeXZ = PointFrom.GetValue().GetNearestNodeXZ();
		GridNodeBase nearestNodeXZ2 = PointToward.GetValue().GetNearestNodeXZ();
		if (nearestNodeXZ.CellDistanceTo(nearestNodeXZ2) == 0)
		{
			return value + Vector3.forward * Distance;
		}
		return value + value.DirectionTo(value2) * Distance;
	}

	public override string GetCaption()
	{
		return "Position from one point towards the other";
	}
}
