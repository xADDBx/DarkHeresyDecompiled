using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("2823df2bc8754a409cb9d0340ff6a379")]
public class IsBaseUnitEntityGetter : BoolPropertyGetter
{
	protected override bool GetBaseValue()
	{
		return base.CurrentEntity is BaseUnitEntity;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " is base unit entity";
	}
}
