using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Obsolete]
[TypeId("1115bf7464c8a8242aedc07680b8705c")]
public class DodgePenetrationGetter : IntPropertyGetter
{
	public PropertyTargetType Target;

	public bool NoTarget;

	protected override int GetBaseValue()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (NoTarget)
		{
			return "Dodge Penetration of " + FormulaTargetScope.Current + " against abstract target";
		}
		return "Dodge Penetration of " + FormulaTargetScope.Current + " against " + Target.Colorized();
	}
}
