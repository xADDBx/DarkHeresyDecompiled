using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/Distance/AiPositioningDistanceGetter")]
[TypeId("826daf5acba547fea2e81aaf2a93125f")]
public class AiPositioningDistanceGetter : IntPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		MechanicEntity targetByType = this.GetTargetByType(Target);
		GraphNode currentNode = AiPositioningData.CurrentNode;
		return targetByType.DistanceToInCells(currentNode.Vector3Position());
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Distance from <color='purple'>graph node</color> to " + Target.Colorized();
	}
}
