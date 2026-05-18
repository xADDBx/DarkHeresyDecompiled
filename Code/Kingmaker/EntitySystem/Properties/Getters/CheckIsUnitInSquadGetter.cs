using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Squads;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("7891081dede847f2a2a54825a3a8fdb7")]
public class CheckIsUnitInSquadGetter : BoolPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override bool GetBaseValue()
	{
		return EvalContext.Current.GetEntityByType(Target)?.GetOptional<PartSquad>() != null;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return Target.Colorized() + " is in squad";
	}
}
