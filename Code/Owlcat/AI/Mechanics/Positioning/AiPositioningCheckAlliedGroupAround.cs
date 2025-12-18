using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Pathfinding;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/AiPositioningCheckAlliedGroupAround")]
[TypeId("7dbf827e5d9146dbbcef491af6e62880")]
public class AiPositioningCheckAlliedGroupAround : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return Target.Colorized() + " will be connected with 2 or more allies if he stands in <color='purple'>graph node</color>";
	}

	protected override bool GetBaseValue()
	{
		GridNodeBase node = AiPositioningData.CurrentNode as GridNodeBase;
		IntRect currentSizeRect = AiPositioningData.CurrentSizeRect;
		MechanicEntity targetByType = this.GetTargetByType(Target);
		IEnumerable<GridNodeBase> nodesSpiralAround = GridAreaHelper.GetNodesSpiralAround(node, currentSizeRect, 1);
		MechanicEntity mechanicEntity = null;
		foreach (GridNodeBase item in nodesSpiralAround)
		{
			BaseUnitEntity firstUnit = item.GetFirstUnit();
			if (firstUnit != null && firstUnit != targetByType && firstUnit != mechanicEntity && firstUnit.IsAlly(targetByType))
			{
				if (mechanicEntity != null)
				{
					return true;
				}
				mechanicEntity = firstUnit;
			}
		}
		foreach (GridNodeBase item2 in GridAreaHelper.GetNodesSpiralAround(mechanicEntity.CurrentNode.node as GridNodeBase, mechanicEntity.SizeRect, 1))
		{
			BaseUnitEntity firstUnit2 = item2.GetFirstUnit();
			if (firstUnit2 != null && firstUnit2 != targetByType && firstUnit2 != mechanicEntity && firstUnit2.IsAlly(targetByType))
			{
				return true;
			}
		}
		return false;
	}
}
