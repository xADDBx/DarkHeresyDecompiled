using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("d271cb15605a459ca6188e939a2d5d72")]
public class ContextConditionProperty : ContextCondition
{
	public PropertyCalculator Property;

	public bool NegativeDoesNotCount;

	protected override string GetConditionCaption()
	{
		return Property.ToString();
	}

	protected override bool CheckCondition()
	{
		MechanicEntity mechanicEntity = base.Target.Entity ?? base.Eval.Caster;
		if (mechanicEntity == null)
		{
			PFLog.Default.ErrorWithReport("CurrentEntity is missing");
			return false;
		}
		int value = Property.GetValue(mechanicEntity, base.Eval);
		if (!NegativeDoesNotCount)
		{
			return value != 0;
		}
		return value > 0;
	}
}
