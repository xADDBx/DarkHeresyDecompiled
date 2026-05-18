using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Framework;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Serializable]
[PlayerUpgraderAllowed(false)]
[TypeId("4aa7a911d4a94747a70a8146667daa02")]
public class CheckPropertySimple : Condition
{
	public PropertyCalculator Value;

	protected override string GetConditionCaption()
	{
		return $"Check property simple: {Value}";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity caster = EvalContext.Current.Caster;
		if (caster == null)
		{
			PFLog.Default.ErrorWithReport("CurrentEntity is missing");
			return false;
		}
		return Value.GetBoolValue(caster);
	}
}
