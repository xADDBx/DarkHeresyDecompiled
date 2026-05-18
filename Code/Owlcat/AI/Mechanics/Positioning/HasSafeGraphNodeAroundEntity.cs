using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Pathfinding;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/Movement/HasSafeGraphNodeAroundEntity")]
[TypeId("20e94f72ab8d5c047a7f510273f32901")]
public class HasSafeGraphNodeAroundEntity : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Has safe graph node around " + Target.Colorized() + " (no area effects)";
	}

	protected override bool GetBaseValue()
	{
		MechanicEntity targetByType = this.GetTargetByType(Target);
		if (targetByType == null)
		{
			return false;
		}
		if (!(base.CurrentEntity is BaseUnitEntity unit))
		{
			return false;
		}
		if (!(targetByType.CurrentNode.node is GridNodeBase node))
		{
			return false;
		}
		IntRect sizeRect = targetByType.SizeRect;
		foreach (GridNodeBase item in GridAreaHelper.GetNodesSpiralAround(node, sizeRect, 1))
		{
			if (AiBrainHelper.GetThreatsData(unit, item).AreaEffects.Count == 0)
			{
				return true;
			}
		}
		return false;
	}
}
