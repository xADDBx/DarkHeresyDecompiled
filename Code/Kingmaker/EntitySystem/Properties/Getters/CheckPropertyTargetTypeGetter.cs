using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("976625711af845a4fa4b9c6cd63d5193")]
public class CheckPropertyTargetTypeGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalTargetByType, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check target [" + Target.Colorized() + "] equals " + FormulaTargetScope.Current;
	}

	protected override bool GetBaseValue()
	{
		return base.CurrentEntity == this.GetTargetByType(Target);
	}
}
