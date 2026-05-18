using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/Distance/AiPositioningNodeOccupiedGetter")]
[TypeId("19f00efb6e3a41a7819640e19e83a13f")]
public class AiPositioningNodeOccupiedGetter : IntPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		BaseUnitEntity baseUnitEntity = base.CurrentEntity as BaseUnitEntity;
		BaseUnitEntity firstUnit = (AiPositioningData.CurrentNode as GridNodeBase).GetFirstUnit();
		if (firstUnit == null || firstUnit == baseUnitEntity)
		{
			return 1;
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "If node occupied by someone else but current entity it returns 0. Otherwise returns 1.";
	}
}
