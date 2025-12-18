using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[TypeId("1b2ece1acd7a47b2adb72f88a9ae63ae")]
public abstract class GraphNodeCondition : Element
{
	public abstract bool Check(GraphNode node);
}
