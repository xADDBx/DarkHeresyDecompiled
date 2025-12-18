using System;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/AiPositioningCheckCoverGetter")]
[TypeId("c3a8b60b4e324bafbdbac229c264abc6")]
public class AiPositioningCheckCoverGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[Flags]
	public enum CoverType
	{
		NoCover = 1,
		Cover = 2,
		LosBlocker = 4
	}

	public PropertyTargetType Target;

	[EnumFlagsAsDropdown]
	public CoverType Cover;

	public bool Reverse;

	public bool IncludeCoverCorners;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!Reverse)
		{
			return $"<color='purple'>Graph node</color> has {Cover} against {Target.Colorized()}";
		}
		return $"{Target.Colorized()} has {Cover} against <color='purple'>graph node</color>";
	}

	protected override bool GetBaseValue()
	{
		Vector3 vector = AiPositioningData.CurrentNode.Vector3Position();
		IntRect currentSizeRect = AiPositioningData.CurrentSizeRect;
		Vector3 targetPositionByType = this.GetTargetPositionByType(Target);
		IntRect targetRectByType = this.GetTargetRectByType(Target);
		LosDescription cover = (Reverse ? LosCalculations.GetWarhammerLos(vector, currentSizeRect, targetPositionByType, targetRectByType) : LosCalculations.GetWarhammerLos(targetPositionByType, targetRectByType, vector, currentSizeRect));
		LosCalculations.CoverType coverType = cover.CoverType;
		if (((uint)Cover & ((coverType switch
		{
			LosCalculations.CoverType.Cover => true, 
			LosCalculations.CoverType.LosBlocker => true, 
			_ => true, 
		}) ? 1u : 0u)) != 0)
		{
			if (IncludeCoverCorners)
			{
				return true;
			}
			return GridAreaHelper.GetNodes(vector, currentSizeRect).Any((GridNodeBase node) => (node.CoordinatesInGrid - cover.ObstacleNode.CoordinatesInGrid).sqrMagnitude <= 1);
		}
		return false;
	}
}
