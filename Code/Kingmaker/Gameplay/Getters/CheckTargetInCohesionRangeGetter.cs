using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Gameplay.Features.Cohesion;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters;

[Serializable]
[ComponentName("Cohesion/CheckTargetInCohesionRangeGetter")]
[TypeId("9a931ecdd36143ef923f87661ab7495e")]
public sealed class CheckTargetInCohesionRangeGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target = PropertyTargetType.CurrentTarget;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return Target.Colorized() + " is in cohesion range of " + FormulaTargetScope.Current;
	}

	protected override bool GetBaseValue()
	{
		return base.CurrentEntity.GetOptional<PartCohesion>()?.ContainsInRange(this.GetTargetByType(Target)) ?? false;
	}
}
