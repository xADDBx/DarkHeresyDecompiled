using System;
using System.Linq;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[TypeId("25dc242e781f412194221e4670edb564")]
public class GraphNodeConditionsList : GraphNodeCondition
{
	public Operation Operation;

	[SerializeReference]
	public GraphNodeCondition[] Conditions = new GraphNodeCondition[0];

	public override string GetCaption()
	{
		return "Is " + ((Operation == Operation.And) ? "all" : "any of") + " nodes satisfy conditions";
	}

	public override bool Check(GraphNode node)
	{
		if (Operation != 0)
		{
			return Conditions.Any((GraphNodeCondition cond) => cond.Check(node));
		}
		return Conditions.All((GraphNodeCondition cond) => cond.Check(node));
	}
}
