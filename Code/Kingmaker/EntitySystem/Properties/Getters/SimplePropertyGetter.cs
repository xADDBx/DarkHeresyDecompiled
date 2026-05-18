using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("fd03f97ef3d1416d9a95f874454886d9")]
public class SimplePropertyGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public ContextProperty Property;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"${Property} of {FormulaTargetScope.Current}";
	}

	protected override int GetBaseValue()
	{
		return Property.GetValue(base.CurrentEntity, EvalContext.Current);
	}
}
