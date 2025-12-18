using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("c15bf6ee4acc98d44bf69a967df4bc2f")]
public class CheckEntityIsTarget : BoolPropertyGetter, PropertyContextAccessor.IOptionalFact, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		return base.PropertyContext.GetTargetEntity(PropertyTargetType.CurrentTarget) == base.CurrentEntity;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "CurrentTarget is the same as CurrentEntity";
	}
}
