using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Obsolete]
[TypeId("e2bd702f0c6ba9349a821e41872f2f16")]
public class ContextConditionTargetIsSameAsContextTarget : ContextCondition
{
	protected override bool CheckCondition()
	{
		if (base.Eval.ClickedTarget?.Entity == null)
		{
			PFLog.Default.Error("Context target unit is missing");
			return false;
		}
		if (base.Target.Entity == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return false;
		}
		return base.Target.Entity.Blueprint == base.Eval.ClickedTarget?.Entity?.Blueprint;
	}

	protected override string GetConditionCaption()
	{
		return "Unit's blueprint has the same LocalizedName as context target's";
	}
}
