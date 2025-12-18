using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Settings;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("6e86180ed2c369f449d9618b787d254b")]
public class IsRespecAllowed : Condition
{
	protected override string GetConditionCaption()
	{
		return "Is allowed Respec";
	}

	protected override bool CheckCondition()
	{
		return SettingsRoot.Difficulty.RespecAllowed;
	}
}
