using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.ContextContract;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("b0c4370c781db0142b035f14ca13a6a5")]
[ReadsContext(new ContextField[]
{
	ContextField.Caster,
	ContextField.Target
})]
public class ContextConditionIsEnemy : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Check if target is an enemy";
	}

	protected override bool CheckCondition()
	{
		if (base.Eval.Caster == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
			return false;
		}
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return false;
		}
		return entity.IsEnemy(base.Eval.Caster);
	}
}
