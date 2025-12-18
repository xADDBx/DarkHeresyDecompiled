using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Weakpoints;

[Serializable]
[TypeId("aeb49b8cc9eb4e22b430271badd0dfae")]
public sealed class CheckWeakpoint : BoolPropertyGetter, PropertyContextAccessor.IOptionalTargetByType, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target = PropertyTargetType.CurrentTarget;

	public bool OnlyWeakpointFromCurrentEntity;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " is looks on " + Target.Colorized() + " from weakpoint side";
	}

	protected override bool GetBaseValue()
	{
		if (!(this.GetTargetByType(Target) is MechanicEntity mechanicEntity))
		{
			return false;
		}
		PartWeakpoints optional = mechanicEntity.GetOptional<PartWeakpoints>();
		if (optional == null)
		{
			return false;
		}
		WeakpointSide closestSide = WeakpointSideSelector.GetClosestSide(base.CurrentEntity, mechanicEntity);
		if (!OnlyWeakpointFromCurrentEntity)
		{
			return optional.HasWeakpoint(closestSide);
		}
		return optional.HasWeakpoint(closestSide, base.CurrentEntity);
	}
}
