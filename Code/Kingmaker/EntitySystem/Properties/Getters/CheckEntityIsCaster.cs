using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("9e76955a7d544abd89b5b968ee8f0957")]
public class CheckEntityIsCaster : BoolPropertyGetter, PropertyContextAccessor.IOptionalFact, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		return this.GetFact()?.MaybeContext?.MaybeCaster == base.CurrentEntity;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Fact caster is the same as the checked entity";
	}
}
