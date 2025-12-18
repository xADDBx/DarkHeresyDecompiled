using System;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Mechanics;
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
		if (!SimpleContextData<PropertyContext, PropertyContext.Scope>.TryGetCurrent(out var result))
		{
			MechanicEntity mechanicEntity = SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current?.MaybeCaster;
			if (mechanicEntity == null)
			{
				PFLog.Default.ErrorWithReport("CurrentEntity is missing");
				return false;
			}
			result = new PropertyContext(mechanicEntity);
		}
		return Value.GetBoolValue(result);
	}
}
