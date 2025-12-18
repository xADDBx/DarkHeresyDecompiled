using System;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Gameplay.Features.Scaling.Utility;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Scaling.Getters;

[Serializable]
[TypeId("f2b66c60717549ee938d92ee998e59db")]
public sealed class FactScalingGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalFact, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Fact Scaling";
	}

	protected override int GetBaseValue()
	{
		MechanicEntityFact fact = this.GetFact();
		ScalingInfo? scalingInfo = fact?.GetScaling();
		if (!scalingInfo.HasValue)
		{
			return 0;
		}
		PropertyContext context = new PropertyContext(fact.Owner, fact.Context);
		return scalingInfo.Value.Calculator.GetValue(context);
	}
}
