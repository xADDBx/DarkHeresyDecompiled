using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("b742461a99f449458cd0df048e50c1fe")]
public class ContextConditionGetter : BoolPropertyGetter, PropertyContextAccessor.IMechanicContext, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public ConditionsChecker Conditions;

	protected override bool GetBaseValue()
	{
		using (EvalContext.Current.PushTarget(base.CurrentEntity))
		{
			return Conditions.Check();
		}
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (Conditions.Conditions == null || Conditions.Conditions.Length < 1)
		{
			return "ContextConditionGetter";
		}
		if (Conditions.Conditions.Length < 2)
		{
			return Conditions.Conditions[0].GetCaption();
		}
		return $"ContextConditionGetter({Conditions.Conditions.Length})";
	}
}
