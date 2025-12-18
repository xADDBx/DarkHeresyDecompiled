using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("c5b3b6002404ca249add2943e99f366a")]
public class ContextConditionIsAlly : ContextCondition
{
	public bool ExcludeUnitItself;

	protected override string GetConditionCaption()
	{
		if (!ExcludeUnitItself)
		{
			return "Is target ally to caster";
		}
		return "Is target ally to caster and target not equal caster";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return false;
		}
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return false;
		}
		if (ExcludeUnitItself && entity == maybeCaster)
		{
			return false;
		}
		return maybeCaster.IsAlly(entity);
	}
}
