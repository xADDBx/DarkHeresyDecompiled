using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Features.Concentration;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("ca65a2c26b9f4e38b0a4690eb373d447")]
public class CheckConcentrationGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		return EvalContext.Current.GetEntityByType(Target)?.GetOptional<PartConcentration>() != null;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"{Target} is concentrating";
	}
}
