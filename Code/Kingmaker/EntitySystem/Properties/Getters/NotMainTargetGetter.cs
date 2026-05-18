using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("39d48af336aa4676a18d4565af334343")]
public class NotMainTargetGetter : BoolPropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override bool GetBaseValue()
	{
		return EvalContext.Current.Rule?.Reason.Context?.ClickedTarget != base.CurrentEntity;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "1 if " + FormulaTargetScope.Current + " is not main target";
	}
}
