using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.Framework.ContextContract;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("71959818449d4891939b89c19a6d9a91")]
[ReadsContext(new ContextField[]
{
	ContextField.Caster,
	ContextField.Target
})]
public class ContextValueGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public ContextValue Value;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return Value.ToString();
	}

	protected override int GetBaseValue()
	{
		return Value.Calculate(EvalContext.Current);
	}
}
