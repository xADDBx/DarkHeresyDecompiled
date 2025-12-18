using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete("VS")]
[TypeId("faf78136b4a0688418f1964347209313")]
public class CheckAssassinOpeningGetter : IntPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Hit an Assassin Opening on " + Target.Colorized();
	}

	protected override int GetBaseValue()
	{
		return 0;
	}
}
