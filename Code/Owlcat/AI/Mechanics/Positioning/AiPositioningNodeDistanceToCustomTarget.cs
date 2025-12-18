using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/Distance/AiPositioningNodeDistanceToCustomTarget")]
[TypeId("829c534a690c48e7aea6b060b092fdbd")]
public class AiPositioningNodeDistanceToCustomTarget : IntPropertyGetter
{
	[SerializeReference]
	[CanBeNull]
	public PositionEvaluator PositionEvaluator;

	protected override int GetBaseValue()
	{
		return WarhammerGeometryUtils.DistanceToInCells(AiPositioningData.CurrentNode.Vector3Position(), default(IntRect), PositionEvaluator.GetValue(), default(IntRect));
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Distance from <color='purple'>graph node</color> to " + PositionEvaluator?.NameSafe();
	}
}
