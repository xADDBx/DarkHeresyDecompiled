using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI.Mechanics.Positioning;

[Serializable]
[ComponentName("AI/Distance/AiPositioningDistanceToCustomTarget")]
[TypeId("d5cdf9128c3740c1b7132c6c5db74691")]
public class AiPositioningDistanceToCustomTarget : IntPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	[SerializeReference]
	[CanBeNull]
	public PositionEvaluator PositionEvaluator;

	protected override int GetBaseValue()
	{
		MechanicEntity targetByType = this.GetTargetByType(Target);
		Vector3 value = PositionEvaluator.GetValue();
		return targetByType.DistanceToInCells(value);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Distance from {Target} to {PositionEvaluator.NameSafe()}";
	}
}
