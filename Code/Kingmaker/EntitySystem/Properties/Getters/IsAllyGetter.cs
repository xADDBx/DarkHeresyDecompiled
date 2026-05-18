using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("b5a7d9bbf95591b49b2985d414d2e360")]
public class IsAllyGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		if (!(EvalContext.Current.GetEntityByType(Target) is BaseUnitEntity entity))
		{
			return false;
		}
		return base.CurrentEntity.IsAlly(entity);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is CurrentEntity ally to " + Target;
	}
}
