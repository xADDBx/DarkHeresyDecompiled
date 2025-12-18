using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Pathfinding;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI.Mechanics.Positioning;

[ComponentName("AI/GraphNode/AiPositioningIsCurrentGraphNodeGetter")]
[TypeId("e1dc60cd4dbf4641b908a5a7c76cad66")]
public class AiPositioningIsCurrentGraphNodeGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		return this.GetTargetByType(PropertyTargetType.CurrentEntity)?.GetNearestNodeXZ() == AiPositioningData.CurrentNode;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is <color='purple'>graph node</color> equal CurrentEntity node?";
	}
}
